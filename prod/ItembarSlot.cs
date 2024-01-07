using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Image))]
public class ItembarSlot : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler 
{
	public GameItem item;
	public Image itemIcon;
	public Vector3 origPos;

	bool dragged = false;

	public void Start()
	{
		itemIcon = transform.GetChild(0).GetComponent<Image> ();
		origPos = transform.position;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left) {
			dragged = true;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (dragged && eventData.button == PointerEventData.InputButton.Left) {
			dragged = false;
			gameObject.transform.position = origPos;
			if(Player._curObject != null && item != null)
			{
				Player._curObject.OnInteractionWithOtherObject(item);
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
			Debug.Log ("Left click");
		else if (eventData.button == PointerEventData.InputButton.Middle)
			Debug.Log ("Middle click");
		else if (eventData.button == PointerEventData.InputButton.Right && item != null) {
			item.ShowActions();
		}
	}

	void Update()
	{
		if (dragged) {
			transform.position = Input.mousePosition;
		}
	}
}
