// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public class StartingConfig : MonoBehaviour
{
    public Housing Infrastructure;
	// Use this for initialization
	void Awake ()
	{
	    Player player = (Player)GameObject.FindObjectOfType(typeof(Player));
	    Housing startingInfrastructure = Infrastructure;
	}

    void Start()
    {
        Player player = (Player)GameObject.FindObjectOfType(typeof(Player));
        player.AddInfrastructure(Infrastructure);
    }
}
