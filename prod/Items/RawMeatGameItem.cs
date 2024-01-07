// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;

public class RawMeatGameItem : GameItem {

	protected override void Start () {
        _itemName = "Raw Meat";
        _pickable = true;

        InventoryAction action;
        action.displayName = "Eat";
        action.id = 1;
        InventoryActions.Add(action);
        _objectId = ObjectId.RawMeat;

        Tags.Add(ItemTag.Cookable);

        ItemProperties.Add(ItemProperty.MinCookTemperature, 99d);
        ItemProperties.Add(ItemProperty.MaxCookTemperature, 200d);
        ItemProperties.Add(ItemProperty.CookTime, 5d);

        base.Start();
	}
	
	void Update () {
	
	}

    public override void OnAction(ItemAction action)
    {
        switch(action)
        {
            case ItemAction.Cooked:
                TransformToItem(ObjectId.CookedMeat);
                break;
            case ItemAction.Overcooked:
                TransformToItem(ObjectId.BurnedOrganicMatter);
                break;
            default:
                base.OnAction(action);
                break;
        }
    }
}
