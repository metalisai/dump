// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class BuyBusinessCoinThing : MonoBehaviour {

    public Business _business;
    public Housing _housing;

    void Update()
    {
        transform.rotation *= Quaternion.AngleAxis(90f * GameTime.RealDeltaTime, Vector3.left);
    }

	void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (_business != null)
        {
            // TODO: remove temp
           if( _business.Buy(FindObjectOfType<Player>()))
            {
                gameObject.SetActive(false);
            }
            else
            {
                DialogManager.Instance.ShowDialog("No money yo", "You have no $$$$$$$", delegate { }, delegate{ });
            }
        }
        else if(_housing != null)
        {
            // TODO: remove temp
            if(_housing.Buy(FindObjectOfType<Player>()))
            {
                gameObject.SetActive(false);
            }
            else
            {
                DialogManager.Instance.ShowDialog("No money yo", "You have no $$$$$$$", delegate { }, delegate { });
            }
        }
    }
}
