// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class ItemStore : MonoBehaviour 
{
    public StoreMenuElement TemplateElement;

    Store _store;

    UIList<StoreMenuElement> StoreList = new UIList<StoreMenuElement>();

    public bool IsShown { get { return transform.GetChild(0).gameObject.activeInHierarchy; }}

    void Start()
    {
        _store = FindObjectOfType<Store>();

        StoreList.ElementOffset = new Vector2(0f, -75f);
        StoreList.TemplateElement = TemplateElement;
    }

    public void Show()
    {
        int i = 0;
        var sme = TemplateElement;
        foreach (var el in _store.StoreItems)
        {
            var elCopy = el; // copy reference for the delegate
            if (i != 0)
                sme = StoreList.AddElement();
            // can anyone write a code line that is more retarded?
            sme.SetElementDataSource(el, new UnityAction[] { delegate { Store.Instance.StartPlacing(elCopy); },Hide});
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
