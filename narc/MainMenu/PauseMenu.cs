// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour {

    public GameObject UiGameObject;
    public GameObject UiGameObject2;

    public void Resume()
    {
        HidePauseMenu();
        Time.timeScale = 1f;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void Update()
    {
        if(!GlobalVariables.EscConsumed && Input.GetKeyDown(KeyCode.Escape))
        {
            ShowPauseMenu();
        }
        GlobalVariables.EscConsumed = false;
    }

    void ShowPauseMenu()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        Time.timeScale = 0f;
        UiGameObject.SetActive(false);
        UiGameObject2.SetActive(false);
        FindObjectOfType<NewRTSCamera>().Disabled = true;
    }

    void HidePauseMenu()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        UiGameObject.SetActive(true);
        UiGameObject2.SetActive(true);
        FindObjectOfType<NewRTSCamera>().Disabled = false;
    }
}
