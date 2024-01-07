// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSelector : MonoBehaviour
{

	private GameObject _hoverObject = null;
    private Collider _curSelected = null;

	//private ItemPlacer _placer;

	private float _lastClickTime=0;
	float catchTime=0.5f;

	void Start()
	{
		//_placer = (ItemPlacer)FindObjectOfType (typeof(ItemPlacer));
	}

	// Update is called once per frame
	void Update ()
	{
		Vector3 mousePos = Input.mousePosition; // get cursor position
		
		Ray rayFromCursor = Camera.main.ScreenPointToRay (mousePos); // get ray from cursor position

		RaycastHit hit; // if the ray hits, info will be stored here
		
		LayerMask mask = NarcLayers.ItemsMask; // shift 1, so only items layer is in mask
		//Debug.Log (mask.value);
		if (Physics.Raycast (rayFromCursor, out hit, 50f, mask) && !EventSystem.current.IsPointerOverGameObject())
        {
			if (_hoverObject == null || hit.collider.gameObject != _hoverObject) // didnt hover last frame or hovered different object
            { 
                var objs = hit.collider.transform.GetComponentsInParent<IItemHoverHandler>();
                foreach(var obj in objs)
                {
                    obj.OnHover();
                }
            } 
			if (_hoverObject != null && _hoverObject != hit.collider.gameObject) // did hover last frame, but different object
            {    
                OnHoverEnd(_hoverObject);
            }
			else
			{
                if (Input.GetMouseButtonDown(0))
				{
                    if (Time.time - _lastClickTime <= catchTime)
                    {
                        var objs = hit.collider.transform.GetComponentsInParent<IItemDoubleclickHandler>();
                        foreach (var obj in objs)
                        {
                            obj.OnDoubleclick();
                        }
                        _lastClickTime = 0f;
                    }
                    else
                    {
                        _lastClickTime = Time.time;
                        if (_curSelected != hit.collider)
                        {
                            if (_curSelected != null)
                                OnDeSelected(_curSelected);
                            OnSelected(hit.collider);
                        }
                    }
				}
			}
			_hoverObject = hit.collider.gameObject;
		}
        // TODO: make sure the second part of if doesn't cause any weird behaviour
        else if (_hoverObject != null && !EventSystem.current.IsPointerOverGameObject())
        {
            OnHoverEnd(_hoverObject);
			_hoverObject = null;
		}
        else
        {
            if(Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    CancelSelection();
                }
            }
        }
	}

    public void CancelSelection()
    {
        Camera.main.gameObject.transform.GetChild(0).GetComponent<HighlightCamera>().ClearHighlighting();
        if (_curSelected != null)
        {
            // NOTE: order important!
            // NOTE: this is spaghetti to prevent infinite recursion!
            var item = _curSelected;
            _curSelected = null;
            OnDeSelected(item);
        }
    }

    void OnSelected(Collider item)
    {
        bool anyListeners = false;
        var objs = item.transform.GetComponentsInParent<IItemSelectHandler>();
        foreach (var obj in objs)
        {
            anyListeners =  obj.OnSelected() || anyListeners; // NOTE: order is important here!
        }

        if (anyListeners)
        {
            Debug.Log("Set highlighting to " + item.gameObject);
            Camera.main.gameObject.transform.GetChild(0).GetComponent<HighlightCamera>().SetHighlighting(item.gameObject);
            _curSelected = item;
        }
        else
        {
            Debug.Log("Highlighting not set to " + item.gameObject);
        }
    }

    void OnDeSelected(Collider item)
    {
        var objs = item.transform.GetComponentsInParent<IItemSelectHandler>();
        foreach (var obj in objs)
        {
            obj.OnDeSelecded();
        }
        Debug.Log("Deselect");
        _lastClickTime = 0f;
        _curSelected = null;
    }

    void OnHoverEnd(GameObject hov)
    {
        var objs = hov.transform.GetComponentsInParent<IItemHoverHandler>();
        foreach (var obj in objs)
        {
            obj.OnHoverEnd();
        }
    }

	public void PlacerEvent(PlacerEventId eventId)
	{
		if (eventId == PlacerEventId.OnEndPlace) {
			
		}
	}
}
