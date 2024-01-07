using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SwitchInterface : ManagedMonoBehaviour {

    public Toggle toggle;

    void ShowInternal(Vector3 pos, bool currentValue, UnityAction<bool> OnChanged)
    {
        toggle.transform.position = Camera.main.WorldToScreenPoint(pos);
        toggle.onValueChanged.AddListener(OnChanged);
        toggle.isOn = currentValue;
        gameObject.SetActive(true);
    }

    public void Show(Vector3 pos, bool currentValue, UnityAction<bool> OnChanged)
    {
        Action action = () =>
        {
            ShowInternal(pos, currentValue, OnChanged);
        };
        UI.RequestPopup(this, action);
    }


    public void UpdateData(Vector3 pos)
    {
        toggle.transform.position = Camera.main.WorldToScreenPoint(pos);
    }

    void HideInternal()
    {
        toggle.onValueChanged.RemoveAllListeners();
        gameObject.SetActive(false);
    }

    public void Hide()
    {
        Action action = () =>
        {
            HideInternal();
        };
        UI.HidePopup(this, action);
    }
}
