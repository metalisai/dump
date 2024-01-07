using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class MonoBehaviourManager
{
    static List<ManagedMonoBehaviour> _managedMonoBehaviours = new List<ManagedMonoBehaviour>();
    static List<ManagedMonoBehaviour> _tempAddList = new List<ManagedMonoBehaviour>();
    static List<ManagedMonoBehaviour> _tempRemoveList = new List<ManagedMonoBehaviour>();

    public static void Add(ManagedMonoBehaviour mmb)
    {
        _tempAddList.Add(mmb);
    }

    public static void Remove(ManagedMonoBehaviour mmb)
    {
        _tempRemoveList.Add(mmb);
    }

    public static void Update()
    {
        // update everything
        int count = _managedMonoBehaviours.Count;
        for(int i = 0; i < count; i++)
        {
            _managedMonoBehaviours[i].ManagedUpdate();
        }
        // remove removed behaviours
        // IF you ever add some custom destroying callback then note that this does not get called on application exit (because update won't be called after it's added to the remove list)
        count = _tempRemoveList.Count;
        for(int i = 0; i < count; i ++)
        {
            _managedMonoBehaviours.Remove(_tempRemoveList[i]);
        }
        // add new behaviours
        _tempRemoveList.Clear();
        count = _tempAddList.Count;
        for (int i = 0; i < count; i++)
        {
            _managedMonoBehaviours.Add(_tempAddList[i]);
        }
        _tempAddList.Clear();
    }

    public static void Clear()
    {
        _managedMonoBehaviours.Clear();
        _tempAddList.Clear();
        _tempRemoveList.Clear();
    }
}

public class ManagedMonoBehaviour : MonoBehaviour
{
    public ActiveObject ActiveObject;

    public virtual void ManagedUpdate() {}
    public void OnEnable()
    {
        MonoBehaviourManager.Add(this);
    }
    public void OnDisable()
    {
        MonoBehaviourManager.Remove(this);
    }
}
