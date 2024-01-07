using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public struct PopupMenuElement
{
	public GameObject go;
	public Text text;
	public Button button;
}

public enum PopupMenuEventId
{
	OptionChosen
}

public class PopupMenu : MonoBehaviour {

	public static PopupMenu current;
	public int elementHeight = 20;
	public float hideDistance = 20f;
	public delegate void PopupmenuEvent(PopupMenuEventId eventId, int param);

	event PopupmenuEvent OnPopupmenuEvent = delegate {};
	PopupMenuElement[] elements;
	bool shown = false;

	void Awake()
	{
		elements = new PopupMenuElement[1];
		elements [0].go = transform.GetChild (0).gameObject;
		elements [0].go.SetActive (true);
		elements [0].text = elements [0].go.GetComponentInChildren<Text> ();
		elements [0].button = elements [0].go.GetComponent<Button> ();
		elements [0].button.onClick.AddListener(delegate{OnPopupmenuEvent(PopupMenuEventId.OptionChosen, 0);});
		if (elements [0].go == null)
			Debug.LogError ("PopupMenu is missing a reference element!");
		elements [0].go.SetActive (false);
		current = this;
	}

	void Update()
	{
		// TODO: can cache Rect if needed
		if (shown) {
			RectTransform trans = gameObject.transform as RectTransform;
			float minx = trans.position.x - trans.sizeDelta.x/2;
			float miny = trans.position.y - trans.sizeDelta.y/2;
			float maxx = trans.position.x + trans.sizeDelta.x/2;
			float maxy = trans.position.y + trans.sizeDelta.y/2;

			Vector3 p = Input.mousePosition;

			float dx = Mathf.Max(minx - p.x, 0, p.x - maxx);
			float dy = Mathf.Max(miny - p.y, 0, p.y - maxy);
			float dist = Mathf.Sqrt(dx*dx + dy*dy); // mouse distance from the popupmenu

			if(dist > hideDistance)
				HidePopupMenu();
		}
	}

	public void ShowPopupMenu(Vector3 position, string[] menuItems)
	{
		if (shown)
			HidePopupMenu ();

		if (menuItems.Length >= 1) {

			gameObject.SetActive(true);
			RectTransform rtr = transform as RectTransform;
			Vector2 size = rtr.sizeDelta;
			size.y = menuItems.Length*elementHeight+10f;
			rtr.sizeDelta = size;
			Vector3 pos = position;
			pos.y -= size.y/2f;
			pos.x += size.x/2f;
			rtr.position = pos;

			if(elements.Length < menuItems.Length) // resize
			{
				PopupMenuElement[] newElements = new PopupMenuElement[menuItems.Length];
				for(int i = 0; i < elements.Length; i++)
				{
					newElements[i] = elements[i];
				}
				elements = newElements;
			}

			for(int i = 0; i < elements.Length; i++)
			{
				if(elements[i].go == null)
				{
					elements[i].go = GameObject.Instantiate(elements[0].go);
					elements[i].text = elements[i].go.GetComponentInChildren<Text>();
					elements[i].button = elements[i].go.GetComponent<Button>();
					int option = i;
					elements[i].button.onClick.AddListener(delegate{OnPopupmenuEvent(PopupMenuEventId.OptionChosen, option);});
				}
				elements[i].go.transform.SetParent(transform);
				elements[i].go.transform.localPosition = elements[0].go.transform.localPosition + new Vector3(0f,(float)(-i*elementHeight),0f);
				elements[i].go.SetActive(true);
				elements[i].text.text = menuItems[i];
			}
			shown = true;
		}
	}

	// the listener will be automatically removed after the popup is gone
	public void Listen(PopupmenuEvent func)
	{
		OnPopupmenuEvent += func;
	}

	public void HidePopupMenu()
	{
		gameObject.SetActive (false);
		foreach (var el in elements) {
			el.go.SetActive(false);
		}
		OnPopupmenuEvent = delegate {};
		shown = false;
	}
}
