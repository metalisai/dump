using UnityEngine;
using System.Collections;

public class ObjectPlacer : MonoBehaviour {

	bool _placing = false;
	GameItem _curItem;
    GameObject _curObject;

	public static ObjectPlacer current;

	void Start()
	{
		current = this;
	}

	public void StartPlacing(GameItem go)
	{
		_placing = true;
		_curItem = go;
        _curObject = go.gameObject;
	}

    public void StartPlacing(GameObject go)
    {
        _placing = true;
        _curObject = go;
    }

	void Update()
	{
		if (_placing) {
			RaycastHit hit;
			Ray scenter = Camera.main.ScreenPointToRay(Input.mousePosition);
			int mask = ~(3 << 8);
			if(Physics.Raycast(scenter,out hit,15f,mask))
			{
				_curObject.transform.position = hit.point;
			}
			if(Input.GetMouseButtonDown(0))
			{
				_placing = false;
				_curItem = null;
                _curObject = null;
			}
		}
	}
}
