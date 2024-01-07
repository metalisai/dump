using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

struct AstarCell
{
    public float gScore;
    public float fScore;
}

[System.Serializable]
public struct CellCoord
{
    public CellCoord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    [SerializeField]
    public int x;
    [SerializeField]
    public int y;

    public CellCoord? Left()
    {
        if (x <= 0)
            return null;
        return new CellCoord() { x = x - 1, y = y};
    }
    public CellCoord? Right()
    {
        if (x >= Game.pathFinding.grid.GetCells().GetLength(0)-1)
            return null;
        return new CellCoord() { x = x + 1, y = y };
    }
    public CellCoord? Down()
    {
        if (y <= 0)
            return null;
        return new CellCoord() { x = x , y = y - 1 };
    }
    public CellCoord? Up()
    {
        if (y >= Game.pathFinding.grid.GetCells().GetLength(1)-1)
            return null;
        return new CellCoord() { x = x, y = y + 1 };
    }
}

public class AStar
{
    readonly AstarCell[,] acells;
    readonly GridCellFlag[,] cells;
    readonly GridCellMetadata[,] mcells;
    readonly CellCoord[] neighs;

    CellCoord dest;
    HashSet<CellCoord> openSet = new HashSet<CellCoord>();
    Dictionary<CellCoord, CellCoord> cameFrom = new Dictionary<CellCoord, CellCoord>();
    HashSet<CellCoord> closedSet = new HashSet<CellCoord>();
    bool prepared = false;

    public AStar(GridCellFlag[,] cells, GridCellMetadata[,] mcells)
    {
        acells = new AstarCell[cells.GetLength(0), cells.GetLength(1)];
        this.cells = cells;
        this.mcells = mcells;
        neighs = new CellCoord[4];
        neighs[0] = new CellCoord() { x = -1, y = 0 };
        neighs[1] = new CellCoord() { x = 1, y = 0 };
        neighs[2] = new CellCoord() { x = 0, y = -1 };
        neighs[3] = new CellCoord() { x = 0, y = 1 };

        /*
        // set preallocation (had no performance benefit..)
        List<CellCoord> PreallocationList = Enumerable.Range(0, cells.GetLength(0)*cells.GetLength(1))
            .Select(x => new CellCoord() { x = 0, y = 0 }).ToList();
        closedSet = new HashSet<CellCoord>(PreallocationList);
        List<CellCoord> PreallocationList2 = Enumerable.Range(0, 1000)
            .Select(x => new CellCoord() { x = 0, y = 0 }).ToList();
        openSet = new HashSet<CellCoord>(PreallocationList);
        closedSet.Clear();
        openSet.Clear();*/

        /*neighs[4] = new CellCoord() { x = -1, y = 1 };
        neighs[5] = new CellCoord() { x = 1, y = 1 };
        neighs[6] = new CellCoord() { x = -1, y = -1 };
        neighs[7] = new CellCoord() { x = 1, y = -1 };*/
    }

    public void Prepare(CellCoord start, CellCoord dest)
    {
        closedSet.Clear();
        openSet.Clear();
        cameFrom.Clear();

        this.dest = dest;
        
        // if start cell is filled, try to find a way out through subgraph
        // if there is a way out, add it to openset
        // TODO: instead of going straight to exit, should also pathfind the subgraph itself
        if((cells[start.x, start.y] & GridCellFlag.Occupied) != 0)
        {
            var po = mcells[start.x, start.y].placeableObject;
            if ( po != null && po.waypointGraph != null && po.waypointGraph.cellEdges != null)
            {
                CellCoord? closest = null;
                float minSqDist = float.MaxValue;
                foreach(var edge in po.waypointGraph.cellEdges)
                {
                    var ofst = edge.offsetFromCenter;
                    float sqdist = Vector3.SqrMagnitude(mcells[start.x, start.y].center - mcells[start.x + ofst.x, start.y + ofst.y].center);
                    if (sqdist < minSqDist)
                    {
                        closest = new CellCoord(start.x + ofst.x, start.y + ofst.y);
                        minSqDist = sqdist;
                    }
                }
                if(closest != null)
                    openSet.Add(closest.Value);
            }
        }

        for (int i = 0; i < acells.GetLength(0); i++)
        {
            for (int j = 0; j < acells.GetLength(1); j++)
            {
                acells[i, j].gScore = float.MaxValue / 2.0f;
                acells[i, j].fScore = float.MaxValue / 2.0f;
            }
        }

        acells[start.x, start.y].gScore = 0.0f;
        acells[start.x, start.y].fScore = CellDist2(start, dest);
        openSet.Add(start);

        prepared = true;
    }

    // appears to be much fater for greater distances, but for some reason doesn't work well in more complex layouts
    float CellDist2(CellCoord start, CellCoord dest)
    {
        int a = start.x - dest.x;
        int b = start.y - dest.y;
        return a*a+b*b;
    }

    // very slow for long distances
    float CellDist(CellCoord start, CellCoord dest)
    {
        return (new Vector2(start.x, start.y)-new Vector2(dest.x, dest.y)).magnitude;
    }

    int maxO = 0;
    int maxC = 0;

