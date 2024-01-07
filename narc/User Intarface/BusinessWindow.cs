// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BusinessWindow : MonoBehaviour
{
    public Color LowRiskColor;
    public Color MediumRiskColor;
    public Color HighRiskColor;
    public Color InactiveRiskColor;

    public Text NameText;
    public Text SteadyIncomeText;
    public Text LaunderingIncomeText;
    public Text TaxText;

    public Text DescriptionText;

    public Text LaunderValueText;

    public Text MaxValueText;
    public Text MinValueText;

    public Text RiskLevelLowText;
    public Text RiskLevelMediumText;
    public Text RiskLevelHighText;

    public Slider LaunderSlider;

    public Image Icon;

    private Business _business;

    public Business Business
    {
        get { return _business; }
        set
        {
            _business = value;
            if (value != null)
            {
                UpdateInfo();
            }
        }
    }

    void Start()
    {
        // TODO: temp code
        //Business = FindObjectOfType<Business>();
    }

    public void Show()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void Hide()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    void UpdateInfo()
    {
        NameText.text = _business.BusinessName.ToUpper();
        SteadyIncomeText.text = string.Format("{0}$/Week", _business.IncomePerPayday);
        //LaunderingIncomeText.text = string.Format("{0}$/Week", _business.DefaultLaundering);
        TaxText.text = string.Format("{0}%", Mathf.RoundToInt(_business.Capacity*100));
        DescriptionText.text = _business.Description;
        MaxValueText.text = _business.MaxLaundering + "$";
        LaunderValueText.text = string.Format("Weekly laundering: (current {0}$)",_business.LaunderingIncome);
        Icon.sprite = _business.Icon;

        // TODO: can probably delete this in release build
        var temp = _business.LaunderingIncome;

        float launderValue = (float)(_business.LaunderingIncome)/(_business.MaxLaundering);
        LaunderSlider.value = launderValue;

        Debug.Assert(temp == _business.LaunderingIncome); // make sure that our formula is right
    }

    void UpdateRiskTexts(float value)
    {
        if (value < 0.4)
        {
            RiskLevelLowText.color = LowRiskColor;
            RiskLevelMediumText.color = InactiveRiskColor;
            RiskLevelHighText.color = InactiveRiskColor;
        }
        else if (value < 0.666)
        {
            RiskLevelLowText.color = InactiveRiskColor;
            RiskLevelMediumText.color = MediumRiskColor;
            RiskLevelHighText.color = InactiveRiskColor;
        }
        else
        {
            RiskLevelLowText.color = InactiveRiskColor;
            RiskLevelMediumText.color = InactiveRiskColor;
            RiskLevelHighText.color = HighRiskColor;
        }
    }

    public void SliderValueChanged(Single value)
    {
        int launder = (int)Mathf.Lerp(0, _business.MaxLaundering, value);
        //LaunderingIncomeText.text = String.Format("{0}$/Week", launder);
        LaunderValueText.text = string.Format("Weekly laundering: (current {0}$)", launder);
        UpdateRiskTexts(value);
        _business.LaunderingIncome = launder;
    }
}
