// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine.Events;

public class BusinessStoreElement : MonoBehaviour 
{
    public Image Icon;
    public Text CostText;
    public Text PaymentText;
    public Text MaxLaunderingText;
    public Text BusinessNameText;
    public Button BuyButton;

    public void SetDataSource(Business business, UnityAction OnAttemptBuy)
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = ",";
        nfi.CurrencySymbol = "$";
        nfi.CurrencyPositivePattern = 1;

        Icon.sprite = business.Icon;
        CostText.text = business.Cost.ToString("c0", nfi);
        PaymentText.text = Regex.Replace(business.PaymentType.ToString(), "(\\B[A-Z])", " $1");
        MaxLaunderingText.text = business.MaxLaundering.ToString("c0", nfi)+"/Week";
        BusinessNameText.text = business.BusinessName;
        BuyButton.onClick.RemoveAllListeners();
        BuyButton.onClick.AddListener(OnAttemptBuy);
    }
}
