// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Store : MonoBehaviour {

	private Player _player;
	private ItemPlacer _placer;

	private ItemInStore _curItem;

    public List<ItemInStore> StoreItems = new List<ItemInStore>();

    public static Store Instance;

	void Awake()
	{
		_player = (Player)GameObject.FindObjectOfType (typeof(Player));
		_placer = (ItemPlacer)GameObject.FindObjectOfType (typeof(ItemPlacer));

        if (Instance != null)
            Debug.LogError("Store object should be singleton right now.. !");
        Instance = this;
	}

    public bool PeekItem(ItemInStore item)
    {
        if (_player.Cash >= item.Price)
        {
            return true;
        }
        return false;
    }

	public bool BuyItem(ItemInStore item)
	{
		if (PeekItem(item)) {
			_player.Cash -= item.Price;
			return true;
		}
		return false;
	}

	public void StartPlacing(ItemInStore item)
	{
		_placer.StartPlacing (item.PlaceableObject, PlacerEvent);
		_curItem = item;
		
	}

	public void PlacerEvent(PlacerEventId eventId)
	{
		switch (eventId) {

		case PlacerEventId.OnTryingPlace:
		{
			if(_curItem != null && BuyItem(_curItem))
				_placer.Place();
			else
				Debug.Log("No money");
			break;
		}
		case PlacerEventId.OnEndPlace:
		{
			_curItem = null;
			break;
		}

		default:
			return;
		}
	}

    public ItemInStore GetStoreItemOfItem(PlaceableObjectType itemType)
    {
        return StoreItems.FirstOrDefault(x => x.PlaceableObject == itemType);
    }
}

[System.Serializable]
public class ItemInStore
{
	public int Price;
	public string Name;
	public PlaceableObjectType PlaceableObject;
    public Sprite Image;
    public string Description;
}
