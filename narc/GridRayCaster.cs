// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public class GridRayCaster
{
    public static int TypeToMask(GridCellType type)
    {
        switch(type)
        {
            case GridCellType.FloorCell:
                return 1<<9;
            case GridCellType.CeilingCell:
                return 1<<8;
            case GridCellType.DealerCell:
                return 1<<14;
			case GridCellType.All:
				return 17152;
            default:
                Debug.LogError("Unknown GridCellType!");
                return -1;
        }
    }

	public static bool RayCastGrids(out GridRaycastInfo info, GridCellType type)
	{
		info = new GridRaycastInfo();
		Vector3 mousePos = Input.mousePosition; // get cursor position	
		Ray rayFromCursor = Camera.main.ScreenPointToRay(mousePos); // get ray from cursor position			
		RaycastHit hit; // if the ray hits, info will be stored here
		LayerMask mask = TypeToMask(type);
		//LayerMask mask = 1 << layer; // shift 1, so only floors layer is in mask
        // TODO: check if the apartment matches to the active one
		if (Physics.Raycast (rayFromCursor, out hit, 35f, mask))
		{
			info.Hitpoint = hit.point;
			Grid grid = hit.collider.gameObject.GetComponentInParent<Grid>();
			info.GridCell = grid==null? Grid.NullGrid.GetClosestCell(hit.point,type):grid.GetClosestCell(hit.point, type);
			Debug.DrawLine(hit.point,hit.point+Vector3.up);
			return true;
		}

		return false;
	}

}

public struct GridRaycastInfo
{
	public Vector3 Hitpoint;
	public GridCell GridCell;
}