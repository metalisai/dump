using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.EventSystems;

[System.Serializable]
class GameSettings
{
    public Player.PlayerSettings PlayerSettings;

    public GameSettings()
    {
        PlayerSettings = new Player.PlayerSettings()
        {
            CameraMovementSpeed = 1.0f,
            CameraZoomSpeed = 1.0f
        };
    }
}

public class Game : MonoBehaviour
{
    public const float GRIDCELL_SIZE = 4.0f;
    public const float GRIDCELL_HALFSIZE = GRIDCELL_SIZE/2.0f;

    public enum GameMode{
        Normal,
        Editor,
        Test
    }

    public GameMode Mode;

    public static PathFinding pathFinding;
    public static Player localPlayer;

    public static MapEditor editor;

    public static ProjectileManager BulletManager;

    public Light Sun;
    public AnimationCurve SunHorizontalAngle;
    public AnimationCurve SunVerticalAngle;

    public Camera MainCamera;
    public GameObject Bullet;
    public GameObject Bomb;
    public GameObject UIGameObject;
    public GameObject ESPrefab;
    public Nexus Nexus;
    public Shader BuildingShader;
    public Texture2D LeftrightCursor;
    public Texture2D GrabCursor;

    public AudioClip ExplosionClip;
    public GameObject Explosion;

    public FogOfWar FogOfWar;
    public ItemSelector ItemSelector;

    public AudioClip PurchaseFailed;
    public AudioSource UIAudioSource;

    public static List<IResourceStore> ResourceStores = new List<IResourceStore>();
    public static CustomTerrain customTerrain;

    GameSettings gameSettings;
    public WireLogicManager WireManager;

    public static Game Instance;
    public static ResourceInfo CurrentResourceInfo;

	public Shader BeamShader;

    public ItemShop ItemShop = new ItemShop();

    public HashSet<GameObject> DynamicallyCreated = new HashSet<GameObject>();

    static Dictionary<GameObject, LinkedList<Action<GameObject>>> TrackedLifetime = new Dictionary<GameObject, LinkedList<Action<GameObject>>>();
    static Dictionary<ActiveObject.ObjectTag, LogicInfo> BehaviourLookup;
    static Dictionary<GameObject, bool> ManagedLifetime = new Dictionary<GameObject, bool>();

    public List<BaseAI> AiUnits = new List<BaseAI>();

	static bool _paused = false;

    public static void TrackLifetime(GameObject trackObject, Action<GameObject> onDestroy)
    {
        //Assert.IsTrue(DynamicallyCreated.Contains(gameObject), "Can't track lifetime of static object!");
        LinkedList<Action<GameObject>> ll;
        if (!TrackedLifetime.TryGetValue(trackObject, out ll))
        {
            ll = new LinkedList<Action<GameObject>>();
            TrackedLifetime.Add(trackObject, ll);
        }
        ll.AddFirst(onDestroy);
    }

    public static void StopTracking(GameObject trackObject, Action<GameObject> onDestroy)
    {
        LinkedList<Action<GameObject>> ll;
        if (TrackedLifetime.TryGetValue(trackObject, out ll))
        {
            bool result = ll.Remove(onDestroy);
            Assert.IsTrue(result, "Lifetimetrack remove failed");
        }
        else
        {
            Debug.LogError("StopTracking called for " + trackObject + " but not currently tracked!");
        }
    }

    public void RegisterDynamicObject(GameObject go, bool manageLifetime, bool scanComponents = true)
    {
        DynamicallyCreated.Add(go);
        ManagedLifetime.Add(go, manageLifetime);
        // TODO: maybe something more efficient?
        if (scanComponents)
        {
            var ai = go.GetComponent<BaseAI>();
            if (ai != null)
            {
                AiUnits.Add(ai);
            }
        }
    }

    public int GetResourceAmount(ResourceType resource)
    {
        return ResourceStores.Sum(x => x.GetResourceCount(resource));
    }

