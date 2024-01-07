using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

[ObjectLogic(ActiveObject.ObjectTag.Destroyable)]
public class Destroyable : ManagedMonoBehaviour {

    public enum DestroyableType
    {
        Unit,
        Building
    }

    public enum DestroyableSide
    {
        Ally,
        Enemy
    }

    public static bool NoDamage = false;

    public DestroyableType Type;
    public DestroyableSide Side;
    public bool HasSight = false;

	public float MaxHitPoints = 1000.0f;
    public float Hitpoints;

    bool started = false;

    MeshRenderer[] renderers;

    public void Hit(float damage)
    {
        DebugUI.ShowMessage(transform.position, "Dmg -" + ((int)damage).ToString());
        if (NoDamage)
            return;
        Hitpoints -= damage;
        if (Hitpoints <= 0)
            Game.Instance.DestroyDynamicObject(this.gameObject);
    }

	public float Health01()
	{
		return Mathf.Clamp01 ((float)Hitpoints / (float)MaxHitPoints);
	}

    public void Hide()
    {
        //hi.gameObject.SetActive(false);
        for (int i = 0; i < renderers.Length; i ++)
        {
            renderers[i].enabled = false;
        }
    }

    public void Show()
    {
        //hi.gameObject.SetActive(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = true;
        }
    }

    void AddToTracking()
    {
        //hi = UI.HealthInterface.Create();
        if (Game.Instance.FogOfWar != null)
        {
            if (Side == DestroyableSide.Ally && HasSight)
                Game.Instance.FogOfWar.AddSightAgent(transform, 100.0f);
            else if(Side == DestroyableSide.Enemy)
                Game.Instance.FogOfWar.AddEnemyAgent(this);
        }
        if (Side == DestroyableSide.Enemy)
            Hide();
    }

    public new void OnEnable()
    {
        if (started) // hack to deal with the fack that onEnable is called like Awake, which might happen before the object is initialized
        {
            AddToTracking();
        }
        base.OnEnable();
    }

    public new void OnDisable()
    {
        if (hi != null)
            Game.Instance.DestroyDynamicObject(hi.gameObject);
        if (Game.Instance.FogOfWar != null)
        {
            if (Side == DestroyableSide.Ally && HasSight)
                Game.Instance.FogOfWar.RemoveSightAgent(transform);
            else if(Side == DestroyableSide.Enemy)
                Game.Instance.FogOfWar.RemoveEnemyAgent(transform);
        }
        base.OnDisable();
    }

    void Awake()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
    }

    void Start()
    {
        // TODO: get rid of linq
        Side = ActiveObject.ObjectTags.Any(x => x == ActiveObject.ObjectTag.Ally) ? DestroyableSide.Ally : DestroyableSide.Enemy;
        HasSight = ActiveObject.ObjectTags.Any(x => x == ActiveObject.ObjectTag.HasSight);
        Hitpoints = MaxHitPoints;
        Game.Instance.RegisterDestroyable(this);
        AddToTracking();
        started = true;
    }

    public override void ManagedUpdate()
    {
        Profiler.BeginSample("Destroyable.ManagedUpdate()");
        if (hi != null)
			hi.UpdateData(transform.position, Side!=DestroyableSide.Ally, Health01());
        Profiler.EndSample();
    }

    void OnDestroy()
    {
        Game.Instance.DestroyDestroyable(this);
    }

    HealthbarInterface hi;
}
