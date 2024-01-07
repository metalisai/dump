using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour, IGameItemOwner {

	public List<GameItem> _inventory = new List<GameItem>();

	public static IInteractableObject _curObject;

	void Update()
	{
		// check for items
		RaycastHit hit;
		Ray scenter = Camera.main.ScreenPointToRay (Input.mousePosition);
		int mask = 3 << 8; // layer 8 == item layer
		if (Physics.Raycast (scenter, out hit, 2f, mask)) {
			var go = hit.collider.gameObject;
			var gi = go.GetComponent<IInteractableObject> ();
			if(gi != null)
			{
				_curObject = gi;
                gi.OnPlayerPointingAt();
			}
			else 
			{
				Debug.LogError ("An item without GameItem component!");
			}
		} else { // raycast didn't collide
			if(_curObject != null)
			{
                _curObject.OnPointingAtEnd();
				_curObject = null;
			}
		}

		if (Input.GetKeyDown (KeyCode.E) && _curObject != null && 
            _curObject.GetType() == InteractableObjectType.GameItem && ((GameItem)_curObject).pickable) {
			AddItemToInventory(_curObject as GameItem);
		}
	}

	public void AddItemToInventory(GameItem item)
	{
		item.gameObject.SetActive(false);
		_inventory.Add(item);
		item.GiveOwnershipTo(this);
	}

	public void RemoveItemFromInventory(GameItem item)
	{
		_inventory.Remove (item);
		Debug.Log ("Removed "+item);
		item.ResetOwnership();
	}

	public void OnLoseOwnership (GameItem item)
	{
		if (_inventory.Contains (item))
			_inventory.Remove (item);
	}

	public void OnGainOwnership (GameItem item)
	{

	}

    public void OnOwnedItemTransform(GameItem oldItem, GameItem newItem)
    {
        throw new System.NotImplementedException();
    }
}
