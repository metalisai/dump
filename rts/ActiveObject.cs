using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ActiveObject : ManagedMonoBehaviour {

    [System.Serializable]
    public enum ObjectTag
    {
        Empty = 0,
        ElectricGenerator,
        ElectricPole,
        ElectricLamp,
        ElectricBattery,
        ElectricTurret,
        Nexus,
        Warehouse,
        Destroyable,
        Ally,
        Enemy,
        Progressable,
        Selectable,
		LaserTurret,
        Wall,
        HasSight,
        ArtilleryTurret,
        WoodGenerator,
        WorkerAI,
        PFDummy
    }

    public List<ObjectTag> ObjectTags;

    public Destroyable.DestroyableSide GetSide()
    {
        var result = ObjectTags.Where(x => x == ObjectTag.Ally || x == ObjectTag.Enemy).FirstOrDefault();
        return result == ObjectTag.Empty ? Destroyable.DestroyableSide.Ally : (result == ObjectTag.Ally ? Destroyable.DestroyableSide.Ally : Destroyable.DestroyableSide.Enemy);
    }

    public bool HasTag(ObjectTag tag)
    {
        for (int i = 0; i < ObjectTags.Count; i++)
            if (ObjectTags[i] == tag)
                return true;
        return false;
    }

    void Start()
    {
        name = name.Replace("(Clone)", "");
    }

    public void Activate()
    {
        Game.Instance.ActivateObject(this);
    }
}
