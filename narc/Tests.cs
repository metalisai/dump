// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

// The purpose of this is to double check everything
// this is useless

public class Tests : MonoBehaviour {

    public bool test = false;

    public void Assert(bool condition)
    {
        if(!condition)
        {
            Debug.LogError("Assertion failed!");
        }
    }

	void Start () {

        if (!test)
            return;

        var gt = FindObjectsOfType<GameTime>();
        Assert(gt.Length == 1);

        var or = FindObjectsOfType<ObjectRegister>();
        Assert(or.Length == 1);


	}
	
	void Update () {
	
	}
}
