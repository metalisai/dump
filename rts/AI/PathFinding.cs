using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using System.Collections.Concurrent;
using UnityEngine.Assertions;
using UnityEngine.AI;

struct PathQuery
{
    public Vector3 from;
    public Vector3 to;
    public float distance;
    public PathQueryResult resultTo;
    public bool allowDestroy;
    public bool exact;
}

public class PathQueryResult
{
    List<Vector3> wp;
    public List<Vector3> waypoints
    {
        get {
            if(!done)
            {
                Debug.LogError("Check the 'done' property before accessing!");
            }
            return wp;
        }
        set
        {
            wp = value;
        }
    }
    public bool reachable;
    volatile public bool done;
    public float pathTimeMs;
    public float simplifyTimeMs;
}

public class PathFinding
{
    public readonly PathFindingGrid grid;
    readonly float cellSize;

    readonly Vector3 terrainOrigin;

    Thread workerThread;
    volatile bool running = true;
    readonly int gridSize;

    UNavmeshPathfinding _upf;

    public PathFinding(CustomTerrain terrain)
    {
        float gridSizef = terrain.Size / Game.GRIDCELL_SIZE;
        float frac = Mathf.Repeat(gridSizef, 1.0f);
        if (frac > Mathf.Epsilon)
            Debug.LogWarning("Map size should be a power of 2! (or at very least divisible by 4)");
        gridSize = Mathf.RoundToInt(gridSizef);
        cellSize = Game.GRIDCELL_SIZE;
        terrainOrigin = terrain.transform.position;

        grid = new PathFindingGrid(terrainOrigin, cellSize, gridSize, gridSize);
        var mcells = grid.GetMetaCells();
        for (int i = 0; i < mcells.GetLength(0); i++)
            for (int j = 0; j < mcells.GetLength(1); j++)
                mcells[i, j].center.y = terrain.SampleHeight(mcells[i, j].center);


        //ThreadStart ts = new ThreadStart(ThreadProc);
        //workerThread = new Thread(ts);
        //workerThread.Start();

        var sources = new List<NavMeshBuildSource>();
        NavMeshBuildSource nbs = new NavMeshBuildSource();
        nbs.shape = NavMeshBuildSourceShape.Mesh;
        Assert.IsNotNull(terrain.TerrainMesh, "Terrain mesh was null when trying to create navmesh");
        nbs.sourceObject = terrain.TerrainMesh;
        nbs.transform = terrain.transform.localToWorldMatrix;
        nbs.area = 0;
        //sources.Add(nbs);

        _upf = new UNavmeshPathfinding();
        _upf.Initialize(sources);
    }

    public PathFinding()
    {
        if (GameObject.FindObjectOfType<Game>() != null)
            Debug.LogError("This constructor is meant only for testing!");
        cellSize = 1;
        terrainOrigin = new Vector3(0.0f, 0.0f, 0.0f);

        grid = new PathFindingGrid(terrainOrigin, cellSize, gridSize, gridSize);
        var mcells = grid.GetMetaCells();
        for (int i = 0; i < mcells.GetLength(0); i++)
            for (int j = 0; j < mcells.GetLength(1); j++)
                mcells[i, j].center.y = 0.0f;

        ThreadStart ts = new ThreadStart(ThreadProc);
        workerThread = new Thread(ts);
        workerThread.Start();
    }

    LockFreeQueue<PathQuery> pathQueries = new LockFreeQueue<PathQuery>();
    LockFreeQueue<PathQueryResult> pathQueryResults = new LockFreeQueue<PathQueryResult>();

