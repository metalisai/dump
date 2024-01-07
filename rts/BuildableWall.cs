using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

[ObjectLogic(ActiveObject.ObjectTag.Wall)]
public class WallBehaviour : ManagedMonoBehaviour
{
    public GameObject _wallVisuals;
    public PlaceableObject _po;

    bool wasEdge = false;

    void Start()
    {
        WallsChanged();
    }

    public void WallsChanged()
    {
        if(_wallVisuals == null)
        {
            _po = GetComponent<PlaceableObject>();
        }
        Assert.IsTrue(_po.ownedCells.Count == 1, "Assuming that wall occupies ONE cell (no diagonal walls supported)");
        CellCoord cell = _po.ownedCells[0];
        CellCoord?[] cells = new CellCoord?[4];
        cells[0] = cell.Left(); cells[1] = cell.Right(); cells[2] = cell.Down(); cells[3] = cell.Up();
        var mcells = Game.pathFinding.grid.GetMetaCells();
        int[] wallTypes = new int[4]; // 0 = no wall, 1 = horizontal wall, 2 = vertical wall
        for(int i = 0; i < 4; i++)
        {
            var pobj = cells[i] == null ? null : mcells[cells[i].Value.x, cells[i].Value.y].placeableObject;
            if (pobj == null)
                wallTypes[i] = 0;
            else if ((pobj.flags & PlaceableObject.PlaceableObjectFlags.HorizontalWall) != 0)
                wallTypes[i] = 1;
            else if ((pobj.flags & PlaceableObject.PlaceableObjectFlags.VerticalWall) != 0)
                wallTypes[i] = 2;
            else
                wallTypes[i] = 0;
        }

        //Debug.Log(wallTypes[0] + " " + wallTypes[1] + " " + wallTypes[2] + " " + wallTypes[3]);

        bool edgeWall = wallTypes[0] != wallTypes[1] || wallTypes[2] != wallTypes[3];
        if(edgeWall && edgeWall != wasEdge)
        {
            var child = transform.GetChild(0);
            if (child != null)
                Game.Instance.DestroyDynamicObject(child.gameObject);
            // TODO: cache resource
            var edgeGo = GameObject.Instantiate(Resources.Load<GameObject>(ObjectRegister.GetResourcePath(GameObjectID.EdgeWall)));
            edgeGo.transform.SetParent(transform);
            edgeGo.transform.localPosition = Vector3.zero;
            edgeGo.transform.localRotation = Quaternion.identity;
            edgeGo.SetActive(true);
            _wallVisuals = edgeGo;
            //_po.flags |= PlaceableObject.PlaceableObjectFlags.HorizontalWall;
        }
        wasEdge = edgeWall;
    }
}

public static class WallPlacer
{
    static CellCoord? _start = null;
    static bool _placing = false;
    public delegate void WallPlacerDelegate();
    public static event WallPlacerDelegate PlacingEnd;

    public static void Reset()
    {
        _start = null;
    }

    public static void StartPlacing()
    {
        Reset();
        _placing = true;
    }

    public static void Update()
    {
        if (!_placing)
            return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f))
        {
            //BuildableWall.RenderGhost(hit.point);
            CellCoord end = Game.pathFinding.WorldCoordToGrid(hit.point);
            if (Input.GetMouseButtonDown(0) && _start == null)
            {
                _start = Game.pathFinding.WorldCoordToGrid(hit.point);
                Debug.LogFormat("Build wall from {0} {1}", _start.Value.x, _start.Value.y);
            }
            else if (Input.GetMouseButtonDown(0) && _start != null)
            {

                Debug.LogFormat("Build wall from {0} {1} to {0} {1}", _start.Value.x, _start.Value.y, end.x, end.y);
                BuildableWall.BuildWall(_start.Value, end);
                _start = end;
            }
            else if (_start != null)
            {
                BuildableWall.RenderGhost(_start.Value, end);
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            _start = null;
            _placing = false;
            if (PlacingEnd != null)
                PlacingEnd.Invoke();
        }
    }
}

public class BuildableWall
{
    class WallGhost
    {
        public Mesh mesh;
        public GridMask mask;
    }

    const GameObjectID straightPart = GameObjectID.StraightWall;
    const GameObjectID diagonalPart = GameObjectID.DiagonalWall;
    const GameObjectID edgePart = GameObjectID.EdgeWall;

    static WallGhost straightWallGhost;
    static WallGhost diagonalWallGhost;
    static WallGhost edgeWallGhost;
    static Material ghostMat;

