using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public struct StoredResource
{
    public ResourceType resourceType;
    public string resourceName;
    public int resourceAmount;

    public static string GetResourceName(ResourceType rtype)
    {
        return Enum.GetName(typeof(ResourceType), rtype);
    }
}

public struct StorageResourceConstraint
{
    public ResourceType? resourceType;
    public string resourceName;
    public int resourceAmount;
}

public interface IResourceStore
{
    bool IsGenericStorage { get; }
    int GetResourceCount(ResourceType type);
    bool GetConstraint(ResourceType rtype, out int value);
    int TakeResource(ResourceType type, int amount, bool allowLess = true);
    StoredResource[] GetStoredResources();
    StorageResourceConstraint[] GetConstraints();
    bool StoreResource(ResourcePack resource);
    GameObject rsgameObject { get; }
    int GetSpareResourceCount(ResourceType type);
}

[ObjectLogic(ActiveObject.ObjectTag.Warehouse)]
public class Warehouse : ManagedMonoBehaviour, ISelectable, IResourceStore
{
    public const int MaxStoredTypes = 24;
    public const int MaxConstraints = 8;

    Dictionary<ResourceType, int> StoredResources = new Dictionary<ResourceType, int>();
    StorageResourceConstraint[] ResourceConstraints = new StorageResourceConstraint[MaxConstraints];

    public bool IsGenericStorage { get { return true; } }
    public GameObject rsgameObject { get { return gameObject; } }

    public delegate void WarehouseDelegate();
    public event WarehouseDelegate ContentsChanged;
    public event WarehouseDelegate ConstraintsChanged;

    void Start()
    {
        gameObject.AddTag(ActiveObject.ObjectTag.Selectable);
    }

    public bool StoreResource(ResourcePack resource)
    {
        if (StoredResources.Keys.Count > 24)
            return false;

        if (!StoredResources.ContainsKey(resource.ResourceType))
            StoredResources.Add(resource.ResourceType, 0);
        StoredResources[resource.ResourceType] += resource.Amount;
        if (ContentsChanged != null)
            ContentsChanged();
        return true;
    }

    public int GetResourceCount(ResourceType type)
    {
        int val;
        if (StoredResources.TryGetValue(type, out val))
            return val;
        return 0;
    }

	public bool GetConstraint(ResourceType rtype, out int value)
	{
		for(int i = 0; i < ResourceConstraints.Length; i++)
		{
			if (ResourceConstraints [i].resourceType != null && ResourceConstraints [i].resourceType == rtype) {
				value = ResourceConstraints [i].resourceAmount;
				return true;
			}
		}
		value = 0;
		return false;
	}

	public int GetSpareResourceCount(ResourceType type)
	{
		int val;
		if (!StoredResources.TryGetValue (type, out val))
			return 0;
		int constraint = 0;
		GetConstraint(type, out constraint);
		int spare = Mathf.Max(0, val - constraint);
		return spare;
	}

    public int TakeResource(ResourceType type, int amount, bool allowLess = true)
    {
        int val;
        if(StoredResources.TryGetValue(type, out val))
        {
            int take = Mathf.Min(val, amount);
            if (take < amount && !allowLess)
                return 0;
            StoredResources[type] -= amount;
            if (StoredResources[type] == 0)
                StoredResources.Remove(type);
            if (ContentsChanged != null)
                ContentsChanged();
            return take;
        }
        return 0;
    }

    public void SetConstraint(int index, ResourceType? type, int amount)
    {
        Debug.LogFormat("Constraint {0} set to {1} {2}", index, type, amount);
        ResourceConstraints[index].resourceType = type;
        ResourceConstraints[index].resourceAmount = amount;
        if(type != null)
            ResourceConstraints[index].resourceName = Enum.GetName(typeof(ResourceType), type);
        if (ConstraintsChanged != null)
            ConstraintsChanged.Invoke();
    }

    // TODO: should probably just not return the constraints with null types to avoid null checking on the user end
    public StorageResourceConstraint[] GetConstraints()
    {
        return ResourceConstraints;
    }

    public StoredResource[] GetStoredResources()
    {
        StoredResource[] ret = new StoredResource[StoredResources.Count];
        int index = 0;
        foreach(var key in StoredResources.Keys)
        {
            ret[index].resourceType = key;
            ret[index].resourceName = Enum.GetName(typeof(ResourceType), key);
            ret[index].resourceAmount = StoredResources[key];
			index++;
        }
        return ret;
    }

    public void OnSelected()
    {
        UI.WarehouseInterface.UpdateForWarehouse(this);
        UI.WarehouseInterface.Show();
    }

    public void OnDeSelected()
    {
        UI.WarehouseInterface.Hide();
    }
}
