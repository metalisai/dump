using UnityEngine;
using System.Collections;

public enum ObjectId
{
	Stick,
	Fireplace,
    RawMeat,
    CookedMeat,
    BurnedOrganicMatter
}

public class ObjectDatabase : MonoBehaviour {

	public GameObject stick;
	public GameObject fireplace;
    public GameObject rawMeat;
    public GameObject cookedMeat;
    public GameObject burnedOrganicMatter;

	static ObjectDatabase current;

	void Start()
	{
		current = this;
	}

	public static GameObject CreateObject(ObjectId oid)
	{
		switch(oid)
		{
		case ObjectId.Stick:
			return GameObject.Instantiate(current.stick);
		case ObjectId.Fireplace:
			return GameObject.Instantiate(current.fireplace);
        case ObjectId.RawMeat:
            return GameObject.Instantiate(current.rawMeat);
        case ObjectId.CookedMeat:
            return GameObject.Instantiate(current.cookedMeat);
        case ObjectId.BurnedOrganicMatter:
            return GameObject.Instantiate(current.burnedOrganicMatter);
		default:
			Debug.LogError("Tried to create an object that doesn't exist!");
			return null;
		}
	}
}
