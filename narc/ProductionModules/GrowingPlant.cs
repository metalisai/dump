// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Linq;

public class GrowingPlant : MonoBehaviour, ISaveable, IItemHoverHandler, IItemSelectHandler, IProgressable
{

	public GameObject[] Stages;
	public static float StageTime = 5f; // the Time each stage lasts
    public float DefaultGramsExtractable = 60f;

    public bool Replant = true;
	
    public Sprite HarvestLogo;
    public GameObject Exclamation;

	private float _currentStage = 0f; // the Time Current stage has lasted

	private int _inStage = 0; // the Current stage the tree is in

	public bool grown = false; // is the tree grown full ?
	private bool _growing = true;

    private GameObject _harvestButton;
    private SelectionWindow _selWin;

    public PlaceableObject PlaceableObject
    {
        get
        {
            return _pObj;
        }
    }
	private PlaceableObject _pObj;

    private float _progRatio = 0f;


	void Awake()
	{
		_pObj = (PlaceableObject) (GetComponentInParent (typeof(PlaceableObject)) ?? GetComponent (typeof(PlaceableObject)));
	}

    void Start()
    {
        GameTime.OnTick += Tick;
        _selWin = FindObjectOfType<SelectionWindow>();
    }

    private float GetLightMultiplier(GridEffectType lighteffect)
    {
        switch(lighteffect)
        {
            case GridEffectType.None:
                return 1f;
            case GridEffectType.LampLight200W:
                return 1.6f;
            case GridEffectType.LampLight400W:
                return 2.1f;
            case GridEffectType.LampLight800W:
                return 2.9f;
            default:
                return 0f;
        }
    }

    // TODO: this doesn't belong here
    public string GetLightName(GridEffectType light)
    {
        switch (light)
        {
            case GridEffectType.None:
                return "Daylight";
            case GridEffectType.LampLight200W:
                return "200W";
            case GridEffectType.LampLight400W:
                return "400W";
            case GridEffectType.LampLight800W:
                return "800W";
            default:
                Debug.LogError("Invalid parameter!");
                return "";
        }
    }

    // TODO: keep Effects ordered, then no need for linear search
    public GridEffectType GetBestLight()
    {
        var eff = _pObj.Cells.FirstOrDefault().Effects;

        GridEffectType best = GridEffectType.None;
        float bestMult = 1f;

        for (int i = 0; i < eff.Count; i++)
        {
            float mult = GetLightMultiplier(eff[i].Effect);
            if (mult > bestMult)
            {
                bestMult = mult;
                best = eff[i].Effect;
            }

        }

        return best;
    }

    void Tick()
    {
        float bestMultiplier = GetLightMultiplier(GetBestLight());

        if (_growing)
        {

            if (!grown)
            {
                _currentStage += GameTime.GameTickInterval; // increase the timer
                if (_currentStage >= StageTime) // the timer for the stage has reached its end
                {
                    NextStage(); // switch stage
                    _currentStage = 0f; // reset timer
                }
                _progRatio += bestMultiplier/(((StageTime * (Stages.Length - 1))/GameTime.GameTickInterval)-1);
            }
        }
    }

    void Update()
    {
        if(grown)
        {
            UpdateHarvestButton();
        }
    }

    private void UpdateHarvestButton() // sets the position of the harvest button to the plant psition on screen
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(gameObject.transform.position+new Vector3(0f,0.75f,0f));

        _harvestButton.transform.position = screenPos;
    }

    private void SetStage(int stage)
    {
        if (stage >= Stages.Length)
            Debug.LogError("Plant state out of range!");
        for(int i = 0; i < Stages.Length; i++)
        {
            Stages[i].SetActive(i == stage);
        }

        if (stage == Stages.Length - 1)
        {
            _inStage = stage - 1;
            OnGrown();
        }
        else
        {
            _inStage = stage;
        }
    }

	private void NextStage() // switch to next stage
	{
		Stages [_inStage].SetActive (false); // disable the Current stage gameObject

		if (Stages.Length-1 != _inStage + 1) 
		{
			_inStage++;
            // apply random rotation every stage, so plants look different
            Stages [_inStage].transform.Rotate(new Vector3(0f,Random.Range(0f,360f),0f));
			Stages [_inStage].SetActive (true);
		} 

		else 
		{
			OnGrown();
		}
	}

    private void OnGrown() // the plant has grown full
    {
        grown = true;
        _inStage++;
        Stages[_inStage].SetActive(true);

        _harvestButton = UIHelpers.CreateButton(UIController.Canvas.transform, new Vector3(200f, -200f, 0f), new Vector2(47f, 50f), delegate { Harvest(); }, HarvestLogo);

        Exclamation.SetActive(true);
        _harvestButton.SetActive(false);

        UpdateHarvestButton();
        Debug.Log("ratio: " + _progRatio);
        //gameObject.layer = 0;
    }

    public bool Harvest() // the plant has been harvested
    {
        bool addedDrying = false;

        if (_pObj != null && _pObj.Infrastructure != null)
        {
            var dryer = _pObj.Infrastructure.Dryers.FirstOrDefault(x => x.State != WeedDryerState.Full);

            if (dryer != null)
            {
                // default weed amount generated without bonus ratios
                float def = Random.Range(DefaultGramsExtractable - (0.2f * DefaultGramsExtractable), DefaultGramsExtractable + (0.2f * DefaultGramsExtractable));
                addedDrying = dryer.PutWeed(def * _progRatio); // put weed drying and add bonuses
                if(Replant)
                {
                    this._pObj.Infrastructure.GardenerObserver.QueueForReplant(this._pObj.Cells.FirstOrDefault());
                }
            }
            else
            {
                Debug.Log("No Dryers assigned to player!");
            }
        }
        else
        {
            Debug.Log("Plant didn't have infrastructure, but trying to harvest!");
        }

        if (addedDrying)
        {
            Destroy(_harvestButton);
            Destroy(_pObj.gameObject);
			//var grid = Infrastructure.Grids.FirstOrDefault(x => x.IsInGrid(transform.position));
        }

        Debug.Log("Harvest");
        return addedDrying;
    }

    public void OnHover()
    {
        if(grown)
            _harvestButton.SetActive(true);
    }

    public void OnHoverEnd()
    {
        if (grown)
            _harvestButton.SetActive(false);
    }

    public System.Object GetState()
    {
        GrowingPlantSave state = new GrowingPlantSave
        {
            Stage = _inStage,
            StageTimer = _currentStage,
            CurrentRatio = _progRatio
        };
        return state;
    }

    public void SetState(System.Object state)
    {
        var cstate = state as GrowingPlantSave;
        SetStage(cstate.Stage);
        _currentStage = cstate.StageTimer;
        _progRatio = cstate.CurrentRatio;
    }

    public bool OnSelected()
    {
        if (_selWin.currentSelection != this.gameObject)
        {
            _selWin.SetSelected(this);
        }
        return true;
    }

    public void OnDeSelecded()
    {
        if (_selWin.currentSelection == PlaceableObject.gameObject)
        {
            _selWin.Hide();
        }
    }

    void OnDestroy()
    {
        Debug.Log("plant destroyed");
        GameTime.OnTick -= Tick;
    }

    public float GetProgress()
    {
        return (_currentStage + _inStage * StageTime) / (StageTime * Stages.Length - 1);
    }
}

[System.Serializable]
class GrowingPlantSave
{
    public int Stage;
    public float StageTimer;
    public float CurrentRatio;
}
