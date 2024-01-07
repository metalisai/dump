// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public enum GridType
{
    ApartmentGrid,
    DealerGrid
}

[ExecuteInEditMode]
public class Grid : MonoBehaviour
{

    public int Width = 1;
    public int Height = 1;

    public GridType GridType;

    public bool HasCeiling = true;

    public bool CeilingContinueTop = false;

    public float CeilingHeight = 2.3f;

    public const float CellSize = 0.75f; // change this to change grid cell size

    public GridCell[,] GridGroundCells { get; set; }
    public GridCell[,] GridCeilingCells { get; set; }

    public Housing Infrastructure { get; set; }

	[SerializeField]
    [HideInInspector]
	public List<GridCoordinates> InactiveFloorCells = new List<GridCoordinates> ();
    [SerializeField]
    [HideInInspector]
    public List<GridCoordinates> InactiveCeilingCells = new List<GridCoordinates>();

    private int CeilCellsUp
    {
        get { return Height - (CeilingContinueTop ? 0 : 1); }
    }

    static private Grid _nullGrid;
    public static Grid NullGrid
    {
        get
        {
            if (_nullGrid == null)
            {
                var go = new GameObject();
                Grid grid = go.AddComponent<Grid>();
                grid.GridGroundCells = new GridCell[0, 0];
                grid.HasCeiling = false;
                grid.Infrastructure = null;
                _nullGrid = grid;
            }
            return _nullGrid;
        }
    }

    // Use this for initialization
    void Start()
    {
		//Debug.Log ("Grid start");
        CreateCells();

        InactiveCeilingCells.ForEach(x => GridCeilingCells[x.x,x.y].Filled = true);
        InactiveFloorCells.ForEach(x => GridGroundCells[x.x, x.y].Filled = true);
    }

    void CreateCells()
    {
        if (GridType == GridType.ApartmentGrid)
        {
            CreateFloorCells(GridCellType.FloorCell);

            if (HasCeiling)
            {
                // create ceiling cells
                GridCeilingCells = new GridCell[Width - 1, CeilCellsUp];
                for (int i = 0; i < GridCeilingCells.GetLength(0); i++)
                {
                    for (int j = 0; j < GridCeilingCells.GetLength(1); j++)
                    {
                        GridCeilingCells[i, j] = new GridCell()
                        {
                            Center = gameObject.transform.position + new Vector3((i + 1) * CellSize, CeilingHeight, (j + 1) * CellSize),
                            Grid = this,
                            Type = GridCellType.CeilingCell,
                            Coord = new GridCoordinates() { x = i, y = j }
                        };
                        //Debug.Log("init i: " + i + " j:" + j);
                    }
                }
                // create ceiling collider
                if (GridCeilingCells.Length > 0 && transform.FindChild("ceiling") == null)
                {
                    GameObject ceilingcollider = new GameObject();
                    ceilingcollider.name = "ceiling";
                    ceilingcollider.layer = 8;
                    ceilingcollider.transform.parent = gameObject.transform;
                    ceilingcollider.transform.localPosition = Vector3.zero;
                    BoxCollider col = ceilingcollider.AddComponent<BoxCollider>();
                    col.center = new Vector3((Width - 1) * CellSize / 2 + CellSize / 2, CeilingHeight, CeilCellsUp * CellSize / 2 + CellSize / 2);
                    col.size = new Vector3((Width - 1) * CellSize, 0.1f, (Height - 1) * CellSize);
                }
            }
        }
        else if (GridType == GridType.DealerGrid)
        {
            CreateFloorCells(GridCellType.DealerCell);
        }
    }

    void CreateFloorCells(GridCellType type)
    {
        int layer = type == GridCellType.FloorCell ? 9 : 14;

        // create floor cells to store info
        GridGroundCells = new GridCell[Width, Height];
        for (int i = 0; i < GridGroundCells.GetLength(0); i++)
        {
            for (int j = 0; j < GridGroundCells.GetLength(1); j++)
            {
                GridGroundCells[i, j] = new GridCell()
                {
                    Center = gameObject.transform.position + new Vector3(i * CellSize + CellSize / 2, 0f, j * CellSize + CellSize / 2),
                    Grid = this,
                    Type = GridCellType.FloorCell,
                    Coord = new GridCoordinates() { x = i, y = j },
                    Filled = IsCellBlocked(gameObject.transform.position + new Vector3(i * CellSize + CellSize / 2, 0f, j * CellSize + CellSize / 2))
                };
                //Debug.Log("init i: " + i + " j:" + j);
            }
        }
        // create floor collider
		if (GridGroundCells.Length > 0 && transform.FindChild("floor") == null)
        {
            GameObject floorcollider = new GameObject {layer = layer};
            floorcollider.name = "floor";
            floorcollider.transform.parent = gameObject.transform;
            floorcollider.transform.localPosition = Vector3.zero;
            BoxCollider col = floorcollider.AddComponent<BoxCollider>();
            col.center = new Vector3(Width * CellSize / 2, 0f, Height * CellSize / 2);
            col.size = new Vector3(Width * CellSize, 0.1f, Height * CellSize);
        }
    }

