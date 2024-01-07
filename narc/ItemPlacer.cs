// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

// this class is messy, I know
// how to use this from outside? call StartPlacing and subscribe for events (OnPlacerEvent)
// if you want to place something, call Place() on the OnTryingPlace event
public class ItemPlacer : MonoBehaviour {

	public delegate void  PlacerAction (PlacerEventId eventId);
    public static ItemPlacer Current;

    private event PlacerAction OnPlacerEvent;
	private PlacerMode _mode;
	private bool _placingItem = false; // are we placing an object?	
	private GameObject _ghostObject; // the ghost item
	private PlacementGhost _placementGhost;
    private GridShape _curShape;
    private GridCellType _curGridType;
	private PlaceableObjectType _curPlObjectType;
    private PlaceableObject _curPlObject;
	private Grid _curGrid;
	private bool _down = false;
    private Arrow _arrow;

    void Start()
    {
        _arrow = Arrow.CreateArrow(Vector3.zero, Vector3.left);
        if (Current != null)
            Debug.LogError("Multiple item placers make no sense! (delete other instances)");
        Current = this;
    }

	public void StartPlacing(PlaceableObjectType plType, PlacerAction callback)
	{
        Debug.Log("Started placing");
        if (_placingItem)
        {
            CancelPlacing();
        }
        // yup...
		_mode = PlacerMode.CreateNew;
		_placingItem = true;
		_ghostObject = ObjectRegister.GetGhostObject(plType).gameObject;
        _ghostObject.SetActive(true);
		_placementGhost = (PlacementGhost)_ghostObject.GetComponent(typeof(PlacementGhost));
        _curShape = ObjectRegister.GetPlacableObject(plType).GridShape.Clone();
        _curGridType = ObjectRegister.GetPlacableObject(plType).GridType;
        _curPlObject = ObjectRegister.GetPlacableObject(plType);
        _ghostObject.transform.rotation = Quaternion.identity;
		_curPlObjectType = plType;
        OnPlacerEvent += callback;
    }

	public void StartMoving(PlaceableObject plObject, PlacerAction callback)
	{
        if (_placingItem)
        {
            CancelPlacing();
        }

        Debug.Log("smove");

        _mode = PlacerMode.Move;

        _arrow.SetCoordinates(plObject.transform.position, Vector3.left);

		_placingItem = true;
		_ghostObject = plObject.GhostObject.gameObject;
		_placementGhost = (PlacementGhost)_ghostObject.GetComponent (typeof(PlacementGhost));
		_curPlObject = plObject;
        _curGridType = plObject.GridType;
        _curShape = plObject.GridShape.Clone();

        OnPlacerEvent += callback;
    }

	public void Place()
	{
        if (_curGrid.Infrastructure != null)
        {
            _curGrid.Infrastructure.AddObjectToPosition(_curPlObjectType, _ghostObject.transform.position, _curShape.Rotation);
        }
        else
        {
            PlaceableObject pObject = ObjectRegister.CreatePlacableObject(_curPlObjectType);

            if (pObject.InGrid)
            {
                pObject.SetRotation(_curShape.Rotation);

                if (pObject.Fits(_curGrid, _ghostObject.transform.position))
                {
                    pObject.Place(_curGrid, _ghostObject.transform.position);
                }
                else
                {
                    Destroy(pObject);
                    pObject = null;
                    // This should never occur, if it does then you need to check if object fits before you call this function!
                    // This check is here only to catch bugs!
                    Debug.LogError("Tried to put object where it would not fit!");
                }
            }
            else
            {
                pObject.gameObject.transform.position = _ghostObject.transform.position;
                pObject.Infrastructure = _curGrid.Infrastructure;
            }

            Housing curMin = null;
            float curMinVal = float.MaxValue;
            foreach (var ap in FindObjectOfType<Player>().Infrastructures)
            {
                float dist = Vector3.Distance(_ghostObject.transform.position, ap.transform.position);
                if (dist < curMinVal)
                {
                    curMin = ap;
                    curMinVal = dist;
                }
            }
            //_apartment = curMin;
            // TODO: this doesn't belong here
            // TODO: bug when multiple infrastructures and this doesn't match with the loaded one
            curMin.ExtPObjects.Add(pObject);
            pObject.Infrastructure = curMin;
            pObject.gameObject.SetActive(true);
        }
	}

