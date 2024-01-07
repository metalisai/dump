using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class ItemHighligher : ManagedMonoBehaviour
{
    struct GameObjectHighlightData
    {
        public MeshFilter[] meshfilters;
        public Material highlightMaterial;
    }

    CommandBuffer buf;
    RenderTexture tex;
    public Material SelectionStencilMaterial;
    public Material posProcMat;

    Camera clearCamera;
    Camera outlineFor;

    Dictionary<MeshFilter, Material> highlightMeshes = new Dictionary<MeshFilter, Material>();
    Dictionary<GameObject, GameObjectHighlightData> highlightGameObjects = new Dictionary<GameObject, GameObjectHighlightData>();

    void Start()
    {
        outlineFor = GetComponent<Camera>();
        GameObject cleargo = new GameObject("HighlightClearCamera");
        cleargo.transform.parent = transform;
        cleargo.transform.localPosition = Vector3.zero;
        cleargo.transform.localRotation = Quaternion.identity;
        clearCamera = cleargo.AddComponent<Camera>();

        // camera settings
        clearCamera.renderingPath = RenderingPath.Forward;
        clearCamera.cullingMask = 0;
        clearCamera.clearFlags = CameraClearFlags.Color;
        Color col = Color.white;
        col.a = 0.0f;
        clearCamera.backgroundColor = col;
        clearCamera.allowHDR = false;
        clearCamera.allowMSAA = false;
        clearCamera.useOcclusionCulling = false;
    }

    /// <summary>
    /// Highlight a mesh.
    /// </summary>
    /// <returns>True if not already added.</returns>
    bool HighlightMesh(MeshFilter meshFilter, Color color)
    {
        Material mat = new Material(SelectionStencilMaterial);
        mat.SetColor("_Color", color);
        if (!highlightMeshes.ContainsKey(meshFilter))
        {
            highlightMeshes.Add(meshFilter, mat);
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Remove highlight from a mesh.
    /// </summary>
    /// <returns>True if mesh was highlighted.</returns>
    public bool RemoveHighlight(MeshFilter meshFilter)
    {
        if (!highlightMeshes.ContainsKey(meshFilter))
        {
            highlightMeshes.Remove(meshFilter);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Highligths all meshes in gameobject and it's children.
    /// </summary>
    /// <remarks>DO NOT USE IF ANY PARTS OF THE GAMEOBJECT CAN BE DESTROYED DURING HIGHLIGHT</remarks>
    /// <param name="go"></param>
    /// <param name="color"></param>
    public bool HighlightGameObject(GameObject go, Color color)
    {
        if (highlightGameObjects.ContainsKey(go))
            return false;

        GameObjectHighlightData hdata;
        hdata.meshfilters = go.GetComponentsInChildren<MeshFilter>();
        hdata.highlightMaterial = new Material(SelectionStencilMaterial);
        hdata.highlightMaterial.SetColor("_Color", color);
        highlightGameObjects.Add(go, hdata);
        return true;
    }

    /// <summary>
    /// Removes highlight from gameObject
    /// </summary>
    /// <returns>True if the gameobject was highlighted</returns>
    public bool RemoveHighlight(GameObject go)
    {
        if(highlightGameObjects.ContainsKey(go))
        {
            highlightGameObjects.Remove(go);
            return true;
        }
        return false;
    }

    // Test code
    /*void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f))
        {
            if(Input.GetMouseButtonDown(0))
                HighlightGameObject(hit.collider.gameObject, Color.yellow);
            if (Input.GetMouseButtonDown(1))
                RemoveHighlight(hit.collider.gameObject);
        }
    }*/

    void OnDestroy()
    {
        if (tex != null)
            tex.Release();
    }

    void OnPreRender()
    {
        var cam = Camera.current;
        if (cam != outlineFor)
        {
            return;
        }

        if (buf == null)
        {
            buf = new CommandBuffer();
            tex = new RenderTexture(Screen.width, Screen.height, 0);
            clearCamera.targetTexture = tex;
            buf.name = "Item Selection";
            clearCamera.AddCommandBuffer(CameraEvent.AfterEverything, buf);
            buf.Clear();
        }
        else
        {
            buf.Clear();
        }

        RenderTargetIdentifier rtid = new RenderTargetIdentifier(tex);
        buf.SetRenderTarget(rtid, rtid);

        foreach(var hmesh in highlightMeshes)
        {
            var mf = hmesh.Key;
            buf.DrawMesh(mf.mesh, mf.transform.localToWorldMatrix, hmesh.Value);
        }

        foreach(var go in highlightGameObjects)
        {
            foreach(var mf in go.Value.meshfilters)
            {
                buf.DrawMesh(mf.mesh, mf.transform.localToWorldMatrix, go.Value.highlightMaterial);
            }
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (tex != null)
        {
            posProcMat.SetTexture("_OutlineTex", tex);
            Graphics.Blit(source, destination, posProcMat);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
