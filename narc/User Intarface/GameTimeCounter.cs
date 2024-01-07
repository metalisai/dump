// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.UI;

public class GameTimeCounter : MonoBehaviour
{

    public enum GameTimeType
    {
        DayTime,
        Day
    }

    public GameTimeType type;
    private Text _text;

	void Start ()
	{
	    _text = GetComponent<Text>();
        GameTime.OnTick += UpdatePropery;
	}
	
	void UpdatePropery () {
        switch(type)
        {
            case GameTimeType.DayTime:
                _text.text = string.Format("{0}:{1}", GameTime.Hour.ToString("D2"), GameTime.Minute.ToString("D2"));
                break;
            case GameTimeType.Day:
                _text.text = string.Format("Day {0}", GameTime.Day);
                break;
            default:
                break;
        }
	}

    void OnDestroy()
    {
        GameTime.OnTick -= UpdatePropery;
    }
}