    bool IsCellBlocked(Vector3 cellCenter)
    {
        float halfCellSize = CellSize / 2 - 0.05f;
        int mask = 1 << 11;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // uncomment = 2x less precise and 2x faster
                /*if ((i == 0) != (j == 0))
                    continue;*/
                if (Physics.Raycast(cellCenter + new Vector3(halfCellSize * i, 5f, halfCellSize * j), Vector3.down, 6f, mask))
                    return true;
            }
        }
        return false;
    }

    void Update()
    {
        // to support hot recompile
        if(GridGroundCells == null)
        {
            CreateCells();
        }
    }

    GridCell[,] GetArray(GridCellType type)
    {
        switch(type)
        {
            case GridCellType.FloorCell:
                return GridGroundCells;
            case GridCellType.DealerCell:
                return GridGroundCells;
            case GridCellType.CeilingCell:
                return GridCeilingCells;
            default:
                return null;
        }

    }

    public bool IsInGrid(Vector3 position, GridCellType type)
    {
        switch (type)
        {
            case GridCellType.FloorCell:
                return position.x >= gameObject.transform.position.x &&
                    position.x <= gameObject.transform.position.x + Width * CellSize &&
                        position.z >= gameObject.transform.position.z &&
                        position.z <= gameObject.transform.position.z + Height * CellSize;
            case GridCellType.CeilingCell:
                return position.x >= gameObject.transform.position.x + CellSize / 2 &&
                    position.x <= gameObject.transform.position.x + CellSize / 2 + (Width - 1) * CellSize &&
                        position.z >= gameObject.transform.position.z + CellSize / 2 &&
                        position.z <= gameObject.transform.position.z + CellSize / 2 + (Height - 1) * CellSize;
            case GridCellType.DealerCell:
                return IsInGrid(position, GridCellType.FloorCell);
            default:
                return false;
        }

    }

    public GridCell GetClosestCell(Vector3 position, GridCellType type)
    {
        if(GridGroundCells.Length == 0) // for nullgrid
        {
            var cell = new GridCell();
            cell.Grid = this;
            return cell;
        }

        Vector3 gridZero;
        // TODO: this kinda bad..
        if (type != GridCellType.CeilingCell)
            gridZero = gameObject.transform.position;
        else
            gridZero = transform.position + new Vector3(CellSize / 2f, CeilingHeight, CellSize / 2f);

        int i = (int)((position.x - gridZero.x) / CellSize);
        int j = (int)((position.z - gridZero.z) / CellSize);
        var cellarray = GetArray(type);
        i = Mathf.Clamp(i, 0, cellarray.GetLength(0) - 1);
        j = Mathf.Clamp(j, 0, cellarray.GetLength(1) - 1);

        return cellarray[i, j];
    }

    private GridCoordinates GetBestCoordinateForShape(GridShape shape, Vector3 position, GridCellType type)
    {
        GridCoordinates shapeCenter = GetCenterCoord(shape.Shape.GetLength(1), shape.Shape.GetLength(0));
        GridCoordinates coord = GetClosestCell(position, type).Coord;
        coord.xHalf = shapeCenter.xHalf;
        coord.yHalf = shapeCenter.yHalf;
        return coord;
    }

    public Vector3 GetPositionForShape(GridShape shape, Vector3 position, GridCellType type)
    {
        return ToWorldPoint(GetBestCoordinateForShape(shape, position, type), type);
    }

    // checks if shape fits in the given position
    public bool Fits(GridShape shape, Vector3 position, GridCellType type)
    {
        // calculate the local coordinates of the center of the shape
        GridCoordinates shapeCenter = GetCenterCoord(shape.Shape.GetLength(1), shape.Shape.GetLength(0));
        // calculate the gridcoordinates for shape
        GridCoordinates coord = GetBestCoordinateForShape(shape, position, type);

        // loop through all the cells the shape should occupy
        var cellArray = GetArray(type);

        for (int i = 0; i < shape.Shape.GetLength(0); i++) // i = row (height)
        {
            for (int j = 0; j < shape.Shape.GetLength(1); j++) // j = column (width)
            {
                int x = coord.x + (j - shapeCenter.x); // calculates the grid width coordinate
                int y = coord.y - (i - shapeCenter.y + (shapeCenter.yHalf ? 0 : 1)); // calculates the grid height coordinate

                var fits = x >= 0 && x < cellArray.GetLength(0) && y >= 0 && y < cellArray.GetLength(1);
                if (fits)
                {
                    if (shape.Shape[i, j]) // only check if cell is empty when the shape actually needs that cell
                        fits = !cellArray[x, y].Filled;
                    Debug.DrawLine(cellArray[x, y].Center, cellArray[x, y].Center + Vector3.up * 5, cellArray[x, y].Filled ? Color.red : Color.green);
                    //Debug.Log("broke at i: "+i+" j: "+j);
                }

                if (!fits) // found a cell that doesnt fit, no point to continue
                {
                    return false;
                }
            }
        }

        return true;
    }

    public Vector3 PlaceShape(GridShape shape, Vector3 position, GridCellType type)
    {
        // calculate the local coordinates of the center of the shape
        GridCoordinates shapeCenter = GetCenterCoord(shape.Shape.GetLength(1), shape.Shape.GetLength(0));
        // calculate the gridcoordinates for shape
        GridCoordinates coord = GetBestCoordinateForShape(shape, position, type);
        // world point of the center the object will be placed at
        Vector3 center = ToWorldPoint(coord, type);

        //Debug.Log (shapeCenter.xHalf+" "+shapeCenter.yHalf);

        if (!Fits(shape, position, type))
        { // safety measure
            Debug.LogError("Check if object fits before placing it!");
            return Vector3.zero;
        }

        var cellArray = GetArray(type);

        for (int i = 0; i < shape.Shape.GetLength(0); i++) // i = row (height)
        {
            for (int j = 0; j < shape.Shape.GetLength(1); j++) // j = column (width)
            {
                int x = coord.x + (j - shapeCenter.x); // calculates the grid width coordinate
                int y = coord.y - (i - shapeCenter.y + (shapeCenter.yHalf ? 0 : 1)); // calculates the grid height coordinate

                if (shape.Shape[i, j])
                {
                    cellArray[x, y].Filled = true;
                    // create a one-one realationship
                    shape.Cells.Add(cellArray[x, y]);
                }
            }
        }

        return center;
    }

    public static GridCoordinates GetCenterCoord(int width, int height)
    {
        GridCoordinates coord = new GridCoordinates();
        coord.x = width / 2;
        coord.y = height / 2;
        coord.xHalf = width % 2 == 1;
        coord.yHalf = height % 2 == 1;
        return coord;
    }

    public Vector3 ToWorldPoint(GridCoordinates coord, GridCellType type)
    {
        switch (type)
        {
            case GridCellType.FloorCell:
                return transform.position + new Vector3(coord.x * CellSize + (coord.xHalf ? CellSize / 2f : 0f), 0f, coord.y * CellSize + (coord.yHalf ? CellSize / 2f : 0f));
            case GridCellType.CeilingCell:
                return transform.position + new Vector3(CellSize / 2f, CeilingHeight, CellSize / 2f) + new Vector3(coord.x * CellSize + (coord.xHalf ? CellSize / 2f : 0f), 0f, coord.y * CellSize + (coord.yHalf ? CellSize / 2f : 0f));
            case GridCellType.DealerCell:
                return ToWorldPoint(coord, GridCellType.FloorCell);
            default:
                return Vector3.zero;
        }
    }


    void OnDrawGizmos() // draws the debug gizmos in editor
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < Width + 1; i++)
        {
            Gizmos.DrawLine(gameObject.transform.position + new Vector3(CellSize * i, 0f, 0f), gameObject.transform.position + new Vector3(CellSize * i, 0f, Height * CellSize));
        }
        for (int i = 0; i < Height + 1; i++)
        {
            Gizmos.DrawLine(gameObject.transform.position + new Vector3(0f, 0f, CellSize * i), gameObject.transform.position + new Vector3(Width * CellSize, 0f, CellSize * i));
        }

        Gizmos.color = Color.red;
        if (HasCeiling)
        {
            for (int i = 0; i < Width; i++)
            {
                Gizmos.DrawLine(gameObject.transform.position + new Vector3(CellSize / 2, 0f, CellSize / 2) + new Vector3(CellSize * i, CeilingHeight, 0f), gameObject.transform.position + new Vector3(CellSize / 2, 0f, CellSize / 2) + new Vector3(CellSize * i, CeilingHeight, CeilCellsUp * CellSize));
            }
            for (int i = 0; i < (CeilCellsUp+1); i++)
            {
                Gizmos.DrawLine(gameObject.transform.position + new Vector3(CellSize / 2, 0f, CellSize / 2) + new Vector3(0f, CeilingHeight, CellSize * i), gameObject.transform.position + new Vector3(CellSize / 2, 0f, CellSize / 2) + new Vector3((Width - 1) * CellSize, CeilingHeight, CellSize * i));
            }
        }

        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
		foreach (var cell in InactiveCeilingCells) {

            if (GridCeilingCells == null || GridCeilingCells[cell.x, cell.y] == null)
                return;

                var ret = GridCeilingCells[cell.x,cell.y].Center;
			Gizmos.DrawCube (ret, new Vector3(0.75f,0f,0.75f));
		}
        foreach (var cell in InactiveFloorCells)
        {
            if(GridCeilingCells == null || GridGroundCells[cell.x, cell.y] == null)
            {
                return;
            }

            var ret = GridGroundCells[cell.x, cell.y].Center;
            Gizmos.DrawCube(ret, new Vector3(0.75f, 0f, 0.75f));
        }

        if (!Application.isPlaying) return;
        Gizmos.color = Color.cyan;
        foreach (var cell in GridGroundCells)
        {
            if (cell.Effects.Any())
            {
                Gizmos.DrawLine(cell.Center, cell.Center + Vector3.up);
            }
        }
    }
}
