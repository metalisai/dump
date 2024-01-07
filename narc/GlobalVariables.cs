// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public class GlobalVariables : MonoBehaviour {

    public Material ArrowMaterial;

    public static Material ArrowMat;

    public static bool EscConsumed = false;

	void Awake () {
        ArrowMat = ArrowMaterial;
	}
}
