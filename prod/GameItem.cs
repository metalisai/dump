using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct InventoryAction
{
	public string displayName;
	public int id;
}

public enum ItemTag
{
	Burns,	// represents that the item can burn
    Cookable
}

public enum ItemProperty
{
	Mass,
	BurnEnergy,	// per kilogram
	BurnPower,
	BurnerEfficiency,
    MinCookTemperature,
    MaxCookTemperature,
    CookTime
}

public enum ItemAction
{
    Cooked,
    Overcooked
}

public class GameItem : MonoBehaviour, IInteractableObject
{
	public List<InventoryAction> InventoryActions = new List<InventoryAction>(); // should be static ?
	public List<ItemTag> Tags = new List<ItemTag>();  // should be static ?
	public Dictionary<ItemProperty,double> ItemProperties = new Dictionary<ItemProperty,double>();  // should be static ?
	public Sprite icon;
	public string _itemName;

	protected bool _pickable = true;

	public bool pickable
	{
		get { return _pickable; }
	}

	public IGameItemOwner owner;

	protected ObjectId _objectId;
	public ObjectId ObjectId
	{
		get { return _objectId; }
	}

	// Do NOT use unity API in here (constructors can be called on different thread)
	public GameItem ()
	{

	}

	public virtual void OnInventoryAction(int actionId)
	{

	}

    public virtual void OnAction(ItemAction action)
    {

    }

	protected virtual void Start()
	{
		
	}

	public virtual void OnDestroy()
	{
		if (owner != null) {
			owner.OnLoseOwnership(this);
		}
	}

	public void ShowActions()
	{
		string[] items = InventoryActions.Select( x => x.displayName).ToArray();
		PopupMenu.current.ShowPopupMenu(Input.mousePosition,items);
		PopupMenu.current.Listen(OnOption);
	}

	void OnOption(PopupMenuEventId eventId, int param)
	{
		switch (eventId) {
		case PopupMenuEventId.OptionChosen:
			Debug.Log("Option " + InventoryActions[param].displayName);
			PopupMenu.current.HidePopupMenu();
			OnInventoryAction(InventoryActions[param].id);
			break;
		}
	}

	public virtual void OnCombinedWithItem(GameItem item)
	{

	}


	public void GiveOwnershipTo(IGameItemOwner newOwner)
	{
        if (owner != null)
        {
            if (owner != newOwner)
                owner.OnLoseOwnership(this);
            else
                Debug.LogError("Why are you giving ownership if the object already has it!?");
        }
		owner = newOwner;
		owner.OnGainOwnership (this);
	}

	public void ResetOwnership()
	{
		if(owner != null)
			owner.OnLoseOwnership (this);
	}

    public void TransformToItem(ObjectId resultItem)
    {
        var go = ObjectDatabase.CreateObject(resultItem);
        var gi = go.GetComponent<GameItem>();
        if(this.gameObject.active)
        {
            go.transform.position = this.transform.position;
            go.transform.rotation = this.transform.rotation;
        }
        else
        {
            go.SetActive(false);
        }
        go.SetActive(gameObject.active);
        go.transform.SetParent(this.transform.parent);

        if(owner != null)
        {
            owner.OnOwnedItemTransform(this, gi);
        }
        Destroy(this.gameObject);
    }

    public void OnInteraction(IInteractableObject otherObject)
    {
        throw new System.NotImplementedException();
    }

    new public InteractableObjectType GetType()
    {
        return InteractableObjectType.GameItem;
    }

    public void OnInteractionWithOtherObject(IInteractableObject otherObject)
    {
        switch(otherObject.GetType())
        {
            case InteractableObjectType.GameItem:
                this.OnCombinedWithItem(otherObject as GameItem);
                break;

            default:
                Debug.LogError("Undefined interaction " + this + " and " + otherObject);
                break;
        }
    }

    public void OnPlayerPointingAt()
    {
        if (_pickable)
        { // valid item aimed at
            GlobalSettings.ItemDesc.SetActive(true);
            GlobalSettings.ItemDesc.GetComponent<Text>().text = _itemName;
            GlobalSettings.ItemDesc.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        }
        if(Input.GetMouseButtonDown(1))
            ShowActions();
    }

    public void OnPointingAtEnd()
    {
        GlobalSettings.ItemDesc.SetActive(false);
    }
}
