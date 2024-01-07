using UnityEngine;
using System.Collections;

public struct FireplaceBurningSlot
{
	public GameItem item;
	public double energy;
	public double power;
}

[System.Serializable]
public struct FireplaceCookingSlot
{
    public GameItem item;
    public bool cookable;
    public double minTemp;
    public double maxTemp;
    public double timeCooked;
    public double cookTime;
}

public class Fireplace : GameItem, IGameItemOwner {

	public int minFuelToLight = 10;

	FireplaceBurningSlot _burningSlot;
    FireplaceCookingSlot _cookingSlot;

	float _burningSpeed = 1;
	int _temperature = 110; // difference from environment temperature
	bool _burning = false;

	float _timer = 0f;

	protected override void Start()
	{
		_itemName = "Fireplace";
		_pickable = false;

		InventoryAction action;
		action.displayName = "Light";
		action.id = 1;
		InventoryActions.Add (action);
		_objectId = ObjectId.Fireplace;

		ItemProperties.Add (ItemProperty.BurnerEfficiency, 0.3d);

		base.Start ();
	}

	void Update()
	{
		if (!_burning)
			return;
		_timer += Time.deltaTime;
		if (_timer >= 1f) {
			_timer = 0f;
			Tick ();
		}
	}

	void Tick()
	{
		//Debug.Log (_slot.energy);
		_burningSlot.energy -= _burningSlot.power;
		if (_burningSlot.energy <= 0d)
			EndFire ();

        if(_cookingSlot.item != null && _cookingSlot.cookable)
        {
            if(_cookingSlot.minTemp <= _temperature && _cookingSlot.maxTemp > _temperature)
            {
                _cookingSlot.timeCooked += 1f;
                if(_cookingSlot.timeCooked >= _cookingSlot.cookTime)
                {
                    _cookingSlot.item.OnAction(ItemAction.Cooked);
                }
            }
            else if(_cookingSlot.minTemp <= _temperature)
            {
                _cookingSlot.item.OnAction(ItemAction.Overcooked);
            }
        }
	}
	
	public bool TryAddFuel(GameItem fuel)
	{
		if (fuel.Tags.Contains(ItemTag.Burns) && _burningSlot.item == null) {
			fuel.GiveOwnershipTo(this);
			_burningSlot.item = fuel;
			_burningSlot.energy = fuel.ItemProperties[ItemProperty.Mass]*fuel.ItemProperties[ItemProperty.BurnEnergy];
			_burningSlot.power = fuel.ItemProperties[ItemProperty.BurnPower];
			fuel.gameObject.SetActive(false);
			Debug.Log (_burningSlot);
            return true;
		}
		return false;
	}

    public bool AddCookItem(GameItem item)
    {
        if(_cookingSlot.item == null)
        {
            Debug.Log("no?");
            _cookingSlot.item = item;
            item.GiveOwnershipTo(this);
            if(item.Tags.Contains(ItemTag.Cookable))
            {
                _cookingSlot.cookable = true;
                _cookingSlot.timeCooked = 0f;
                _cookingSlot.minTemp = item.ItemProperties[ItemProperty.MinCookTemperature];
                _cookingSlot.maxTemp = item.ItemProperties[ItemProperty.MaxCookTemperature];
                _cookingSlot.cookTime = item.ItemProperties[ItemProperty.CookTime];
            }
            else
            {
                _cookingSlot.cookable = false;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

	bool LightFire()
	{
		Debug.Log (_burningSlot);
		if (!_burning && _burningSlot.item != null) {
			_burning = true;
			transform.GetChild(0).gameObject.SetActive(true);
			return true;
		}
		return false;
	}

	void EndFire()
	{
		Debug.Log ("endfire");
		Destroy (_burningSlot.item.gameObject);
		_burning = false;
		transform.GetChild(0).gameObject.SetActive(false);
	}

	public override void OnInventoryAction (int actionId)
	{
		switch (actionId) {
		case 1: // light
			LightFire();
			break;
		}
	}

	public override void OnCombinedWithItem (GameItem item)
	{
		if(!TryAddFuel (item))
        {
            AddCookItem(item);
        }
	}

	public void OnLoseOwnership (GameItem item)
	{
		if(item == _cookingSlot.item)
        {
            _cookingSlot.item = null;
        }
	}

	public void OnGainOwnership (GameItem item)
	{
		//throw new System.NotImplementedException ();
	}

    public void OnOwnedItemTransform(GameItem oldItem, GameItem newItem)
    {
        Debug.Log(oldItem);
        Debug.Log(newItem);
        if(oldItem == _cookingSlot.item)
        {
            Debug.Log("yews");
            _cookingSlot.item = null;
            AddCookItem(newItem);
        }
    }
}
