// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIList<T> where T:Component
{
    public T TemplateElement;
    public Vector2 ElementOffset;

    List<T> _createdElements = new List<T>();

    public T AddElement()
    {
        Vector3 offset = Vector3.zero;
        offset.y = ElementOffset.y * (_createdElements.Count+1);
        offset.x = ElementOffset.x * (_createdElements.Count+1);
        var go = Object.Instantiate(TemplateElement.gameObject);
        go.transform.position = TemplateElement.transform.position + offset;
        go.transform.SetParent(TemplateElement.transform.parent);
        var ret = go.GetComponent<T>();

        if (ret == null)
        {
            Debug.LogError("The list element must be a component of the root element!");
        }

        _createdElements.Add(ret);

        Vector2 parentSize = ((RectTransform) TemplateElement.transform.parent).sizeDelta;
        parentSize.y = -(offset.y + ElementOffset.y - 20f);
        ((RectTransform) TemplateElement.transform.parent).sizeDelta = parentSize;

        return ret;
    }

    public void DestroyElements()
    {
        foreach (var element in _createdElements)
        {
            Object.Destroy(element.gameObject);
        }
        _createdElements = new List<T>();
    }

    public void HideElements()
    {
        TemplateElement.gameObject.SetActive(false);
    }
}
