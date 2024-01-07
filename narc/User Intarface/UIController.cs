// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public class UIController : MonoBehaviour
{

    public static Canvas Canvas;
    public GameObject gameSaveWindow;

	// Use this for initialization
	void Start ()
	{
	    Canvas = gameObject.GetComponentInParent<Canvas>();
	}

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            gameSaveWindow.SetActive(!gameSaveWindow.activeInHierarchy);
        }
    }

    public void ToggleActive()
    {
        Canvas.gameObject.SetActive(!Canvas.gameObject.activeInHierarchy);
    }
}
