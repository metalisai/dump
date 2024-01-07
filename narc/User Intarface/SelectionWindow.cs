// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;

public class SelectionWindow : MonoBehaviour
{
    public DefaultSelectionWindow _defaultSel;
    public PlantSelectionWindow _plantSel;

    protected GameObject curSelection;
    public GameObject currentSelection
    {
        get
        {
            return curSelection;
        }
    }

    public void CancelSelection()
    {
        curSelection = null;
    }

    public void Hide()
    {
        _defaultSel.Hide();
        _plantSel.Hide();
    }

    public void SetSelected(PlaceableObject pobj)
    {
        Hide();
        _defaultSel.SetSelected(pobj);
        curSelection = pobj.gameObject;
    }

    public void SetSelected(GrowingPlant pobj)
    {
        Hide();
        _plantSel.SetSelected(pobj);
        curSelection = pobj.PlaceableObject.gameObject;
    }
}