    static WallGhost CreateGhost(GameObjectID from)
    {
        Debug.Log("Created wall ghost mesh");
        var copy = GameObject.Instantiate(Resources.Load<GameObject>(ObjectRegister.GetResourcePath(from))).GetComponent<PlaceableObject>();
        copy.transform.position = Vector3.zero;
        copy.transform.rotation = Quaternion.identity;
        // destroy all monobehaviours
        foreach (var component in copy.gameObject.GetComponents<MonoBehaviour>())
            MonoBehaviour.Destroy(component);
        // change all renderers
        var meshFilters = copy.gameObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] cis = new CombineInstance[meshFilters.Length];
        for(int i = 0; i < meshFilters.Length;i ++)
        {
            cis[i].mesh = meshFilters[i].sharedMesh;
            cis[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(cis);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        WallGhost ret = new WallGhost();
        ret.mesh = mesh;
        ret.mask = new GridMask(copy.MaskString);
        return ret;
    }

    static void CalculateParts(CellCoord start, CellCoord finish, out int horDirection, out int vertDirection, out int diaParts, out int horParts, out int vertParts)
    {
        //float cellSize = Game.pathFinding.grid.gridScale;
        horDirection = Mathf.RoundToInt(Mathf.Sign(finish.x - start.x));
        vertDirection = Mathf.RoundToInt(Mathf.Sign(finish.y - start.y));
        int horMovement = Mathf.Abs(start.x - finish.x);
        int vertMovement = Mathf.Abs(start.y - finish.y);
        //diaParts = Mathf.Min(horMovement, vertMovement);
        diaParts = 0;
        horParts = Mathf.Max(horMovement/* - diaParts*/, 0);
        vertParts = Mathf.Max(vertMovement/* - diaParts*/, 0);
    }

    public static void RenderGhost(CellCoord start, CellCoord finish)
    {
        if(ghostMat == null)
        {
            ghostMat = Resources.Load<Material>("Materials/PlacementGhost");
        }
        if (straightWallGhost == null)
            straightWallGhost = CreateGhost(straightPart);

        if (diagonalWallGhost == null)
            diagonalWallGhost = CreateGhost(diagonalPart);

        if (edgeWallGhost == null)
            edgeWallGhost = CreateGhost(edgePart);

        int hdir, vdir, dprts, hprts, vprts;
        CalculateParts(start, finish, out hdir, out vdir, out dprts, out hprts, out vprts);

        float cellSize = Game.pathFinding.grid.gridScale;

        Vector3 wallCursor = Game.pathFinding.GridCoordToWorld(start);

        for (int i = 0; i < dprts; i++)
        {
            if (hdir == vdir)
                diagonalWallGhost.mask.SetRotation(1);
            else
                diagonalWallGhost.mask.SetRotation(0);
            //Vector3 hintPos = wallCursor + new Vector3(0.5f * cellSize * hdir, 0.0f, 0.5f * cellSize * vdir);
            Vector3 position = diagonalWallGhost.mask.GetSnappedPosition(wallCursor);
            Graphics.DrawMesh(diagonalWallGhost.mesh, position, (hdir == vdir ? Quaternion.AngleAxis(90.0f, Vector3.up) : Quaternion.identity), ghostMat, 0);
            wallCursor += new Vector3(hdir * cellSize, 0.0f, vdir * cellSize);
        }
        for (int i = 0; i < hprts; i++)
        {
            bool nexttovertical = (i == hprts/* - 1*/ && vprts != 0);
            Mesh mesh = nexttovertical ? edgeWallGhost.mesh : straightWallGhost.mesh;
            straightWallGhost.mask.SetRotation(0);
            Vector3 position = straightWallGhost.mask.GetSnappedPosition(wallCursor);
            Graphics.DrawMesh(mesh, position, Quaternion.identity, ghostMat, 0);
            wallCursor += new Vector3(cellSize * hdir, 0.0f, 0.0f);
        }
        for (int i = 0; i < vprts; i++)
        {
            bool nexttovertical = (i == 0 && hprts != 0);
            Mesh mesh = nexttovertical ? edgeWallGhost.mesh : straightWallGhost.mesh;
            straightWallGhost.mask.SetRotation(1);
            Vector3 position = straightWallGhost.mask.GetSnappedPosition(wallCursor);
            Graphics.DrawMesh(mesh, position, Quaternion.AngleAxis(90.0f, Vector3.up), ghostMat, 0);
            wallCursor += new Vector3(0.0f, 0.0f, cellSize * vdir);
        }
    }

    public static void BuildWall(CellCoord start, CellCoord finish)
    {
        float cellSize = Game.pathFinding.grid.gridScale;

        int hdir, vdir, dprts, hprts, vprts;
        CalculateParts(start, finish, out hdir, out vdir, out dprts, out hprts, out vprts);

        Vector3 wallCursor = Game.pathFinding.GridCoordToWorld(start);

        var diaGo = Resources.Load<GameObject>(ObjectRegister.GetResourcePath(diagonalPart));
        var straGo = Resources.Load<GameObject>(ObjectRegister.GetResourcePath(straightPart));
        for (int i = 0; i < dprts; i++)
        {
            // TODO: register dynamic object
            var go = GameObject.Instantiate(diaGo);
            var po = go.GetComponent<PlaceableObject>();
            po.LoadMask();
            if (hdir == vdir)
                po.RotateLeft();
            //Vector3 tpos = po.gridMask.GetSnappedPosition(wallCursor);
            po.TryPlace(wallCursor);
            go.SetActive(true);
            wallCursor += new Vector3(hdir*cellSize, 0.0f, vdir*cellSize);
        }
        for (int i = 0; i < hprts; i++)
        {
            // TODO: register dynamic object
            var go = GameObject.Instantiate(straGo);
            var po = go.GetComponent<PlaceableObject>();
            po.flags |= PlaceableObject.PlaceableObjectFlags.HorizontalWall;
            po.LoadMask();
            po.TryPlace(wallCursor);
            go.SetActive(true);
            wallCursor += new Vector3(cellSize*hdir, 0.0f, 0.0f);
        }
        for (int i = 0; i < vprts; i++)
        {
            // TODO: register dynamic object
            var go = GameObject.Instantiate(straGo);
            var po = go.GetComponent<PlaceableObject>();
            po.flags |= PlaceableObject.PlaceableObjectFlags.VerticalWall;
            po.LoadMask();
            po.RotateLeft();
            po.TryPlace(wallCursor);
            go.SetActive(true);
            wallCursor += new Vector3(0.0f, 0.0f, cellSize*vdir);
        }

        // TODO: SLOW
        foreach(var wall in GameObject.FindObjectsOfType<WallBehaviour>())
        {
            wall.WallsChanged();
        }
    }
}