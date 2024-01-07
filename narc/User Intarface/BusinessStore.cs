// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class BusinessStore : MonoBehaviour 
{
    public BusinessStoreElement TemplateElement;

    UIList<BusinessStoreElement> StoreList = new UIList<BusinessStoreElement>();

    public Vector2 ElementOffset = new Vector2(0f,-150f);

    Player _player;

    bool _shown = false;
    public bool IsShown { get { return _shown; } }

    void Awake()
    {
        StoreList.ElementOffset = ElementOffset;
        StoreList.TemplateElement = TemplateElement;
    }

    void Start()
    {
        _player = FindObjectOfType<Player>();
    }

    public void Show()
    {
        MovableWindow m;
        if((m = GetComponent<MovableWindow>()) != null)
        {
            m.SetOnTop();
        }

        int i = 0;
        var comp = TemplateElement;
        // TODO: replace temp
        foreach (var business in FindObjectsOfType<Business>().Where(x => x.Owner == null))
        {
            var elCopy = business; // copy reference for the delegate

            if (i != 0)
            {
                comp = StoreList.AddElement();
            }
            UnityAction onBuyAction = delegate { AttemptBuyBuysiness(_player, elCopy); };
            comp.SetDataSource(business, onBuyAction);
            i++;
        }
        if(i == 0)
        {
            StoreList.HideElements();
        }

        // use all the screen space
        var storeMenuGameObject = transform.GetChild(0).gameObject;
        /*Vector2 sizeVector2 = storeMenuGameObject.transform.AsRectTransform().sizeDelta;
        float halfHeight = Screen.height / 2f;
        sizeVector2.y = storeMenuGameObject.transform.AsRectTransform().position.y - halfHeight + (halfHeight - 50f);
        storeMenuGameObject.transform.AsRectTransform().sizeDelta = sizeVector2;*/

        storeMenuGameObject.SetActive(true);
        _shown = true;
    }

    void AttemptBuyBuysiness(Player player, Business business)
    {
        // TODO: on failure show something to user or whatever
        var result = business.Buy(player);
        if(result)
            Hide();
    }

    public void Hide()
    {
        StoreList.DestroyElements();
        transform.GetChild(0).gameObject.SetActive(false);
        _shown = false;
    }
}
