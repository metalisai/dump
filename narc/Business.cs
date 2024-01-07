// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BusinessBuyingPaymentType
{
    Cash,
    BankTransfer
}

public class Business : MonoBehaviour, IItemSelectHandler
{
    public string BusinessName;
    public int IncomePerPayday;
    public int ExpenditurePerPayday;
    public int MaxLaundering;
    public int LaunderingIncome;
    public float Capacity = 1f;
    public string Description;
    public Sprite Icon;
    public Player Owner;
    public int Cost;
    public BusinessBuyingPaymentType PaymentType;

    BusinessWindow selectionWindow;
    RiskHeatMap _riskMap;

    void Start()
    {
        GameTime.OnPayday += Payday;
        selectionWindow = FindObjectOfType<BusinessWindow>();
        _riskMap = FindObjectOfType<RiskHeatMap>();

        GameTime.OnTick += Tick;
        GameTime.OnHour += Hour;
    }

    void Payday()
    {
        if (Owner != null)
        {
            int moneyToLaunder = Mathf.Min(LaunderingIncome, Owner.Cash);
            int income = (int)(IncomePerPayday*Capacity) - ExpenditurePerPayday;
            Owner.BankMoney += moneyToLaunder + income;
            Owner.Cash -=  moneyToLaunder;
        }
    }

    void Hour()
    {
        Capacity = Mathf.Clamp01(1f - _riskMap.GetHeat(transform.position) + Random.Range(-0.05f,0.05f));
    }

    void Tick()
    {
        //Capacity = 1f-_riskMap.GetHeat(transform.position);
    }

    public bool Buy(Player buyer)
    {
        if(Owner != null)
        {
            Debug.LogError("The player should not be able to buy an business that's already owned!");
            return false;
        }

        switch(PaymentType)
        {
            case BusinessBuyingPaymentType.Cash:
                if(buyer.Cash >= Cost)
                {
                    buyer.Cash -= Cost;
                    Owner = buyer;
                    return true;
                }
                else
                {
                    return false;
                }
            case BusinessBuyingPaymentType.BankTransfer:
                if (buyer.BankMoney >= Cost)
                {
                    buyer.BankMoney -= Cost;
                    Owner = buyer;
                    return true;
                }
                else
                {
                    return false;
                }
            default:
                Debug.LogError("Unknown payment type");
                return false;
        }
    }

    void OnDestroy()
    {
        GameTime.OnPayday -= Payday;
        GameTime.OnTick -= Tick;
    }

    public bool OnSelected()
    {
        // TODO: this assumes there is always 1 player
        bool show = Owner != null;
        if (show)
        {
            selectionWindow.Business = this;
            selectionWindow.Show();
        }
        return show;
    }

    public void OnDeSelecded()
    {
        selectionWindow.Hide();
    }
}
