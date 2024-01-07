// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.UI;

public class PlantSelectionWindow : DefaultSelectionWindow 
{
    public Text LightText;
    public Toggle AutoReplant;

	public void SetSelected(GrowingPlant plant)
    {
        AutoReplant.isOn = plant.Replant;
        GetComponentInChildren<ProgressBar>().TrackedItem = plant;
        LightText.text = plant.GetLightName(plant.GetBestLight());
        SetSelected(plant.PlaceableObject);
    }

    public void OnReplantChanged(bool value)
    {
        curSelection.GetComponentInChildren<GrowingPlant>().Replant = value;
    }
}
