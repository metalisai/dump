// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum PlayerPropertyTextProperty
{
    Cash,
    BankMoney,
    WeedStorageAmount,
    GardenerCount,
    DealerCount
}

[RequireComponent(typeof(Text))]
public class PlayerPropertyText : MonoBehaviour
{
    public PlayerPropertyTextProperty TextProperty;

    private Text _text;
    private Player _player;

    void Start()
    {
        _text = GetComponent<Text>();
        _player = FindObjectOfType<Player>();
        GameTime.OnTick += UpdatePropery;
    }

    string ShortWeight(int weightInGrams)
    {

        if (weightInGrams > 1000000)
        {
            return (weightInGrams / 1000000).ToString() + " t";
        }
        else if(weightInGrams > 1000)
        {
            return (weightInGrams / 1000).ToString() + " kg";
        }
        else
        {
            return weightInGrams.ToString() + " g";
        }
    }

	void UpdatePropery () {
	    switch (TextProperty)
	    {
	        case PlayerPropertyTextProperty.BankMoney:
                _text.text = _player.BankMoney.ToString("### ### ### ### ##0.00 $");
                break;
            case PlayerPropertyTextProperty.Cash:
                _text.text = _player.Cash.ToString("### ### ### ### ##0.00 $");
	            break;
            case PlayerPropertyTextProperty.WeedStorageAmount:
                _text.text = ShortWeight(_player.Infrastructures.Sum(x => x.GetWeedAmount()));
	            break;
            case PlayerPropertyTextProperty.DealerCount:
                _text.text = _player.Infrastructures.Sum(x => x.Dealers.Count).ToString();
                break;
            case PlayerPropertyTextProperty.GardenerCount:
                _text.text = _player.Infrastructures.Sum(x => x.Gardeners.Count).ToString();
                break;
            default:
	            _text.text = 0.ToString();
	            break;
	    }
	}

    void OnDestroy()
    {
        GameTime.OnTick -= UpdatePropery;
    }
}
