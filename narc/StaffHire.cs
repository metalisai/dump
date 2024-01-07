// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StaffHire : MonoBehaviour 
{
    [System.Serializable]
    public struct HireableStaff
    {
        public int Price;
        public StaffMember Reference;
    }

    private Player _player;

    private HireableStaff _curItem;

    public List<HireableStaff> AvailableStaff = new List<HireableStaff>();

    void Awake()
    {
        _player = GameObject.FindObjectOfType<Player>();
        //_placer = (ItemPlacer)GameObject.FindObjectOfType(typeof(ItemPlacer));
    }

    // TODO: WTF IS THIS MESSSS
    public HireableStaff ByStaffMember(StaffMember item)
    {
        return AvailableStaff.Where(x => x.Reference.JobTitle == item.JobTitle).FirstOrDefault();
    }

    public bool PeekItem(HireableStaff item)
    {
        if (_player.Cash >= item.Price)
        {
            return true;
        }
        return false;
    }

    public bool BuyItem(HireableStaff item)
    {
        if (PeekItem(item))
        {
            _player.Cash -= item.Price;
            return true;
        }
        return false;
    }

    public void StartPlacing(HireableStaff item)
    {
        item.Reference.StartPlacing();
        _curItem = item;
    }
}