using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;

using UnityEngine;
using System.Runtime.InteropServices;

public struct GridMask
{
    public struct PivotCoord
    {
        /// <summary>
        /// Offset from 0,0
        /// </summary>
        public CellCoord offsetFrom;
        public bool halfX;
        public bool halfY;
    }

    public struct MaskCell
    {
        public bool Occupied;
    }

    public PivotCoord Pivot;
    public int Rotation;
    public MaskCell[,] Cells;

    static MaskCell[,] CellsFromString(string instr)
    {
        if(string.IsNullOrEmpty(instr))
        {
            var eret = new MaskCell[1, 1];
            eret[0, 0].Occupied = true;
            return eret;
        }

        string[] lines = instr.Split(' ');
        int width = lines.Max(x => x.Length);
        var ret = new MaskCell[width, lines.Length];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < lines.Length; j++)
            {
                string str = lines[j];
                ret[i, lines.Length - j - 1].Occupied = i >= str.Length ? false : str[i] == '1' ? true : false;
            }
        }
        return ret;
    }

    public GridMask(string instr)
    {
        Cells = CellsFromString(instr);
        int width = Cells.GetLength(0);
        int height = Cells.GetLength(1);
        Pivot = new PivotCoord() { // at least it's compact
            offsetFrom = new CellCoord
            {
                x = width % 2 == 0 ? width / 2 - 1 : width / 2,
                y = height % 2 == 0 ? height / 2 - 1 : height / 2
            },
            halfX = width % 2 == 0,
            halfY = height % 2 == 0
        };
        Rotation = 0;
    }

    public GridMask(string instr, PivotCoord pivot)
    {
        Cells = CellsFromString(instr);
        Pivot = pivot;
        Rotation = 0;
    }

    public void RotateRight()
    {
        MaskCell[,] newArray = new MaskCell[Cells.GetLength(1), Cells.GetLength(0)];
        for (int i = 0; i < newArray.GetLength(0); i++)
        {
            for (int j = 0; j < newArray.GetLength(1); j++)
            {
                newArray[i, j] = Cells[newArray.GetLength(1) - 1 - j, i];
            }
        }
        Cells = newArray;
        Pivot = new PivotCoord() { offsetFrom = new CellCoord() { x = Pivot.offsetFrom.y, y = newArray.GetLength(1) - 1 - Pivot.offsetFrom.x },
            halfX = Pivot.halfX,
            halfY = Pivot.halfY
        };
        Rotation += 1;
        Rotation = Rotation >= 0 ? Rotation % 4 : 4 + Rotation % 4;
    }

    public void RotateLeft()
    {
        MaskCell[,] newArray = new MaskCell[Cells.GetLength(1), Cells.GetLength(0)];
        for (int i = 0; i < newArray.GetLength(0); i++)
        {
            for (int j = 0; j < newArray.GetLength(1); j++)
            {
                newArray[i, j] = Cells[j, newArray.GetLength(0) - 1 - i];
            }
        }
        Cells = newArray;
        Pivot = new PivotCoord() { offsetFrom = new CellCoord() { x = newArray.GetLength(0) - 1 - Pivot.offsetFrom.y, y = Pivot.offsetFrom.x },
            halfX = Pivot.halfX,
            halfY = Pivot.halfY
        };
        Rotation -= 1;
        Rotation = Rotation >= 0 ? Rotation % 4 : 4 + Rotation % 4;
    }

    public void SetRotation(int rotation)
    {
        Assert.IsTrue(rotation >= 0, "Rotation can't be negative! (even though it should work)");
        rotation = rotation >= 0 ? rotation % 4 : 4 + rotation % 4;
        while (Rotation != rotation)
        {
            RotateRight();
        }
    }

    public Vector3 GetSnappedPosition(Vector3 inPos)
    {
        Vector3 ret;
        var cellCoord = Game.pathFinding.grid.WorldPosToCellCoord(inPos, this);
        ret = Game.pathFinding.grid.GetSnappedPosition(cellCoord, this);
        return ret;
    }
}

public enum GridCellFlag : byte
{
    Free = 0,
    Occupied = 1,
    Destroyable = 2,
    Visited = 4,
    ConnectedToWaypointGraph = 8
}

public struct GridCellMetadata
{
    public struct GridCellSubgraphEdge
    {
        public int edgeIndex;
        public WaypointGraph edgeTo;
    }
    /// <summary>
    /// In case cell is blocked, this will point to the blocking object
    /// </summary>
    public PlaceableObject placeableObject;
    /// <summary>
    /// Center world coordinate of the cell
    /// </summary>
    public Vector3 center;
    /// <summary>
    /// Connections to subgraphs
    /// </summary>
    public List<GridCellSubgraphEdge> subgraphEdges;
}

