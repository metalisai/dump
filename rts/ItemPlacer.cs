using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class ItemPlacer
{
    public delegate void PlacerStartEventHandler();
    public delegate void PlacerPlaceEventHandler(PlaceableObject placedObject, GameObjectID objId);
    public event PlacerStartEventHandler PlacingStarted;
    public event PlacerPlaceEventHandler ObjectPlaced;
    public event PlacerStartEventHandler PlacingEnded;

    bool placing = false;
    GameObjectID placingObjectID;
    PlaceableObject placingObject = null;
    bool noScripts = false;

    GameObject currentGhost = null;
    Material ghostMat;

    Vector3 lastPoint;

    public void DisableScripts()
    {
        noScripts = true;
    }

    void MakeGhostRed()
    {
        if (currentGhost == null)
            return;
        foreach (var renderer in currentGhost.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material = ghostMat;
            var col = Color.red;
            renderer.material.SetColor("_EmissionColor", col);
        }
    }

    void MakeGhostGreen()
    {
        if (currentGhost == null)
            return;
        foreach (var renderer in currentGhost.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material = ghostMat;
            var col = Color.green;
            renderer.material.SetColor("_EmissionColor", col);
        }
    }

    void CreateGhost(PlaceableObject pobj)
    {
        // TOOD: maybe something better?
        if (currentGhost == null)
        {
            GameObject ghost = GameObject.Instantiate(pobj.gameObject);
            foreach (var script in ghost.GetComponents<MonoBehaviour>())
            {
                Debug.Log("dest " + script.GetInstanceID());
                
                MonoBehaviour.Destroy(script);
            }
            Game.Instance.RegisterDynamicObject(ghost, false, false);

            currentGhost = ghost;
        }
        //currentGhost.transform.SetParent(go.transform);
        //currentGhost.transform.localPosition = Vector3.zero;
        currentGhost.SetActive(true);
    }

    PlaceableObject Create(GameObjectID id)
    {
        var resource = Resources.Load<GameObject>(ObjectRegister.GetResourcePath(id));
        resource.SetActive(false);
        if (resource == null)
            return null;
        var go = GameObject.Instantiate(resource);
        Game.Instance.RegisterDynamicObject(go, false, !noScripts);

        // TODO: can just use ghost if noscript?
        if (noScripts)
        {
            foreach (var script in go.GetComponents<MonoBehaviour>())
            {
                if (!(script is PlaceableObject))
                {
                    MonoBehaviour.Destroy(script);
                }
            }
        }
        var po = go.GetComponent<PlaceableObject>();
        Assert.IsNotNull(po, "To place an object, it must have PlaceableObject component attached!");
        po.LoadMask();
        return po;
    }

    public bool PlaceObject(GameObjectID id)
    {
        EndPlacingIfPlacing();

        var po = Create(id);
        CreateGhost(po);
        Assert.IsNotNull(po);
        placing = true;
        placingObject = po;
        placingObjectID = id;
        if (PlacingStarted != null)
            PlacingStarted.Invoke();
        return true;
    }

    public bool PlaceObject(GameObjectID id, CellCoord location, int rotation)
    {
        var po = Create(id);
        po.SetRotation(rotation);
        bool placed = po.TryPlace(location);
        Assert.IsTrue(placed);
        po.gameObject.SetActive(true);
        po.Activate();
        return true;
    }

    public bool AttemptPlace()
    {
        //Game.pathFinding.grid.MarkOccupied(cell, true);
        if (placingObject.TryPlace(lastPoint))
        {
            placingObject.gameObject.SetActive(true);
            var po = Create(placingObjectID);
            placingObject = po;
            Debug.Log("Placed");
            return true;
        }
        else
        {
            return false;
        }
    }

    void EndPlacingIfPlacing()
    {
        if(placing)
        {
            Game.Instance.DestroyDynamicObject(placingObject.gameObject);
            placing = false;
            placingObject = null;
            Game.Instance.DestroyDynamicObject(currentGhost);
            currentGhost = null;
            if (PlacingEnded != null)
                PlacingEnded.Invoke();
        }
    }

    public void Update()
    {
        // TODO: just make a initialize function or something
        if(ghostMat == null)
        {
            ghostMat = Resources.Load<Material>("Materials/PlacementGhost");
        }

        if(placing)
        {
            RaycastHit hit;
            int mask = 1 << 8;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f, mask))
            {
                lastPoint = hit.point;
                placingObject.gameObject.transform.position = placingObject.GetSnappedPosition(lastPoint);

                if (currentGhost != null)
                {
                    currentGhost.transform.position = placingObject.transform.position;
                    currentGhost.transform.rotation = placingObject.transform.rotation;
                    // TODO: only call those functions when the state actually changes
                    if (placingObject.CanPlace(lastPoint))
                        MakeGhostGreen();
                    else
                        MakeGhostRed();
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (ObjectPlaced != null)
                        ObjectPlaced.Invoke(placingObject, placingObjectID);
                    else // no one listening so just place it
                        AttemptPlace();
                }
                else if(Input.GetMouseButtonDown(1))
                {
                    EndPlacingIfPlacing();
                }
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    placingObject.RotateRight();
                }

                /*var mWheel = Input.GetAxis("Mouse ScrollWheel");
                if (mWheel > 0)
                    placingObject.RotateLeft();
                if (mWheel < 0)
                    placingObject.RotateRight();*/
            }
        }
    }
}