    public void DestroyDynamicObject(GameObject go, bool wasManaged = false)
    {
        if (ManagedLifetime.ContainsKey(go) && wasManaged != ManagedLifetime[go])
            Debug.LogError("Managed boolean on destroy didn't match creation one");
        List<Action<GameObject>> tempList = new List<Action<GameObject>>();

        foreach(var gameObj in go.GetChildrenRecursive())
        {
            LinkedList<Action<GameObject>> ll;
            if (TrackedLifetime.TryGetValue(gameObj, out ll))
            {
                foreach(var ac in ll)
                {
                    tempList.Add(ac);
                }
                foreach (var ac in tempList) // iterate a copy because the invocation might modify the linked list
                {
                    ac(gameObj);
                }
                tempList.Clear();
                TrackedLifetime.Remove(gameObj);
            }
        }

        // TODO: scanning every destroyed object will be horrible
        var ai = go.GetComponent<BaseAI>();
        if (ai != null)
            AiUnits.Remove(ai);
        var irs = go.GetComponent<IResourceStore>();
        if (irs != null)
            ResourceStores.Remove(irs);

        Assert.IsNotNull(go, "Cannot destroy null??");

        if (Nexus != null && go == Nexus.gameObject)
        {
            UI.GameOver();
        }

        DynamicallyCreated.Remove(go);
        GameObject.Destroy(go);
    }

    public bool TakeResource(ResourceType resource, int amount)
    {
        int got = 0;
        int curIndex = 0;
        // TODO: currently if less resources stored, it goes out of bounds
        while (got < amount)
        {
            got += ResourceStores[curIndex].TakeResource(resource, amount - got);
            curIndex++;
        }
        return got == amount;
    }
    

    public void UnloadMap()
    {
        // copying to array cause they will get deleted during iterating
        foreach(var obj in DynamicallyCreated.ToArray())
        {
            if (!ManagedLifetime[obj])
                DestroyDynamicObject(obj);
            else
                Assert.IsTrue(obj == null, "Object with managed lifetime not destroyed before unload");
        }
        pathFinding = null;
        customTerrain = null;
        WireManager = null;

        Assert.IsTrue(DynamicallyCreated.Count == 0);
    }

    public void ActivateObject(ActiveObject robj)
    {
        if (Mode == GameMode.Editor)
            return;
        Assert.IsTrue(robj.gameObject.activeInHierarchy, "The gameobject you're trying to activate must be active! Otherwise Awake() won't be called on scripts that need it");

        foreach(var tag in robj.ObjectTags)
        {
            LogicInfo linfo;
            var hasLogic = BehaviourLookup.TryGetValue(tag, out linfo);
            Component bcomponent = null;
            if (hasLogic)
            {
                bcomponent = robj.gameObject.AddComponent(linfo.BehaviourType);
                if(bcomponent is IResourceStore)
                {
                    ResourceStores.Add(bcomponent as IResourceStore);
                }
                (bcomponent as ManagedMonoBehaviour).ActiveObject = robj;
                // TODO: the value also has to be removed from the dictionary
                if (linfo.IsConnactable)
                    WireManager.AddConnector(robj.gameObject, (bcomponent as Connectable).Connector);
            }

            switch (tag)
            {
                case ActiveObject.ObjectTag.Nexus:
                    if (Nexus != null)
                        Debug.LogError("Only one Nexus allowed in a game!");
                    Nexus = bcomponent as Nexus;
                    break;
            }
        }
    }

    [System.Serializable]
    public class ResourceInfo
    {
        public List<GameObjectID> DefensePlaceables     = new List<GameObjectID>();
        public List<GameObjectID> ElectricityPlaceables = new List<GameObjectID>();
        public List<GameObjectID> UtilityPlaceables     = new List<GameObjectID>();
        public List<GameObjectID> UnitPlaceables        = new List<GameObjectID>();
    }
    //ttest

    public List<Destroyable> Destroyables = new List<Destroyable>();

    public void RegisterDestroyable(Destroyable destroyable)
    {
        Destroyables.Add(destroyable);
    }

