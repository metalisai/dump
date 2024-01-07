// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;

public class RangeGridEffect : GridEffect
{
	public float Range;
	
	public override GridCell[] AffectedCells(Grid grid, Vector3 position)
	{
		List<GridCell> cells = new List<GridCell>();
		foreach(var cell in grid.GridGroundCells)
		{
			Vector2 a = new Vector2(position.x,position.z);
			Vector2 b = new Vector2(cell.Center.x,cell.Center.z);
			if(Vector2.Distance(a, b) <= Range)
				cells.Add(cell);
		}
		return cells.ToArray();
	}
}
