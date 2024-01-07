using UnityEngine;
using System.Collections;

public class GlobalSettings : MonoBehaviour {

    private static GlobalSettings _current;

    public static Material BlueprintMaterial
    {
        get
        {
            return _current.BPMaterial;
        }
    }

    public static GameObject ItemDesc
    {
        get
        {
            return _current.ItemDescription;
        }
    }

    public Material BPMaterial;
    public GameObject ItemDescription;

	void Start () {
        _current = this;
	}
}
