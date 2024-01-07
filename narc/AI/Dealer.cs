// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Linq;

public class Dealer : MonoBehaviour, ISaveable {

    public int BaseSalesPerDay = 10;

    [HideInInspector]
    public int WeedPrice = 100;

    int _curCarryingGrams = 0;

    private float _curSpentTime;
    //private int _curCarryingPackages;
    private int _curCash;
    public Housing Apartment;
    private RiskHeatMap _riskHeatmap;
    private WeedTrend _weedTrend;

    bool _timerSet = false;

    public int _salesToday = 0;
    float _dayAcc = 0f;

    int gramsPerClient = 10;
    // TODO: remoce constant
    int gramsPerPackage = 200;

	void Awake () {

        Apartment = GetComponentInParent<PlaceableObject>().Infrastructure;

        Apartment.SearchComponents(this.gameObject);

        GetComponentInParent<StaffMember>().Apartment = Apartment;
        GameTime.OnTick += Tick;
        GameTime.OnHour += Hour;
        GameTime.OnDay += Day;
        _riskHeatmap = FindObjectOfType<RiskHeatMap>();
        _weedTrend = FindObjectOfType<WeedTrend>();

        WeedPrice = (int)_weedTrend.basePrice;
	}

    void Tick()
    {
        _riskHeatmap.incPixel(gameObject.transform.position);

    }

    void Hour()
    {
        float salesThisHour = (_weedTrend.GetSalesMultiplier(WeedPrice)*BaseSalesPerDay)/24f;

        int sales = (int)salesThisHour;
        _dayAcc += salesThisHour - sales;
        if(_dayAcc >= 1f)
        {
            sales += (int)_dayAcc;
            _dayAcc -= (int)_dayAcc;
        }

        for (int i = 0; i < sales; i++)
        {
            var storage = Apartment.Storages.FirstOrDefault(x => x.AmountStored > 0);
            if (storage != null)
            {
                Debug.Log("Sale "+ WeedPrice * gramsPerClient);
                //TODO: if you make this at random points of the day, make sure you cache this at the start of every hour
                //_curCash += WeedPrice;
                Apartment.Owner.Cash += WeedPrice*gramsPerClient;
                ++_salesToday;
                _curCarryingGrams -= gramsPerClient;

                if (_curCarryingGrams < 0)
                {
                    storage.AddProduct(-1);
                    _curCarryingGrams += gramsPerPackage;
                }
            }
            else
            {
                Debug.LogWarning("Dealer did not find storage!");
            }
        }
    }

    void Day()
    {
        _salesToday = 0;
    }

    void OnDestroy()
    {
        GameTime.OnTick -= Tick;
        GameTime.OnHour -= Hour;
        GameTime.OnDay -= Day;
    }

    public System.Object GetState()
    {
        return new DealerSave
        {
            //MaxCarryingPackages = MaxCarryingPackages,
            CurSpentTime = _curSpentTime,
            //CurCarryingPackages = _curCarryingPackages,
            CurCash = _curCash
        };
    }

    public void SetState(object state)
    {
        var dstate = state as DealerSave;
        //MaxCarryingPackages = dstate.MaxCarryingPackages;
        //_curCarryingPackages = dstate.CurCarryingPackages;
        _curSpentTime = dstate.CurSpentTime;
        _curCash = dstate.CurCash;
    }

    [System.Serializable]
    class DealerSave
    {
        public int MaxCarryingPackages;
        public float CurSpentTime;
        //public int CurCarryingPackages;
        public int CurCash;
    }
}
