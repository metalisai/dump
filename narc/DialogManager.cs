// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class DialogManager : MonoBehaviour {

    public Dialog SampleObject;
    public static DialogManager Instance;

    void Start()
    {
        Instance = this;
    }

    public void ShowDialog(string Title, string Description, UnityAction OnYes, UnityAction OnNo)
    {
        var go = GameObject.Instantiate(SampleObject.gameObject);
        go.transform.SetParent(SampleObject.transform.parent);
        var dia = go.GetComponent<Dialog>();
        dia.Title.text = Title;
        dia.Description.text = Description;
        dia.YesButton.onClick.AddListener(delegate { Destroy(go); });
        dia.NoButton.onClick.AddListener(delegate { Destroy(go); });
        dia.YesButton.onClick.AddListener(OnYes);
        dia.NoButton.onClick.AddListener(OnNo);
        go.transform.localPosition = SampleObject.transform.localPosition;
        go.SetActive(true);
    }
}
