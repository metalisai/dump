// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;

public class MagicalMysticalPixieDust : MonoBehaviour
{
    public float DealerBustThresold = 0.9f;

    bool _triggeredToday = true;
    int _triggerHour = 0;
    int _triggerMinute = 0;
    RiskHeatMap _rishHeatMap;
    
    void Start()
    {
        GameTime.OnDay += OnDay;
        GameTime.OnMinute += OnMinute;
        _rishHeatMap = FindObjectOfType<RiskHeatMap>();
    }

    void OnDestroy()
    {
        GameTime.OnDay -= OnDay;
        GameTime.OnMinute -= OnMinute;
    }

    void OnDay()
    {
        _triggeredToday = false;
        _triggerHour = Random.Range(0, 23);
        _triggerMinute = Random.Range(0, 59);
    }

    void OnMinute()
    {
        if(_triggeredToday == false && _triggerHour <= GameTime.Hour && _triggerMinute <= GameTime.Minute)
        {
            _triggeredToday = true;
            CheckDealers();
        }
    }

    void CheckDealers()
    {
        // TODO: replace with more performant option
        var dealers = FindObjectsOfType<Dealer>();
        foreach (var dealer in dealers)
        {
            if(_rishHeatMap.GetHeat(dealer.transform.position) > DealerBustThresold)
            {
                DialogManager.Instance.ShowDialog("RIP Dealer", "Your dealer got caught", delegate { }, delegate {});
                Destroy(dealer.gameObject);
            }
        }
    }

}
