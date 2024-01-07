// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class InterfaceExtension
{

    public static T GetInterface<T>(this GameObject inObj) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
            return null;
        }

        return inObj.GetComponents<Component>().OfType<T>().FirstOrDefault();
    }

    public static T GetInterfaceInChildren<T>(this GameObject inObj) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
            return null;
        }

        return inObj.GetComponentsInChildren<Component>().OfType<T>().FirstOrDefault();
    }

    public static IEnumerable<T> GetInterfaces<T>(this GameObject inObj) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
            return Enumerable.Empty<T>();
        }

        return inObj.GetComponents<Component>().OfType<T>();
    }

    public static RectTransform AsRectTransform(this Transform trans)
    {
        return trans as RectTransform;
    }
}
