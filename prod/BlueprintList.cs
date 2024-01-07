using UnityEngine;
using System.Collections;

public class BlueprintList : MonoBehaviour
{

    public GameObject Element;
    public int ElementHeight;

    void Start()
    {
        for (int i = 1; i < 2; i++)
        {
            Vector3 offset = Vector3.zero;
            offset.y = -i*ElementHeight;

            var temp = GameObject.Instantiate(Element);
            temp.transform.SetParent(gameObject.transform);
            temp.transform.position = Element.transform.position + offset;
        }
    }
}