[System.Serializable]
public class WaypointGraph
{
    public WaypointGraph(Transform transform)
    {
        this.transform = transform;
    }

    [System.Serializable]
    public struct CellEdge
    {
        [SerializeField]
        public int waypoint;
        [SerializeField]
        public CellCoord offsetFromCenter;
    }
    [SerializeField]
    public Vector3[] waypoints;
    [SerializeField]
    public int[] waypointEdges;
    [SerializeField]
    public CellEdge[] cellEdges;
    [NonSerialized]
    public Transform transform;
}

public class PathFindingGrid
{
    //readonly Vector3 origin;
    public readonly float gridScale;

    public readonly int width;
    public readonly int height;

    GridCellFlag[,] cells;
    GridCellMetadata[,] metaCells;

    public PathFindingGrid(Vector3 origin, float scale, int width, int height)
    {
        //this.origin = origin;
        this.gridScale = scale;
        this.width = width;
        this.height = height;
        cells = new GridCellFlag[width,height];
        metaCells = new GridCellMetadata[width, height];

        // init cells
        for(int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                cells[i,j] = GridCellFlag.Free;
                metaCells[i, j].center = origin + new Vector3(i*gridScale+gridScale/2.0f, 0.0f, j*gridScale + gridScale / 2.0f);
                metaCells[i, j].placeableObject = null;
            }
        }
    }

    public void AddSubgraphEdgeToCell(CellCoord cell, WaypointGraph graph, int edgeIndex)
    {
        if (metaCells[cell.x, cell.y].subgraphEdges == null)
        {
            metaCells[cell.x, cell.y].subgraphEdges = new List<GridCellMetadata.GridCellSubgraphEdge>();
        }
        metaCells[cell.x, cell.y].subgraphEdges.Add(new GridCellMetadata.GridCellSubgraphEdge() { edgeIndex = edgeIndex, edgeTo = graph });
        cells[cell.x, cell.y] |= GridCellFlag.ConnectedToWaypointGraph;
    }

    public void DisconnectGraphFromCell(CellCoord cell, WaypointGraph graph)
    {
        if (metaCells[cell.x, cell.y].subgraphEdges == null || graph == null)
            return;
        // TODO: non linq version?
        metaCells[cell.x, cell.y].subgraphEdges.RemoveAll(x => x.edgeTo == graph);
        if(metaCells[cell.x, cell.y].subgraphEdges.Count == 0)
        {
            // TODO: should the list be freed? player placing another structure on the cell is quite likely, so maybe not?
            cells[cell.x, cell.y] &= ~GridCellFlag.ConnectedToWaypointGraph;
        }
    }

    public GridCellFlag[,] GetCells()
    {
        Assert.IsTrue(cells != null);
        return cells;
    }

    public GridCellMetadata[,] GetMetaCells()
    {
        Assert.IsTrue(metaCells != null);
        return metaCells;
    }

    public bool IsOccupied(CellCoord coord)
    {
        return (cells[coord.x, coord.y] & GridCellFlag.Occupied) == GridCellFlag.Occupied;
    }

    public bool IsOccupied(CellCoord coord, GridMask mask)
    {
        for (int i = 0; i < mask.Cells.GetLength(0); i++)
        {
            for (int j = 0; j < mask.Cells.GetLength(1); j++)
            {
                int relativeX = coord.x + (i - mask.Pivot.offsetFrom.x);
                int relativeY = coord.y + (j - mask.Pivot.offsetFrom.y);
                 
                // cell out of bounds
                if(relativeX < 0 || relativeX >= cells.GetLength(0)
                    || relativeY < 0 || relativeY >= cells.GetLength(1))
                {
                    return true;
                }
                // needed free cell, but no free cell
                if(mask.Cells[i, j].Occupied && (cells[relativeX, relativeY] & GridCellFlag.Occupied) == GridCellFlag.Occupied)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ForceFreeCell(CellCoord coord)
    {
        cells[coord.x, coord.y] = GridCellFlag.Free;
        metaCells[coord.x, coord.y].placeableObject = null;
    }

    public void FreeCell(CellCoord coord, PlaceableObject pobj)
    {
        // pobj owns the cell, free it
        if (metaCells[coord.x, coord.y].placeableObject == pobj)
        {
            cells[coord.x, coord.y] = GridCellFlag.Free;
            metaCells[coord.x, coord.y].placeableObject = null;
        } // pobj doesn't own the cell but might have a graph connected to it
        else
        {
            DisconnectGraphFromCell(coord, pobj.waypointGraph);
        }
    }

    public void MarkOccupied(CellCoord coord, bool blockDestroyable, PlaceableObject owner)
    {
        cells[coord.x, coord.y] |= GridCellFlag.Occupied;
        if(blockDestroyable)
            cells[coord.x, coord.y] |= GridCellFlag.Destroyable;
        metaCells[coord.x, coord.y].placeableObject = owner;
    }

    //public Vector3 Ge

    /// <summary>
    /// Occupies space on the grid.
    /// </summary>
    /// <param name="coord"></param>
    /// <param name="mask"></param>
    /// <param name="occupiedCells">occupied cells will be added to the list</param>
    /// <remarks>Use only after checking with IsOccupied</remarks>
    /// <returns>grid-snapped world coordinates</returns>
    public Vector3 MarkOccupied(CellCoord coord, GridMask mask, bool blockDestroyable, PlaceableObject owner, List<CellCoord> occupiedCells = null)
    {
        for (int i = 0; i < mask.Cells.GetLength(0); i++)
        {
            for (int j = 0; j < mask.Cells.GetLength(1); j++)
            {
                int relativeX = coord.x + (i - mask.Pivot.offsetFrom.x);
                int relativeY = coord.y + (j - mask.Pivot.offsetFrom.y);

                Assert.IsTrue(relativeX >= 0 && relativeX < cells.GetLength(0));
                Assert.IsTrue(relativeY >= 0 && relativeY < cells.GetLength(1));

                CellCoord offsetFromCenter;
                offsetFromCenter.x = i - mask.Pivot.offsetFrom.x;
                offsetFromCenter.y = j - mask.Pivot.offsetFrom.y;
                var wpg = owner.waypointGraph;

                // TODO: rethink this decision
                metaCells[relativeX, relativeY].placeableObject = owner;
                if (occupiedCells != null)
                    occupiedCells.Add(new CellCoord() { x = relativeX, y = relativeY });
                if (mask.Cells[i, j].Occupied)
                {
                    cells[relativeX, relativeY] |= GridCellFlag.Occupied;
                    if(blockDestroyable)
                        cells[relativeX, relativeY] |= GridCellFlag.Destroyable;
                }
                if(wpg != null) // check for subgraph stuff
                {
                    int edgec = wpg.cellEdges.Length;
                    for (int edgeIndex = 0; edgeIndex < edgec; edgeIndex++) // check if any matches
                    {
                        CellCoord edgeCoord = wpg.cellEdges[edgeIndex].offsetFromCenter;
                        if (edgeCoord.x == offsetFromCenter.x && edgeCoord.y == offsetFromCenter.y)
                        {
#if DEBUG
                            if (mask.Cells[i, j].Occupied)
                                Debug.LogWarning("Subgraph edge should not be connected to occupied cell (" + owner + ")");
#endif
                            AddSubgraphEdgeToCell(new CellCoord() { x = relativeX, y = relativeY }, wpg, wpg.cellEdges[edgeIndex].waypoint);
                            //cells[relativeX, relativeY] |= GridCellFlag.ConnectedToWaypointGraph;
                            break; // only one allowed for now
                        }
                    }
                }
            }
        }
        //Debug.Log(mask.Pivot.halfX+" ss "+ mask.Pivot.halfY);
        //Debug.LogFormat("bases {0} {1}", mask.Pivot.offsetFrom.x, mask.Pivot.offsetFrom.y);
        Vector3 halfOffset = GetHalfOffset(mask);
        return metaCells[coord.x, coord.y].center + halfOffset;
    }

    public CellCoord WorldPosToCellCoord(Vector3 inPos, GridMask mask)
    {
        Vector3 halfOffset = GetHalfOffset(mask);
        var cellCoord = Game.pathFinding.WorldCoordToGrid(inPos - halfOffset);
        cellCoord.x = Mathf.Clamp(cellCoord.x, 0, Game.pathFinding.grid.width - 1);
        cellCoord.y = Mathf.Clamp(cellCoord.y, 0, Game.pathFinding.grid.height - 1);
        return cellCoord;
    }

    public Vector3 GetHalfOffset(GridMask mask)
    {
        Vector3 halfOffset = new Vector3(mask.Pivot.halfX ? gridScale / 2 : 0.0f, 0.0f, mask.Pivot.halfY ? gridScale / 2 : 0.0f);
        halfOffset = Quaternion.AngleAxis(mask.Rotation * 90.0f, Vector3.up) * halfOffset;
        return halfOffset;
    }

    public Vector3 GetSnappedPosition(CellCoord coord, GridMask mask)
    {
        Vector3 halfOffset = new Vector3(mask.Pivot.halfX ? gridScale / 2 : 0.0f, 0.0f, mask.Pivot.halfY ? gridScale / 2 : 0.0f);
        halfOffset = Quaternion.AngleAxis(mask.Rotation * 90.0f, Vector3.up) * halfOffset;
        return metaCells[coord.x, coord.y].center + halfOffset;
    }
}
