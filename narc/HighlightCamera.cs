// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class HighlightCamera : MonoBehaviour {

    public Material Mat;
    public Shader ReplacementShader;
    public Camera Cam;

    GameObject _highlightObject;

    // TODO: monitor this dictionary
    Dictionary<GameObject, int> ChangedValues = new Dictionary<GameObject, int>();

    void Awake()
    {
        Cam = GetComponent<Camera>();
        if (ReplacementShader)
            Cam.SetReplacementShader(ReplacementShader, null);
        Cam.depthTextureMode = DepthTextureMode.Depth;
    }

    public void SetHighlighting(GameObject obj)
    {
        if (_highlightObject != null)
        {
            ClearHighlighting();
        }
        SetLayerRecursively(obj, NarcLayers.HighlightLayer);
        _highlightObject = obj;
        this.gameObject.SetActive(true);
    }

    public void ClearHighlighting()
    {
        if (_highlightObject != null)
        {
            SetOldLayerRecursively(_highlightObject);
        }
        _highlightObject = null;
        this.gameObject.SetActive(false);
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        if(!ChangedValues.ContainsKey(obj))
            ChangedValues.Add(obj, obj.layer);
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void SetOldLayerRecursively(GameObject obj)
    {
        if (null == obj)
        {
            return;
        }

        int oldvalue;
        ChangedValues.TryGetValue(obj, out oldvalue);
        obj.layer = oldvalue;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetOldLayerRecursively(child.gameObject);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, Mat);
    }
}