    // TODO: this is painful to read
    // BUG: if itemselector runs before this script in the frame, then deoubleclick results in instant place 
    // (currently solved by using script executing order but this isn't a good one)
	void Update () 
	{
		if (_placingItem && _placementGhost != null) {

			if(Input.GetKeyDown(KeyCode.Space))
			{
                _ghostObject.transform.Rotate(Vector3.up, -90f);
                _curShape.RotateLeft();
			}

			GridRaycastInfo grcInfo;
			bool hit = GridRayCaster.RayCastGrids(out grcInfo, _curGridType);

			_placementGhost.gameObject.SetActive(true);

            bool fits = _curPlObject.InGrid ? hit && grcInfo.GridCell.Grid.Fits(_curShape, grcInfo.Hitpoint, _curGridType):!_placementGhost.Colliding;

            if (_curPlObject.inNavMesh)
            {
                NavMeshHit nhit;
                fits = fits && NavMesh.SamplePosition(grcInfo.Hitpoint, out nhit, 0.2f, _curPlObject.navMeshMask);
            }
            
            if (fits)
			{
                if (_curPlObject.InGrid)
                {
                    _ghostObject.transform.position = grcInfo.GridCell.Grid.GetPositionForShape(_curShape, grcInfo.Hitpoint, _curGridType);
                    _placementGhost.MakeGreen();

                    if (_mode == PlacerMode.Move)
                    {
                        _arrow.gameObject.SetActive(true);
                        _arrow.SetDestination(_ghostObject.transform.position);
                    }

                    if (Input.GetMouseButtonDown(0))
                        _down = true;
                    else if (Input.GetMouseButtonUp(0) && _down) // place the tree when player presses left click
                    {
                        PlaceItem(grcInfo.GridCell.Grid, grcInfo.Hitpoint);
                        _down = false;

                    }
                }
                else
                {
                    _ghostObject.transform.position = grcInfo.Hitpoint;
                    _placementGhost.MakeGreen();

                    if (Input.GetMouseButtonDown(0))
                        _down = true;
                    else if (Input.GetMouseButtonUp(0) && _down) // place the tree when player presses left click
                    {
                        if (!_curPlObject.inNavMesh)
                        {
                            PlaceItem(grcInfo.GridCell.Grid, grcInfo.Hitpoint);
                            _down = false;
                        }
                        else
                        {
                            PlaceItem(grcInfo.GridCell.Grid, grcInfo.Hitpoint);
                            _down = false;
                        }
                    }
                }
			}
			else if (hit) // found a cell but it was filled
			{
				_placementGhost.MakeRed();
				_placementGhost.transform.position = grcInfo.Hitpoint;
                _arrow.gameObject.SetActive(false);
            }
			else // didn't even find a cell
			{
				_placementGhost.gameObject.SetActive(false);
                _arrow.gameObject.SetActive(false);
            }

            if (Input.GetMouseButtonDown(1))
                CancelPlacing();
		}
	}

	private void PlaceItem(Grid grid, Vector3 position)
	{
		switch (_mode) {
		case PlacerMode.CreateNew:
			{
				_curGrid = grid;
				if (OnPlacerEvent != null) {
					OnPlacerEvent (PlacerEventId.OnTryingPlace);
				}
				break;
			}
		case PlacerMode.Move:
			{
                if (!_curPlObject.inNavMesh)
                {
                    _curPlObject.FreeGridSpace();
                    _curPlObject.SetRotation(_curShape.Rotation);
                    _curPlObject.Place(grid, position);
                    CancelPlacing();
                }
                else
                {
                    _curPlObject.GetComponent<BaseAI>().JobQueue.EnQueue(new MoveJob(position));
                    CancelPlacing();
                }
				break;
			}

		default:
			return;
		}
	}

    private void CancelPlacing()
    {

		_ghostObject.SetActive (false);

        _placingItem = false;
		_ghostObject = null;
        _curShape = null;

        if(_arrow.gameObject.activeInHierarchy)
            _arrow.gameObject.SetActive(false);

        if (OnPlacerEvent != null)
			OnPlacerEvent (PlacerEventId.OnEndPlace);
        OnPlacerEvent = null; // clear subscriptions
    }
}

public enum PlacerEventId
{
	OnTryingPlace,
	OnEndPlace
}

public enum PlacerMode
{
	CreateNew,
	Move
}