    public void DestroyDestroyable(Destroyable destroyable)
    {
        Destroyables.Remove(destroyable);
    }

    public ItemPlacer ItemPlacer
    {
        get { return (Mode == GameMode.Editor) ?  editor.Placer : localPlayer.Placer; }
    }

    // TODO: write faster version
    public List<Destroyable> GetDestroyablesInRange(Vector3 rangeFrom, float range)
    {
        var ret = Destroyables.Where(x => Vector3.Distance(x.transform.position, rangeFrom) <= range).ToList();
        return ret;
    }

    // TODO: write faster version
    public List<Destroyable> GetDestroyableEnemiesInRange(Vector3 rangeFrom, float range)
    {
        var ret = Destroyables.Where(x => Vector3.Distance(x.transform.position, rangeFrom) <= range && x.Side == Destroyable.DestroyableSide.Enemy && x.gameObject.activeInHierarchy).ToList();
        return ret;
    }

    public List<Destroyable> GetDestroyableEnemiesInRange(Vector3 rangeFrom, float minRange, float maxRange)
    {
        var ret = Destroyables.Where(x => Vector3.Distance(x.transform.position, rangeFrom) <= maxRange
            && Vector3.Distance(x.transform.position, rangeFrom) >= minRange
            && x.Side == Destroyable.DestroyableSide.Enemy 
            && x.gameObject.activeInHierarchy).ToList();
        return ret;
    }

    // TODO: write faster version
    public List<Destroyable> GetDestroyableAlliesInRange(Vector3 rangeFrom, float range)
    {
        var ret = Destroyables.Where(x => Vector3.Distance(x.transform.position, rangeFrom) <= range && x.Side == Destroyable.DestroyableSide.Ally && x.gameObject.activeInHierarchy).ToList();
        return ret;
    }

    public void SerializeCurrentSettings()
    {
#if UNITY_EDITOR
        string backupFileName = "Assets/Resources/Backup/gamesettings";
        string path = "Assets/Resources/gamesettings.json";

        // create backup of old
        if(File.Exists(path))
            File.Copy(path, backupFileName + "_backup_"+DateTime.Now.ToString("_dd-MM-yyyy_HH-mm-ss") + ".json");

        string settingsString = JsonUtility.ToJson(gameSettings, true);
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(settingsString);
            }
        }
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    void LoadSettings()
    {
        var settingsFile = Resources.Load<TextAsset>("gamesettings");
        if (settingsFile != null && settingsFile is TextAsset)
        {
            var settingsText = settingsFile as TextAsset;
            gameSettings = JsonUtility.FromJson<GameSettings>(settingsText.text);
            if (gameSettings == null) // create new if failed
                gameSettings = new GameSettings();
        }
        else
        {
            Debug.LogError("GameSettings.json wasn't a textasset??");
        }
    }

    void DebugAwake()
    {
#if DEBUG
        Func<string> lb = () =>
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2} Progress {3}", GameTime.Hour, GameTime.Minute, GameTime.Second, GameTime.DayProgress);
        };
        DebugUI.AddParameter("Time", lb);
#if DONTWANTTHISHIT
        var rnames = Enum.GetNames(typeof(ResourceType));
        for (int i = 0; i < rnames.Length; i++)
        {
            int locali = i; // need a copy for the delegate
            Func<string> func = () =>
            {
                return WorkerWorkScheduler.GetCurrentLimit((ResourceType)locali).ToString();
            };
            DebugUI.AddParameter(rnames[i], func);
        }
