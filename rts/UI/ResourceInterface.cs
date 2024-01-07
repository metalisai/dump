using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ResourceInterface : ManagedMonoBehaviour {

    struct RIResource
    {
        public ResourceType type;
        public Text text;
    }

    List<RIResource> cachedTexts = new List<RIResource>();

    void Awake()
    {
        GameTime.SlowTick += SlowUpdate;
    }

    void OnDestroy()
    {
        GameTime.SlowTick -= SlowUpdate;
    }

	void Start () {
        var resources = Enum.GetNames(typeof(ResourceType));
        for(int i = 0; i < resources.Length; i ++)
        {
            var go = GameObject.Instantiate(transform.GetChild(0).gameObject);
            go.transform.SetParent(transform);
            var vt = go.GetComponent<Text>();
            vt.text = resources[i];
            cachedTexts.Add(new RIResource() { text = vt, type = (ResourceType)i });
        }
        GameObject.Destroy(transform.GetChild(0).gameObject);
	}

    void SlowUpdate(float dt)
    {
        foreach(var r in cachedTexts)
        {
            r.text.text = Enum.GetName(typeof(ResourceType), r.type) + ": " + Game.Instance.GetResourceAmount(r.type);
        }
    }
}
