using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class InteractionHandler
{
    public const float MAX_WIRE_LENGTH = 24.0f;

    Interactable wireStart;
    bool wiring = false;

    Vector3 rotatingMouseStart;
    bool rotatingKnob = false;
    Interactable knob;
    Nexus nex;
    LineRenderer _lineRenderer;
    Material _wireMaterialInvalid;

    bool otherCursor = false;

    public void Init()
    {
        var go = new GameObject("InteractionHandlerWire");
        _lineRenderer = go.AddComponent<LineRenderer>();
        _lineRenderer.sharedMaterial = PhysicalWireManager.WireMaterial;
        _lineRenderer.startWidth = PhysicalWireManager.WIRE_WIDTH;
        _lineRenderer.endWidth = PhysicalWireManager.WIRE_WIDTH;
        go.SetActive(false);

        _wireMaterialInvalid = new Material(Shader.Find("Standard"));
        _wireMaterialInvalid.color = Color.red;
    }

    void TryConnect(Interactable intr)
    {
        if (wiring && intr != wireStart)
        {
            if (Vector3.Distance(intr.transform.position, wireStart.transform.position) <= MAX_WIRE_LENGTH)
            {
                Game.Instance.WireManager.AddWire(wireStart, intr);
                EndConnect();
                Debug.Log("wired");
            }
            else
            {
                Debug.LogFormat("Wire distance too high: {0}", Vector3.Distance(intr.transform.position, wireStart.transform.position));
            }
        }
        else // start connecting
        {
            wireStart = intr;
            Game.TrackLifetime(wireStart.gameObject, InteractingWireDestroyed);
            UI.DisablePopups(this);
            _lineRenderer.gameObject.SetActive(true);
            UpdateWireRenderer(intr.transform.position, intr.transform.position); // just to make sure it doesn't draw some old position
        }
    }

    void BeginRotateKnob(Interactable intr)
    {
        if(!rotatingKnob)
        {
            rotatingMouseStart = Input.mousePosition;
            rotatingKnob = true;
            knob = intr;
            nex = intr.GetComponentInParent<Nexus>();
            Cursor.SetCursor(Game.Instance.LeftrightCursor, new Vector2(64.0f, 64.0f), CursorMode.Auto);
            otherCursor = true;
        }
    }

    void EndRotateKnob(Interactable intr)
    {
        rotatingKnob = false;
        knob = null;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        otherCursor = false;
    }

    void InteractingWireDestroyed(GameObject itrw)
    {
        Assert.AreEqual(itrw, wireStart.gameObject);
        EndConnect();
    }

    void EndConnect()
    {
        Game.StopTracking(wireStart.gameObject, InteractingWireDestroyed);
        wireStart = null;
        wiring = false;
        UI.EnablePopups(this);
        _lineRenderer.gameObject.SetActive(false);
    }

    public void UpdateWireRenderer(Vector3 start, Vector3 end)
    {
        if(_lineRenderer.positionCount != 2)
        {
            _lineRenderer.positionCount = 2;
        }
        _lineRenderer.SetPositions(new Vector3[] { start, end });
        if (Vector3.Distance(start, end) > MAX_WIRE_LENGTH)
            _lineRenderer.material = _wireMaterialInvalid;
        else
            _lineRenderer.sharedMaterial = PhysicalWireManager.WireMaterial;
    }

    public void Update()
    {
        #region Interactables
        RaycastHit hit;
        int mask = 1 << 8;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f, mask))
        {
            CellCoord coord = Game.pathFinding.WorldCoordToGrid(hit.point);
            var grid = Game.pathFinding.grid.GetMetaCells()[coord.x, coord.y].center;
            grid.y = Game.customTerrain.SampleHeight(grid);
            Debug.DrawLine(grid, grid + Vector3.up, Color.cyan);

            Interactable intr = hit.collider.gameObject.GetComponent<Interactable>();
            if (intr != null && !otherCursor)
                Cursor.SetCursor(Game.Instance.GrabCursor, new Vector2(64.0f, 64.0f), CursorMode.Auto);
            else if(!otherCursor)
                Cursor.SetCursor(null, Vector3.zero, CursorMode.Auto);
            if (Extensions.GetMouseButtonDownNoUI(0)
                && intr != null)
            {
                switch (intr.InteractableType)
                {
                    case Interactable.InteractionType.ConnectWire:
                        {
                            TryConnect(intr);
                        }
                        break;
                    case Interactable.InteractionType.RotateKnob:
                        {
                            BeginRotateKnob(intr);
                        }
                        break;
                }
            }

            if (wireStart != null && !wiring && Input.GetMouseButtonUp(0))
            {
                wiring = true;
            }

            if (wiring && Input.GetMouseButtonDown(1))
                EndConnect();

            if (wiring)
            {
                Debug.DrawLine(wireStart.transform.position, hit.point);
                UpdateWireRenderer(wireStart.transform.position, hit.point);
                if (Extensions.GetMouseButtonDownNoUI(0))
                {
                    EndConnect();
                }
            }

            if(rotatingKnob)
            {
                if(Input.GetMouseButtonUp(0))
                {
                    EndRotateKnob(knob);
                }
                else
                {
                    nex.ModifyKnob(((Input.mousePosition.x - rotatingMouseStart.x)/Screen.width)*500.0f);
                    rotatingMouseStart = Input.mousePosition;
                }
            }
        }
        else if(wiring) // if wiring but raycast didn't hit anything then just draw the floaring wire to 10 units ahead of mouse ray
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            UpdateWireRenderer(wireStart.transform.position, ray.GetPoint(10.0f));
        }
        #endregion
    }
}