    bool GridLinecast(Vector3 start, Vector3 end, bool allowDestroy)
    {
        const int segmentsPerUnit = 10;
        float dist = (start - end).magnitude;
        int segments = Mathf.CeilToInt(dist * segmentsPerUnit);
        float segmentLength = (dist < Mathf.Epsilon) ? 1.0f : dist / (float)segments;
        Vector3 segmentV = (end - start).normalized * segmentLength; // can optimize
        var gridCells = grid.GetCells();
        //Debug.LogFormat("dist {0} segments {1}",dist, segments);
        //dlines.Add(new DLine() { start = start, end = end, color = Color.red });
        for (int i = 0; i <= segments; i++)
        {
            //dlines.Add(new DLine() { start = start + (segmentV * i), end = (start + (segmentV * i))+Vector3.up, color = Color.red });
            var cell = WorldCoordToGrid(start + (segmentV * i));

            bool filled;
            var flags = gridCells[cell.x, cell.y];
            if (!allowDestroy)
                filled = (flags & GridCellFlag.Occupied) != 0;
            else
                filled = (flags & GridCellFlag.Occupied) != 0 && (flags & GridCellFlag.Destroyable) == 0;
            if (filled)
                return true;
        }
        return false;
    }

    List<Vector3> SimplifyPath(List<Vector3> path, bool allowDestroy)
    {
        List<Vector3> simplifiedPath = new List<Vector3>();
        int count = path.Count;
        for(int i = 0; i < count; i++)
        {
            for(int j = i+1; j < count; j++)
            {
                    if (GridLinecast(path[i], path[j], allowDestroy) || j == count-1) // something in between or end
                    {
                        simplifiedPath.Add(path[i]);
                        if (j == count - 1)
                            simplifiedPath.Add(path[j]);
                        Assert.IsTrue(i <= j-1);
                        i = j - 1;
                        break;
                    }
            }
        }
        return simplifiedPath;
    }

