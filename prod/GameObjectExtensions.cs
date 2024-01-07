using UnityEngine;
using System.Collections;

public static class GameObjectExtensions
{
    public static void SetLayerRecursively(this GameObject go, int layer)
    {
        go.layer = layer;
        foreach(Transform child in go.transform )
        {
            SetLayerRecursively( child.gameObject, layer );
        }
    }
}
