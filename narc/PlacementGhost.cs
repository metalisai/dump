// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public class PlacementGhost : MonoBehaviour
{
    public bool Colliding = false;
    private MeshRenderer[] _meshRenderers;

    void Awake()
    {
        MeshRenderer[] components = GetComponentsInChildren<MeshRenderer>();

        _meshRenderers = new MeshRenderer[components.Length];

        int i = 0;
        foreach (var component in components)
        {
            _meshRenderers[i] = component;
            i++;
        }
    }

    public void MakeGreen()
    {
        foreach (var meshRenderer in _meshRenderers)
        {
            var col = Color.green;
            col.a = 0.4f;

            var material = meshRenderer.material;

            meshRenderer.material.SetColor("_Color", col);
            meshRenderer.material.SetColor("_EmissionColor", col);

            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_EMISSION");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
    }

    public void MakeRed()
    {
        foreach (var meshRenderer in _meshRenderers)
        {
            var col = Color.red;
            col.a = 0.4f;

            var material = meshRenderer.material;

            meshRenderer.material.SetColor("_Color", col);
            meshRenderer.material.SetColor("_EmissionColor", col);

            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_EMISSION");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
    }

    void FixedUpdate()
    {
        // if this makes no sense, you should look up execution order
        Colliding = false;
    }

    void OnTriggerStay(Collider other)
    {
        Colliding = true;
    }
}