    public List<CellCoord> FindPath(bool allowDestroy, out bool reachable)
    {
        Assert.IsTrue(prepared);
        prepared = false;
        //CellCoord first = openSet.First();
        CellCoord best = openSet.First();
        float bestScore = float.MaxValue;

        CellCoord current;
        current.x = 0;
        current.y = 0;

        while (openSet.Count > 0)
        {
            if (maxO < openSet.Count)
                maxO = openSet.Count;
            if (maxC < closedSet.Count)
                maxC = closedSet.Count;

            float curMin = float.MaxValue;
            foreach(var value in openSet)
            {
                if(acells[value.x, value.y].fScore < curMin)
                {
                    curMin = acells[value.x, value.y].fScore;//
                    current = value;
                }
            }

            if (current.x == dest.x && current.y == dest.y)
            {
                reachable = true;
                return ReconstructPath(current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach(var neigh in neighs)
            {
                var tneigh = current;
                tneigh.x += neigh.x;
                tneigh.y += neigh.y;

                // already evaluated
                if (closedSet.Contains(tneigh))
                    continue;

                // out of bounds
                if (tneigh.x < 0 || tneigh.y < 0 
                    || tneigh.x >= cells.GetLength(0) || tneigh.y >= cells.GetLength(1))
                    continue;

                bool isOccupied = (cells[tneigh.x, tneigh.y] & GridCellFlag.Occupied) != 0;
                // occupied
                if (!allowDestroy && isOccupied)
                    continue;
                // occupied but not destroyable (maybe this can be written with just biwise stuff?)
                else if (allowDestroy && isOccupied 
                    && (cells[tneigh.x, tneigh.y] & GridCellFlag.Destroyable) == 0)
                    continue;

#if true
                cells[tneigh.x, tneigh.y] |= GridCellFlag.Visited;
#endif

                float tentative_gScore = acells[current.x, current.y].gScore + CellDist(current, tneigh);
                if (!openSet.Contains(tneigh)) // new node
                {
                    openSet.Add(tneigh);
                }
                else if (tentative_gScore >= acells[tneigh.x, tneigh.y].gScore) // worse than existing
                    continue;
                // new best
                cameFrom[tneigh] = current;
                acells[tneigh.x, tneigh.y].gScore = tentative_gScore;
                acells[tneigh.x, tneigh.y].fScore = (tentative_gScore + CellDist2(tneigh, dest)*(isOccupied?100.0f:1.0f));
                // for finding a closest path
                if(bestScore > CellDist(tneigh, dest))
                {
                    bestScore = CellDist(tneigh, dest);
                    best = tneigh;
                }
            }

        }
        reachable = false;
        Debug.Log("Openset size " + maxO);
        Debug.Log("Closedset size " + maxC);
        return ReconstructPath(best);
    }

    // COPY PASTA
    public List<CellCoord> FindPathWithDistanceConstraint(bool allowDestroy, out bool reachable, float distance)
    {
        Assert.IsTrue(prepared);
        prepared = false;

        CellCoord first = openSet.First();
        CellCoord best = openSet.First();
        float bestScore = float.MaxValue;

        while (openSet.Count > 0)
        {
            float curMin = float.MaxValue;
            CellCoord current;
            current.x = 0;
            current.y = 0;
            foreach (var value in openSet)
            {
                if (acells[value.x, value.y].fScore < curMin)
                {
                    curMin = acells[value.x, value.y].fScore;
                    current = value;
                }
            }

            //CellCoord current = openSet.OrderBy(x => acells[x.x, x.y].fScore).FirstOrDefault();
            //Assert.IsTrue(openSet.Min(x => acells[x.x, x.y].fScore) == acells[current.x, current.y].fScore);
            if (CellDist(current, dest) <= distance)
            {
                reachable = true;
                return ReconstructPath(current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neigh in neighs)
            {
                var tneigh = current;
                tneigh.x += neigh.x;
                tneigh.y += neigh.y;

                // already evaluated
                if (closedSet.Contains(tneigh))
                    continue;

                // out of bounds
                if (tneigh.x < 0 || tneigh.y < 0
                    || tneigh.x >= cells.GetLength(0) || tneigh.y >= cells.GetLength(1))
                    continue;

                bool isOccupied = (cells[tneigh.x, tneigh.y] & GridCellFlag.Occupied) == GridCellFlag.Occupied;
                // occupied
                if (!allowDestroy && isOccupied)
                    continue;
                // occupied but not destroyable (maybe this can be written with just biwise stuff?)
                else if (allowDestroy && isOccupied
                    && (cells[tneigh.x, tneigh.y] & GridCellFlag.Destroyable) == 0)
                    continue;

#if true
                cells[tneigh.x, tneigh.y] |= GridCellFlag.Visited;
#endif

                float tentative_gScore = acells[current.x, current.y].gScore + CellDist(current, tneigh);
                if (!openSet.Contains(tneigh)) // new node
                {
                    openSet.Add(tneigh);
                }
                else if (tentative_gScore >= acells[tneigh.x, tneigh.y].gScore) // worse than existing
                    continue;
                // new best
                cameFrom[tneigh] = current;
                acells[tneigh.x, tneigh.y].gScore = tentative_gScore;
                acells[tneigh.x, tneigh.y].fScore = (tentative_gScore + CellDist2(tneigh, dest) * (isOccupied ? 100.0f : 1.0f));
                // for finding a closest path
                if (bestScore > CellDist(tneigh, dest))
                {
                    bestScore = CellDist(tneigh, dest);
                    best = tneigh;
                }
            }

        }
        reachable = false;
        return ReconstructPath(best);
    }

    List<CellCoord> ReconstructPath(CellCoord current)
    {
        List<CellCoord> path = new List<CellCoord>();
        path.Add(current);
        //path.Add(current);
        while(cameFrom.Keys.Contains(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        return path;
    }
}
