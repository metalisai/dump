// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour {

    public Button StartButton;

	// Use this for initialization
	void Start () {
        StartButton.onClick.AddListener(StartClick);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void StartClick()
    {
        Debug.Log("Play");
    }
}
