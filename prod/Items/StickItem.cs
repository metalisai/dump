using UnityEngine;
using System.Collections;

public class StickItem : GameItem
{
	public StickItem ()
	{
		_itemName = "Stick";
		
		_objectId = ObjectId.Stick;
		
		Tags.Add (ItemTag.Burns);
		ItemProperties.Add (ItemProperty.BurnEnergy, 15000000d); // 15 megaJoules
		ItemProperties.Add (ItemProperty.BurnPower, 16666d); // 16kW
		ItemProperties.Add (ItemProperty.Mass, 0.2d); // 200g
	}

	protected override void Start ()
	{
		base.Start ();
	}

	public override void OnInventoryAction (int actionId)
	{
		base.OnInventoryAction (actionId);
	}
}