#endif
#endif
    }

    void Awake()
    {
        Assert.IsTrue(Instance == null, "Another instance of Game was found! There can only be one game!!!!");
        Instance = this;

        BehaviourLookup = BehaviourFinder.GetBehaviourTypes();

        FogOfWar = new FogOfWar();
        ItemSelector = new ItemSelector(MainCamera.GetComponent<ItemHighligher>());

        customTerrain = FindObjectOfType<CustomTerrain>();
        pathFinding = new PathFinding(customTerrain);

        FogOfWar.Enable();
        ItemShop.LoadItems();
        LoadSettings();

        TextAsset rinfoAsset = Resources.Load<TextAsset>("resourceinfo");
        CurrentResourceInfo = JsonUtility.FromJson<ResourceInfo>(rinfoAsset.text);

        if (UIAudioSource == null)
            UIAudioSource = gameObject.AddComponent<AudioSource>();

        if(Mode == GameMode.Test)
        {
            customTerrain = FindObjectOfType<CustomTerrain>();
            Assert.IsTrue(pathFinding == null);
            pathFinding = new PathFinding(customTerrain);
        }

        if (Mode == GameMode.Normal || Mode == GameMode.Test)
        {
            Assert.IsTrue(localPlayer == null, "LocalPlayer was set before initialization??");

            Player.PlayerSettings psettings = gameSettings.PlayerSettings;
            localPlayer = new Player(MainCamera, ref psettings);
            WireManager = new WireLogicManager();
            BulletManager = new ProjectileManager(Bullet, Bomb);
        }

        if(Mode == GameMode.Normal)
        {
            if (FindObjectsOfType<UI>() == null)
            {
                UIGameObject = GameObject.Instantiate(UIGameObject);
            }
            if (EventSystem.current == null)
            {
                Instantiate(ESPrefab);
            }

            var pobjs = FindObjectsOfType<PlaceableObject>();
            foreach (var pobj in pobjs)
            {
                //Debug.Log("Loading "+pobj.name);
                pobj.gameObject.SetActive(true);
                bool res = pobj.TryPlace(pobj.transform.position, false);
                Assert.IsTrue(res, "Failed to place PlaceableObject " + pobj.name + ". Make sure it fits where it's placed in the scene");
            }
        }

        if (Mode == GameMode.Editor)
        {
            editor = new MapEditor();
        }
        DebugAwake();
    }

    void OnDestroy()
    {
        MonoBehaviourManager.Clear();
    }

    void Start()
    {
        if (Mode == GameMode.Normal)
        {
            //LoadMap("newmap");
        }
        InitializationCheck();
    }

    void FixedUpdate()
    {
		if (_paused)
			return;
		
        Profiler.BeginSample("GameTime.Update()");
        GameTime.Update(Time.deltaTime);
        Profiler.EndSample();
        Profiler.BeginSample("PhysicalWireManager.FixedUpdate()");
        PhysicalWireManager.FixedUpdate();
        Profiler.EndSample();
    }

    void UpdateDaylight()
    {
        //Time.timeScale = 20.0f;
        float progress = GameTime.DayProgress;
        // why the hell are they using degrees???
        Quaternion sunRot = Quaternion.AngleAxis((SunHorizontalAngle.Evaluate(progress) / Mathf.PI) * 180.0f, Vector3.up);
        sunRot *= Quaternion.AngleAxis((SunVerticalAngle.Evaluate(progress) / Mathf.PI) * 180.0f, Vector3.right);
        if (Sun != null)
        {
            Sun.transform.rotation = sunRot;
            Sun.intensity = Mathf.Max(SunVerticalAngle.Evaluate(progress) / 1.5f, 0.0f);
        }
    }

    void Update()
    {
		if (Input.GetKeyDown (KeyCode.Pause)) {
			_paused = !_paused;
		}

		if (_paused)
			return;

        if(Mode == GameMode.Normal || Mode == GameMode.Test)
        {
            Profiler.BeginSample("ItemSelector.Update");
            ItemSelector.Update();
            Profiler.EndSample();

            Profiler.BeginSample("Pathfinding CheckDoneQueries");
            pathFinding.CheckDoneQueries();
            Profiler.EndSample();

            Profiler.BeginSample("LocalPlayer.Update()");
            localPlayer.Update();
            Profiler.EndSample();

            Profiler.BeginSample("WireManager.Update()");
            WireManager.Update();
            Profiler.EndSample();

            Profiler.BeginSample("Game.UpdateDaylight()");
            UpdateDaylight();
            Profiler.EndSample();

            Profiler.BeginSample("BullterMangaer.Updtae()");
            BulletManager.Update();
            Profiler.EndSample();

            Profiler.BeginSample("WallPlacer.Update()");
            WallPlacer.Update();
            Profiler.EndSample();
        }
        else if(Mode == GameMode.Editor)
        {
            editor.Update();
        }
        Profiler.BeginSample("MonoBehaviourManager.Update()");
        MonoBehaviourManager.Update();
        Profiler.EndSample();
    }

    void OnGUI()
    {
        Profiler.BeginSample("DebugUI.OnGUI()");
        DebugUI.OnGUI();
        Profiler.EndSample();
    }

    void LateUpdate()
    {
        if (Mode == GameMode.Normal || Mode == GameMode.Test)
        {
            Profiler.BeginSample("Player.LateUpdate()");
            localPlayer.LateUpdate();
            Profiler.EndSample();
        }
    }

    void OnApplicationQuit()
    {
        if (pathFinding != null)
            pathFinding.Stop();
    }

    void OnDrawGizmos()
    {
        if (pathFinding != null)
        {
            Profiler.BeginSample("PathFinding.OnDrawGizmos()");
            pathFinding.OnDrawGizmos();
            Profiler.EndSample();
            Profiler.BeginSample("PhyscialWireManager.OnDrawGizmos()");
            PhysicalWireManager.OnDrawGizmos();
            Profiler.EndSample();
        }
    }

	public static IResourceStore FindClosestGenericStore(Vector3 from)
	{
		float maxSqDist = float.MaxValue;
		IResourceStore closest = null;
		int count = ResourceStores.Count;
		for (int i = 0; i < count; i++) {
            IResourceStore store = ResourceStores[i];
            if (!store.IsGenericStorage)
                continue;
			float sqDist = Vector3.SqrMagnitude (from - store.rsgameObject.transform.position);
			if (sqDist < maxSqDist) {
				maxSqDist = sqDist;
				closest = store;
			}
		}
		return closest;
	}

    void InitializationCheck()
    {
        if(Mode == GameMode.Normal)
        {
            if(MainCamera == null)
            {
                Debug.LogError("Assign a camera to the Game Monobehaviour!");
            }
            else
            {
                var ih = MainCamera.GetComponent<ItemHighligher>();
                if (ih == null)
                    Debug.LogError("Camera must have an ItemHighligher attached to it!");
            }
            if (Bullet == null)
                Debug.LogError("Assign a Bullet to the Game MonoBehaviour");
            if (PurchaseFailed == null)
                Debug.LogWarning("There is no PurchaseFailed sound effect assigned to the Game MonoBehaviour!");
            if (UIAudioSource == null)
                Debug.LogError("Assign a UI AudioSource to the Game Monobehaviour!");
            if (Sun == null)
                Debug.LogError("Assign a Sun Directional Light to the Game MonoBehaviour!");
            if (localPlayer == null)
                Debug.LogError("Local Player was not assigned after Initialization! Did you remove it?");
            if (WireManager == null)
                Debug.LogError("WireMinager was not assigned after Initialization! Did you remove it?");
            if (CurrentResourceInfo == null)
                Debug.LogError("CurrentResourceInfo was not set after Initialization! Does Resources/resourceinfo.json still exist?");
            if (BulletManager == null)
                Debug.LogError("BulletManager was not set after initialization! Did you remove it?");
            if (MainCamera.renderingPath != RenderingPath.DeferredShading)
                Debug.LogError("Using non-deferred rendering path, some features might not work!!");
            if (ItemSelector == null)
                Debug.LogError("ItemSelector was not assigned after Initialization! Did you remove it?");
        }
        else if(Mode == GameMode.Editor)
        {
            var mec = Camera.main.GetComponent<MapEditorCamera>();
            if (mec == null)
                Debug.LogError("The camera in map editor doesn't have MapEditorCamera script attached!");
        }
    }
}
