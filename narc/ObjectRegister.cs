// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public enum PlaceableObjectType
{
    WeedPlant,
    Storage,
    Lamp200W,
    Lamp400W,
    Lamp800W,
    Dealer,
    Gardener
}

// you can ask this class to create instance of an object for you
public class ObjectRegister : MonoBehaviour
{

	public GameObject GrowingPlant;
	public GameObject Storage;
    public GameObject Lamp200W;
    public GameObject Lamp400W;
    public GameObject Lamp800W;
    public GameObject Dealer;
    public GameObject Gardener;

    private static ObjectRegister _current;

    void Start()
    {
        _current = this;
    }

	public PlaceableObject InstGetPlacableObject (PlaceableObjectType pObject)
	{
		switch (pObject) {

		case PlaceableObjectType.WeedPlant:
			return GrowingPlant.GetComponent<PlaceableObject>();

		case PlaceableObjectType.Storage:
			return Storage.GetComponent<PlaceableObject>();

        case PlaceableObjectType.Lamp200W:
            return Lamp200W.GetComponent<PlaceableObject>();

        case PlaceableObjectType.Lamp400W:
            return Lamp400W.GetComponent<PlaceableObject>();

        case PlaceableObjectType.Lamp800W:
            return Lamp800W.GetComponent<PlaceableObject>();

        case PlaceableObjectType.Dealer:
            return Dealer.GetComponent<PlaceableObject>();

        case PlaceableObjectType.Gardener:
            return Gardener.GetComponent<PlaceableObject>();

        default:
            Debug.LogError("Tried to access unknown placeable object! (Did you forget to put it into ObjectRegister.cs ?)");
			return null;
		}
	}

	public GameObject InstGetGhostObject (PlaceableObjectType pObject)
	{
		return (InstGetPlacableObject(pObject)).GhostObject.gameObject;
	}

	public static PlaceableObject GetPlacableObject(PlaceableObjectType pObject)
	{
		return _current.InstGetPlacableObject (pObject);
	}

	public static GameObject GetGhostObject(PlaceableObjectType pObject)
	{
		return _current.InstGetGhostObject (pObject);
	}

	public static PlaceableObject CreatePlacableObject(PlaceableObjectType pObject)
	{
		var go = (GameObject)GameObject.Instantiate(_current.InstGetPlacableObject (pObject).gameObject);
        var ret = go.GetComponent<PlaceableObject>();
        Debug.Assert(ret != null); // placeableobject need placeableobject script attached to it!
        ret.Init();
        return ret;
	}
}
