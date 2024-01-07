// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;

public class CharacterWindow : MonoBehaviour {

    GameObject _container;

    public GameObject IdentityTab;
    public GameObject InfoTab;
    StaffMember _staffMember;

    public enum Tab
    {
        Identity,
        Info
    }

    void Start()
    {
        _container = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if(_container.activeInHierarchy && Input.GetKeyUp(KeyCode.Escape))
        {
            Hide();
        }
    }

    public void SetTab(int tab)
    {
        switch((Tab)tab)
        {
            case Tab.Identity:
                IdentityTab.transform.SetSiblingIndex(_container.transform.childCount - 2); // NOTE: -2 because we want fire button to persist
                IdentityTab.GetComponent<CharacterIdentityTab>().SetDataSource(_staffMember);
                break;
            case Tab.Info:
                // TODO: replace temp code
                InfoTab.transform.SetSiblingIndex(_container.transform.childCount - 2);
                var comp = InfoTab.GetComponent<CharacterInfoTab>();
                if(_staffMember.GetComponent<Dealer>() != null)
                {
                    comp.SetDataSource(_staffMember.GetComponent<Dealer>());
                    comp.WeedPriceSlider.enabled = true;
                    comp.SalesText.enabled = true;
                }
                else
                {
                    comp.WeedPriceSlider.enabled = false;
                    comp.SalesText.enabled = false;
                }
                break;
            default:
                Debug.LogError("Character window set to unknown tab!");
                break;
        }
    }

    public void FireCurrent()
    {
        if (_staffMember != null)
        {
            Destroy(_staffMember.gameObject);
            Hide();
        }
        else
        {
            Debug.LogError("StaffMember not set when trying to fire!");
        }
    }

	public void Show(StaffMember staffMember)
    {
        _staffMember = staffMember;
        IdentityTab.GetComponent<CharacterIdentityTab>().SetDataSource(_staffMember);
        //IdentityTab.GetComponent<CharacterIdentityTab>().SetDataSource(StaffMember);
        _container.SetActive(true);
    }

    public void Hide()
    {
        _container.SetActive(false);
    }
}
