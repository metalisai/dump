// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

[RequireComponent(typeof(RectTransform))]
public class MovableWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IDropHandler, IPointerDownHandler{

    RectTransform _rt;
    Vector3 _origWindowPosition;
    Vector3 _origMousePosition;

    public const float DeadCornerSize = 20f;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _origWindowPosition = _rt.position;
        _origMousePosition = Input.mousePosition;
        SetOnTop();
    }

    public void OnDrop(PointerEventData eventData)
    {
        
    }

    public void SetOnTop()
    {
        var go = gameObject;
        int i = 0;
        // TODO: remove temp code
        while (go.transform.parent.GetComponent<Canvas>() == null && i < 10)
        {
            go = transform.parent.gameObject;
            i++;
        }
        if(i < 9)
            go.transform.SetSiblingIndex(go.transform.parent.childCount - 1);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 moveTo = _origWindowPosition+(Input.mousePosition - _origMousePosition);
        if (Input.mousePosition.x < Screen.width-DeadCornerSize && Mathf.Abs(Input.mousePosition.y) < Screen.height-DeadCornerSize
            && Input.mousePosition.x > DeadCornerSize && Mathf.Abs(Input.mousePosition.y) > DeadCornerSize)
        {
            _rt.position = moveTo;
        }
    }

    void Start () {
        _rt = transform as RectTransform;
	}

    void OnEnable()
    {
        SetOnTop();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetOnTop();
    }
}
