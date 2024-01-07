// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;

public abstract class GridEffect : MonoBehaviour
{
	public GridEffectType Effect;
	public GridEffectApplyType ApplyType;
	public List<GridCell> EffectAppliedCells = new List<GridCell>();
	public abstract GridCell[] AffectedCells(Grid grid, Vector3 position);
    public PlaceableObject ApplierPlaceableObject;

    bool _enabled = false;

    protected void Awake()
    {
        ApplierPlaceableObject = GetComponent<PlaceableObject>();
        if(ApplierPlaceableObject == null)
            Debug.LogError("No PlaceableObject found for GridEffect in gameobject named "+gameObject.name);
    }

    public void EnableEffect()
    {
        if (_enabled)
            return;

        Debug.Assert(ApplierPlaceableObject.Cells[0].Grid != null);
        GridCell[] affectedCells = AffectedCells(ApplierPlaceableObject.Cells[0].Grid, transform.position);
        foreach (var cell in affectedCells)
        {
            cell.Effects.Add(this);
            EffectAppliedCells.Add(cell);
            ApplierPlaceableObject.AffectedCells.Add(cell);
        }
        _enabled = true;
    }

    public void DisableEffect()
    {
        if (_enabled == false)
            return;

        foreach (var cell in EffectAppliedCells)
        {
            cell.Effects.RemoveAll(x => x.ApplierPlaceableObject == ApplierPlaceableObject);
        }
    }
}

public enum GridEffectType
{
    None,
	LampLight200W,
    LampLight400W,
    LampLight800W
}

public enum GridEffectApplyType
{
	CenterOnly,
	AllCells
}