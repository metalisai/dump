// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public enum SideMenuSubmenu
{
    Production,
    Laundering,
    Infrastructure,
    Staff
}

public class SideMenu : MonoBehaviour
{
    public GameObject StaffMenuGameObject;
    public StoreMenuElement StaffElement;

    UIList<StoreMenuElement> StaffList = new UIList<StoreMenuElement>();

    private ItemStore _itemStore;
    private StaffStore _staffStore;
    private BusinessStore _businessStore;

    void Start()
    {
        _staffStore = FindObjectOfType<StaffStore>();
        _itemStore = FindObjectOfType<ItemStore>();
        _businessStore = FindObjectOfType<BusinessStore>();

        StaffList.TemplateElement = StaffElement;
    }

    public void OnProdClick()
    {
        HideOthers(_itemStore);
        _itemStore.Hide();
        _itemStore.Show();
    }
    public void OnLaundClick()
    {
        HideOthers(_businessStore);
        if (!_businessStore.IsShown)
            _businessStore.Show();
    }
    public void OnInfraClick()
    {
        
    }
    public void OnStaffClick()
    {
        HideOthers(_staffStore);
        _staffStore.Hide();
        _staffStore.Show();
    }

    // null hides all
    private void HideOthers(object except)
    {
        if (_businessStore.IsShown && !ReferenceEquals(except, _businessStore))
            _businessStore.Hide();
        if (_itemStore.IsShown && !ReferenceEquals(except, _itemStore))
            _itemStore.Hide();
        if (_staffStore.IsShown && !ReferenceEquals(except, _staffStore))
            _staffStore.Hide();
    }

    private bool AnyShown()
    {
        return _businessStore.IsShown || _itemStore.IsShown || _staffStore.IsShown;
    }

    void Update()
    {
        if(AnyShown() && Input.GetKeyDown(KeyCode.Escape))
        {
            GlobalVariables.EscConsumed = true;
            HideOthers(null);
        }
        // click outside of the popup
        else if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HideOthers(null);
        }
    }
}
