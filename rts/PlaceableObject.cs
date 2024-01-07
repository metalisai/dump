using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

[System.Serializable]
public enum PlaceableObjectCategory
{
    Defense = 0,
    Electricity = 1,
    Utility = 2,
    Workers = 3
}

[ExecuteInEditMode]
public class PlaceableObject : ManagedMonoBehaviour{

    [System.Flags]
    public enum PlaceableObjectFlags : byte
    {
        None,
        HorizontalWall,
        VerticalWall
    }

    public PlaceableObjectFlags flags = PlaceableObjectFlags.None;
    public string MaskString;
    public CellCoord? Location = null;
    public GridMask gridMask;
    [SerializeField, HideInInspector]
    public WaypointGraph waypointGraph;
    public List<CellCoord> ownedCells = new List<CellCoord>();

    public float buildProgress = 0.0f;
    bool building = false;
    public bool autoBuild = true;

    public int Rotation { get { return gridMask.Rotation; } }

    public void LoadMask()
    {
        gridMask = new GridMask(MaskString);
    }

    MeshRenderer[] buildingRenderers;
    Shader[] originalShaders;

    public void StartBuilding()
    {
        var progui = gameObject.AddComponent<ProgressUI>();
        progui.SetName("Building " + name.Replace("(Clone)", ""));
        building = true;
        buildProgress = 0.0f;

        buildingRenderers = GetComponentsInChildren<MeshRenderer>();
        originalShaders = new Shader[buildingRenderers.Length];
        for(int i = 0; i < buildingRenderers.Length; i++)
        {
            originalShaders[i] = buildingRenderers[i].material.shader;
            buildingRenderers[i].material.shader = Game.Instance.BuildingShader;
        }
    }

    public void Built()
    {
        Destroy(GetComponent<ProgressUI>());
        building = false;
        for(int i = 0; i < buildingRenderers.Length; i ++)
        {
            buildingRenderers[i].material.shader = originalShaders[i];
        }
        originalShaders = null;
        buildingRenderers = null;
        var ao = GetComponent<ActiveObject>();
        if (ao != null)
            ao.Activate();
    }

    public override void ManagedUpdate()
    {
        if(!Application.isPlaying)
        {
            Profiler.BeginSample("PlaceableObject.ManagedUpdate() (Editor)");
            // snap to grid
            CellCoord ccoord = PathFinding.WorldCoordToGrid(transform.position, Vector3.zero);
            var terr = FindObjectOfType<CustomTerrain>();
            Vector3 coord = new Vector3(ccoord.x * Game.GRIDCELL_SIZE + Game.GRIDCELL_HALFSIZE, 0.0f, ccoord.y * Game.GRIDCELL_SIZE + Game.GRIDCELL_HALFSIZE);
            coord.y = terr.SampleHeight(coord);
            transform.position = coord;
            Profiler.EndSample();
            return;
        }
        Profiler.BeginSample("PlaceableObject.ManagedUpdate()");
        if (building)
        {
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            // find minimum and maximum Y
            foreach (var rnd in buildingRenderers)
            {
                var bounds = rnd.bounds;
                minY = Mathf.Min(bounds.center.y - bounds.extents.y, minY);
                maxY = Mathf.Max(bounds.center.y + bounds.extents.y, maxY);
            }
            // apply clipping Y
            foreach (var rnd in buildingRenderers)
            {
                float clipY = minY + Mathf.Clamp01(buildProgress) * (maxY - minY);
                rnd.material.SetFloat("_clipY", clipY);
            }

            // TODO: remove autoBuild
            if (autoBuild)
            {
                buildProgress += Time.deltaTime / 5.0f;
                if (buildProgress >= 1.0f)
                    Built();
            }
        }
        Profiler.EndSample();
    }

    void Awake()
    {
        LoadMask();
        if (waypointGraph != null)
            waypointGraph.transform = transform;
    }

    void OnDestroy()
    {
        // free all the owned cells
        foreach (var cell in ownedCells)
        {
            if(Game.pathFinding != null && Game.pathFinding.grid != null)
                Game.pathFinding.grid.FreeCell(cell, this);
        }
    }

    public void RotateLeft()
    {
        // unity assertions doesn't allow isnull of nullable, how awesome
        Assert.IsTrue(Location == null, "Rotating a placed object is NOT supported!");
        gridMask.RotateLeft();
        transform.rotation = Quaternion.AngleAxis(gridMask.Rotation * 90.0f, Vector3.up);
    }

    public void RotateRight()
    {
        // unity assertions doesn't allow isnull of nullable, how awesome
        Assert.IsTrue(Location == null, "Rotating a placed object is NOT supported!");
        gridMask.RotateRight();
        transform.rotation = Quaternion.AngleAxis(gridMask.Rotation * 90.0f, Vector3.up);
    }

    // 0 = 0deg, 1 = 90deg...
    public void SetRotation(int rotation)
    {
        gridMask.SetRotation(rotation);
        transform.rotation = Quaternion.AngleAxis(gridMask.Rotation * 90.0f, Vector3.up);
    }

    public Vector3 GetSnappedPosition(Vector3 inPos)
    {
        return gridMask.GetSnappedPosition(inPos);
    }

    public bool CanPlace(Vector3 location)
    {
        var cellCoord = Game.pathFinding.grid.WorldPosToCellCoord(location, gridMask);
        bool occupied = !Game.pathFinding.grid.IsOccupied(cellCoord, gridMask);
        return occupied;
    }

    public void Activate()
    {
        var ao = GetComponent<ActiveObject>();
        if (ao != null)
            ao.Activate();
    }

    void Place(CellCoord cellCoord)
    {
        ownedCells.Clear();
        transform.position = Game.pathFinding.grid.MarkOccupied(cellCoord, gridMask, true, this, ownedCells);
        Location = cellCoord;
        //Game.pathFinding.AddMeshes(GetComponentsInChildren<MeshFilter>());
    }

	public bool TryPlace(Vector3 location, bool build = true)
    {
        var cellCoord = Game.pathFinding.grid.WorldPosToCellCoord(location, gridMask);
        if(!Game.pathFinding.grid.IsOccupied(cellCoord, gridMask))
        {
            Place(cellCoord);
            if(build)
                StartBuilding();
            else
            {
                var ao = GetComponent<ActiveObject>();
                if (ao != null)
                    ao.Activate();
            }
            return true;
        }
        return false;
    }

    public bool TryPlace(CellCoord location)
    {
        if (!Game.pathFinding.grid.IsOccupied(location, gridMask))
        {
            Place(location);
            return true;
        }
        return false;
    }
}
