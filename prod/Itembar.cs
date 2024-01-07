using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Itembar : MonoBehaviour
{
	public GameObject refItem;
	public int width;
	public int margin;
	public int numSlots;
	public Sprite defIcon;

	ItembarSlot[] _slots;
	Player _player;

	// Use this for initialization
	void Start () {
		_slots = new ItembarSlot[numSlots];
		_slots [0] = refItem.GetComponent<ItembarSlot>();
		for (int i = 1; i < numSlots; i++) {
			GameObject curItem = GameObject.Instantiate<GameObject>(refItem);
			curItem.transform.SetParent(transform);
			Vector3 pos = refItem.transform.localPosition;
			pos.x += i*width+i*margin;
			curItem.transform.localPosition = pos;
			_slots[i] = curItem.GetComponent<ItembarSlot>();
		}

		_player = FindObjectOfType<Player> ();
	}

	void Update()
	{
		// TODO: cache the components
		int i = 0;
		foreach (var item in _slots) {
			item.item = i<_player._inventory.Count?_player._inventory[i]:null;
			i++;
			if (item.item == null) // slot empty
			{
				item.itemIcon.enabled = false;
				continue; // TODO: set to empty slot icon 
			}
			item.itemIcon.enabled = true;
			item.itemIcon.sprite = item.item.icon;
		}
	}
}
