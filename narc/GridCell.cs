// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;

// TODO: should use struct if at all possible (optimization)
public class GridCell
{
    public bool Filled = false;
    public Vector3 Center;
    public Grid Grid;
    public GridCellType Type;
    public GridCoordinates Coord;
    public List<GridEffect> Effects = new List<GridEffect>();
}

public enum GridCellType
{
    FloorCell,
    CeilingCell,
    DealerCell,
	All
}
