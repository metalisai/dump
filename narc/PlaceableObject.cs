// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;

public class PlaceableObject : MonoBehaviour, IItemDoubleclickHandler, IItemSelectHandler {

	public Housing Infrastructure { get; set; }
	//public ModuleType Type {get { return _type; } }

    public PlaceableObjectType ObjectType;

    public PlacementGhost GhostObject;

	public GridCellType GridType;

    public bool HasRandomRotationOffset = false;

    public bool InGrid = true;

    public bool DisplaySelectionWindow = true;

    public Sprite Icon;

    public string DisplayName;

    private float _randomRotation = 0f;

    private bool _quitting = false;

    public bool inNavMesh = false;
    public int navMeshMask = 0;

	//protected ModuleType _type = ModuleType.None;

	public IList<GridCell> Cells {
		get { return _gridShape==null? null :_gridShape.Cells; }
	}

    public IList<GridCell> AffectedCells
    {
        get { return _gridShape.AffectedCells; }
    }

    public string ShapeString="1";

	public GridShape GridShape { 
		get{ 
			if(_gridShape == null)
				LoadShape();

			return _gridShape; 
		} 
	}

	private GridShape _gridShape;

    private SelectionWindow _selWin;

    void Start()
    {
        _selWin = FindObjectOfType<SelectionWindow>();
    }

    public void Init()
    {
        Component[] components = GetComponents(typeof(GridEffect));
        GridShape.Effects = new GridEffect[components.Length];
        for (int i = 0; i < components.Length; i++)
        {
            GridShape.Effects[i] = (GridEffect)components[i];
            Debug.Log("Registered effect");
            if (!InGrid)
            {
                Debug.LogError("Effects currently only supported for (static) inGrid objects!");
            }
        }
    }

    void Awake()
    {
        if (HasRandomRotationOffset)
        {
            _randomRotation = Random.Range(0f, 360f);
        }
    }

	public GridShapeRotation Rotation {
		get { return _gridShape.Rotation; }
	}

	void LoadShape()
	{
		_gridShape = new GridShape (ShapeString);
	}

	public bool Place(Grid grid, Vector3 position)
	{
		if (grid.Fits(GridShape,position, GridType)) 
        {
			Vector3 location = grid.PlaceShape (GridShape, position, GridType);

			// TODO: fix this
			gameObject.transform.position = location;
			Infrastructure = grid.Infrastructure;
			return true;
		}
		return false;
	}

	public bool Fits(Grid grid, Vector3 position)
	{
		return grid.Fits (this.GridShape, position, GridType);
	}

	public Vector3 GetGridPosition(Grid grid, Vector3 position)
	{
		return grid.GetPositionForShape (GridShape, position, GridType);
	}

	public void Rotate()
	{
		GridShape.RotateLeft ();
        SetRotation(GridShape.Rotation);
	}

	public void SetRotation(GridShapeRotation rotation)
	{
        if (rotation != GridShape.Rotation)
        {
            gameObject.transform.Rotate(new Vector3(0f, -90f * (int)rotation + (HasRandomRotationOffset ? _randomRotation : 0f), 0f));
            GridShape.SetRotation(rotation);
        }
	}

	public void FreeGridSpace()
	{
        if (Cells == null)
            return;

		foreach (GridCell cell in Cells) {
			cell.Filled = false;
		}
	    Cells.Clear();

        foreach (GridCell cell in AffectedCells)
        {
            cell.Effects.RemoveAll(x => x.ApplierPlaceableObject == this);
        }
        AffectedCells.Clear();
    }

    public bool OnSelected()
    {
        if (DisplaySelectionWindow)
        {
            _selWin.SetSelected(this);
        }
        return DisplaySelectionWindow;
    }

    public void OnDeSelecded()
    {
        if (DisplaySelectionWindow && _selWin.currentSelection == gameObject)
        {
            _selWin.Hide();
        }
    }

    public void Delete()
    {
        if (this == null)
            Debug.LogError("This was null");
        Destroy(this.gameObject);
    }

    void OnApplicationQuit()
    {
        _quitting = true;
    }

    void OnDestroy()
	{
        if (_quitting)
            return;

		FreeGridSpace ();
        if(Infrastructure != null)
            Infrastructure.PlaceableObjectDestroyed(this);

        // NOTE: null check because if game is closed the _selWin might be destroyed by the point this is called
        if (_selWin != null && _selWin.currentSelection == gameObject)
        {
            _selWin.CancelSelection();
            _selWin.Hide();
        }
    }

    public PlaceableObjectSave GetPobjState()
    {
        PlaceableObjectSave state = new PlaceableObjectSave();
        Vector3 pos = transform.position;
        state.PosX = pos.x;
        state.PosY = pos.y;
        state.PosZ = pos.z;
        state.Rotation = Rotation;
        state.Type = ObjectType;

        ISaveable sav = GetComponentInChildren<ISaveable>();
        if (sav != null)
        {
            state.State = sav.GetState();
        }
        return state;
    }

    public void OnDoubleclick()
    {
        Debug.Log("Doubleclick");
        var placer = ItemPlacer.Current;
        placer.StartMoving(this, PlacerEvent);
    }

    void PlacerEvent(PlacerEventId eventId)
    {
        if (eventId == PlacerEventId.OnEndPlace)
        {
        }
    }
}

public enum ModuleType
{
	None,
	WeedDryer,
	Storage,
	WeedPlant
}

[System.Serializable]
public struct GridCoordinates
{
	public int x;
	public bool xHalf;
	public int y;
	public bool yHalf;
}

[System.Serializable]
public class PlaceableObjectSave
{
    public float PosX;
    public float PosY;
    public float PosZ;
    public PlaceableObjectType Type;
    //public float RandomRotationOffset;
    public GridShapeRotation Rotation;
    public System.Object State;
}