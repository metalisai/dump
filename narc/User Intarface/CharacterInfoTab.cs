// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class CharacterInfoTab : MonoBehaviour {

    public Text SalesText;
    public Text PriceText;
    public Slider WeedPriceSlider;

    public int MinPrice = 50;
    public int MaxPrice = 200;

    Dealer _dealer;

    public void Start()
    {
        WeedPriceSlider.onValueChanged.AddListener(SliderValueChanged);
        MinPrice = (int)(FindObjectOfType<WeedTrend>().basePrice * 0.5f);
        MaxPrice = (int)(FindObjectOfType<WeedTrend>().basePrice * 2f);
    }

	public void SetDataSource(Dealer dealer)
    {
        SalesText.text = dealer._salesToday.ToString();
        _dealer = dealer;
        float sliderVal = (float)(_dealer.WeedPrice - MinPrice) / (MaxPrice - MinPrice);
        WeedPriceSlider.value = sliderVal;
    }

    public void SliderValueChanged(float value)
    {
        int weedPrice = (int)Mathf.Lerp(MinPrice, MaxPrice, value);
        //LaunderingIncomeText.text = String.Format("{0}$/Week", launder);
        PriceText.text = string.Format("Weed price: (current {0}$)", weedPrice);
        //UpdateRiskTexts(value);
        _dealer.WeedPrice = weedPrice;
    }
}