    void ThreadProc()
    {
        try
        {
            AStar astar;
            // TODO: is it safe to use the sharedgrid?
            astar = new AStar(grid.GetCells(), grid.GetMetaCells());

            while (running)
            {
                PathQuery curQuery;
                while (pathQueries.Dequeue(out curQuery))
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    CellCoord startC = WorldCoordToGrid(curQuery.from);
                    CellCoord destC = WorldCoordToGrid(curQuery.to);
                    astar.Prepare(startC, destC);
                    List<CellCoord> opath;
                    if (!curQuery.exact)
                        opath = astar.FindPathWithDistanceConstraint(curQuery.allowDestroy, out curQuery.resultTo.reachable, curQuery.distance / cellSize);
                    else
                        opath = astar.FindPath(curQuery.allowDestroy, out curQuery.resultTo.reachable);
                    var unsimplifiedpath = opath.Select(x => GridCoordToWorld(x)).ToList();
                    curQuery.resultTo.pathTimeMs = sw.ElapsedMilliseconds;
                    var wps = SimplifyPath(unsimplifiedpath, curQuery.allowDestroy);
                    curQuery.resultTo.waypoints = wps;
                    pathQueryResults.Enqueue(curQuery.resultTo);
                    curQuery.resultTo.simplifyTimeMs = sw.ElapsedMilliseconds - curQuery.resultTo.pathTimeMs;
                    sw.Stop();
                    if (curQuery.resultTo.pathTimeMs > 200)
                    {
                        Debug.LogWarningFormat("Path query took {0} milliseconds (astar {1} simplify {2}) waypoints: {3}", 
                            curQuery.resultTo.pathTimeMs + curQuery.resultTo.simplifyTimeMs, 
                            curQuery.resultTo.pathTimeMs, 
                            curQuery.resultTo.simplifyTimeMs,
                            wps.Count
                        );
                    }
                }
                Thread.Sleep(10);
            }
        }
        catch(System.Exception e)
        {
            Debug.LogError("Pathfinding thread crashed " + e.StackTrace);
        }
    }

    public void AddMeshes(MeshFilter[] newMeshes)
    {
        Debug.Log("Added " + newMeshes.Length);
        var obs = new List<NavMeshBuildSource>();
        foreach(var mesh in newMeshes)
        {
            var nmvs = new NavMeshBuildSource();
            nmvs.area = 0;
            nmvs.shape = NavMeshBuildSourceShape.Mesh;
            nmvs.sourceObject = mesh.sharedMesh;
            nmvs.transform = mesh.transform.localToWorldMatrix;
            obs.Add(nmvs);
        }
        _upf.AddSources(obs);
    }

    public void CheckDoneQueries()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            _upf.Update();
        PathQueryResult result;
        while(pathQueryResults.Dequeue(out result)) // boolean synchronized just in case
        {
            result.done = true;
        }
    }

    public Vector3 GridCoordToWorld(CellCoord coord)
    {
        var mcells = grid.GetMetaCells();
        var pos = mcells[coord.x, coord.y].center;
        return pos;
    }

    public CellCoord WorldCoordToGrid(Vector3 wpos)
    {
        float xoffset = wpos.x - terrainOrigin.x;
        float zoffset = wpos.z - terrainOrigin.z;
        CellCoord ret = new CellCoord(Mathf.FloorToInt(xoffset / Game.GRIDCELL_SIZE), Mathf.FloorToInt(zoffset / Game.GRIDCELL_SIZE));
        return ret;
    }

    public static CellCoord WorldCoordToGrid(Vector3 wpos, Vector3 terrainOrigin)
    {
        float xoffset = wpos.x - terrainOrigin.x;
        float zoffset = wpos.z - terrainOrigin.z;
        CellCoord ret = new CellCoord(Mathf.FloorToInt(xoffset / Game.GRIDCELL_SIZE), Mathf.FloorToInt(zoffset / Game.GRIDCELL_SIZE));
        return ret;
    }

    public PathQueryResult findPathWithDistanceAsync(Vector3 start, Vector3 dest, float distance, bool allowDestroy)
    {
        PathQuery query;
        query.from = start;
        query.to = dest;
        query.distance = distance;
        query.resultTo = new PathQueryResult();
        query.allowDestroy = allowDestroy;
        query.exact = false;
        pathQueries.Enqueue(query);
        return query.resultTo;
    }

    public PathQueryResult findPathExactAsync(Vector3 start, Vector3 dest, bool allowDestroy)
    {
        PathQuery query;
        query.from = start;
        query.to = dest;
        query.distance = 0.0f;
        query.resultTo = new PathQueryResult();
        query.allowDestroy = allowDestroy;
        query.exact = true;
        pathQueries.Enqueue(query);
        return query.resultTo;
    }

    public void Stop()
    {
        running = false;
        workerThread.Abort();
    }

    public void OnDrawGizmos()
    {
        if (grid == null)
            return;

        var cells = grid.GetCells();
        var mcells = grid.GetMetaCells();
        for(int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(0); j++)
            {
                bool occupied = (cells[i, j] & GridCellFlag.Occupied) == GridCellFlag.Occupied;
                Vector3 start = mcells[i, j].center;
                if (occupied && (cells[i, j] & GridCellFlag.Destroyable) != 0)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(start, start + Vector3.up);
                }
                else if (occupied)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(start, start + Vector3.up);
                }
                /*else
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(start, start + Vector3.up);
                }*/
                

                if (mcells[i,j].placeableObject != null)
                {
                    if ((cells[i, j] & GridCellFlag.ConnectedToWaypointGraph) != 0)
                    {
                        foreach (var edge in mcells[i, j].subgraphEdges)
                        {
                            Gizmos.DrawLine(mcells[i, j].center,
                                edge.edgeTo.transform.position
                                + edge.edgeTo.waypoints[edge.edgeIndex]);
                        }
                    }
                    foreach(var wp in mcells[i, j].placeableObject.waypointGraph.waypoints)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(mcells[i, j].placeableObject.transform.position + wp, 0.1f);
                    }
                }
            }
        }

        /*lock (locks)
        {
            foreach (var line in dlines)
            {
                Debug.DrawLine(line.start, line.end, line.color);
            }
        }*/

        /*Gizmos.color = Color.red;
        foreach(var node in path)
        {
            Gizmos.DrawLine(cells[node.x, node.y].Center, cells[node.x, node.y].Center + Vector3.up);
        }*/
    }
}
