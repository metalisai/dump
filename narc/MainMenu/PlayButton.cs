// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour {

    void OnMouseDown()
    {
        SceneManager.LoadScene("Scene001");
    }
}
