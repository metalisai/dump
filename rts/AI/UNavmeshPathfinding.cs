using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

public class UNavmeshPathfinding
{
    NavMeshData _navMesh;
    NavMeshDataInstance _navDataInstance;
    public Vector3 m_Size = new Vector3(1000.0f, 20.0f, 1000.0f);
    Vector3 _trackedPosition = new Vector3(500.0f, 0.0f, 500.0f);

    List<NavMeshBuildSource> _sources;

    static Vector3 Quantize(Vector3 v, Vector3 quant)
    {
        float x = quant.x * Mathf.Floor(v.x / quant.x);
        float y = quant.y * Mathf.Floor(v.y / quant.y);
        float z = quant.z * Mathf.Floor(v.z / quant.z);
        return new Vector3(x, y, z);
    }

    Bounds QuantizedBounds()
    {
        // Quantize the bounds to update only when theres a 10% change in size
        var center =_trackedPosition;
        return new Bounds(Quantize(center, 0.1f * m_Size), m_Size);
    }

    public void Initialize(List<NavMeshBuildSource> sources)
    {
        _navMesh = new NavMeshData();
        _navDataInstance = NavMesh.AddNavMeshData(_navMesh);
        _sources = sources;

        var asyncUpdate = true;
        var defaultBuildSettings = NavMesh.GetSettingsByID(0);
        //var bounds = QuantizedBounds();
        if (asyncUpdate)
            /*m_Operation = */NavMeshBuilder.UpdateNavMeshDataAsync(_navMesh, defaultBuildSettings, sources, QuantizedBounds());
        else
            NavMeshBuilder.UpdateNavMeshData(_navMesh, defaultBuildSettings, sources, QuantizedBounds());
    }

    public void Update()
    {
        //m_Size = new Vector3(100.0f, 20.0f, 100.0f);
        var defaultBuildSettings = NavMesh.GetSettingsByID(0);
        NavMeshBuilder.UpdateNavMeshDataAsync(_navMesh, defaultBuildSettings, _sources, QuantizedBounds());
    }

    public void AddSources(List<NavMeshBuildSource> sources)
    {
        _sources.AddRange(sources);
    }
}
