// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class StaffStore : MonoBehaviour
{
    public StoreMenuElement TemplateElement;

    UIList<StoreMenuElement> StoreList = new UIList<StoreMenuElement>();

    StaffHire _store;

    public bool IsShown { get { return transform.GetChild(0).gameObject.activeInHierarchy; } }

    void Start()
    {
        _store = FindObjectOfType<StaffHire>();

        StoreList.ElementOffset = new Vector2(0f, -75f);
        StoreList.TemplateElement = TemplateElement;
    }

    public void Show()
    {
        int i = 0;
        var sme = TemplateElement;
        foreach (var el in _store.AvailableStaff)
        {
            var elCopy = el; // copy reference for the delegate
            if (i != 0)
                sme = StoreList.AddElement();
            // can anyone write a code line that is more retarded?
            sme.SetElementDataSource(el.Reference, new UnityAction[] { delegate { _store.StartPlacing(elCopy); }, Hide });
            i++;
        }

        // use all the screen space
        var storeMenuGameObject = transform.GetChild(0).gameObject;
        Vector2 sizeVector2 = storeMenuGameObject.transform.AsRectTransform().sizeDelta;
        float halfHeight = Screen.height / 2f;
        sizeVector2.y = storeMenuGameObject.transform.AsRectTransform().position.y - halfHeight + (halfHeight - 50f);
        storeMenuGameObject.transform.AsRectTransform().sizeDelta = sizeVector2;

        storeMenuGameObject.SetActive(true);
    }

    public void Hide()
    {
        StoreList.DestroyElements();
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public RectTransform GetTransform()
    {
        return transform.GetChild(0).transform.AsRectTransform();
    }
}
