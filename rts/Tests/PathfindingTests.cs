using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PathfindingTests : MonoBehaviour
{
    Texture2D visualTexture;
    PathFinding pf;

    GridCellFlag[,] pfGrid;

    PathQueryResult res = null;
    float time = 0.0f;

    public void Start()
    {
        pf = new PathFinding();
        pfGrid = pf.grid.GetCells();
        int gridSize = pf.grid.GetCells().GetLength(0);
        visualTexture = new Texture2D(gridSize, gridSize);
        visualTexture.filterMode = FilterMode.Point;

        for(int i = 30; i < 70; i++)
        {
            for (int j = 30; j < 70; j++)
            {
                if(i == 30 || j ==30 || i == 69 || j == 69)
                    pfGrid[i, j] |= GridCellFlag.Occupied;
            }
        }

        for (int i = 10; i < 30; i++)
        {
            pfGrid[i, 20] |= GridCellFlag.Occupied;
        }

            pfGrid[30, 50] = GridCellFlag.Free;
        //res = pf.findPathWithDistanceAsync(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(70.0f, 0.0f, 70.0f), 10.0f, false);
        res = pf.findPathExactAsync(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(69.0f, 0.0f, 69.0f), false);
        Debug.Log("started");
    }

    List<Vector3> path;

    void Update()
    {
        pf.CheckDoneQueries();
        UpdateTexture();
        if (res != null && res.done)
        {
            Debug.Log("Path query took "+ time);
            time = 0.0f;
            path = res.waypoints;
            res = null;
        }
        else if(res != null)
        {
            time += Time.deltaTime;
        }

        if(path != null)
        {
            for(int i = 0; i < path.Count; i++)
            {
                if (i != path.Count - 1)
                {
                    Debug.DrawLine(path[i]+Vector3.up, path[i + 1]+Vector3.up, Color.blue);
                }
            }
        }
    }

    public void UpdateTexture()
    {
        int size = pfGrid.GetLength(0);
        for(int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int pixel = (int)pfGrid[i, j];
                if(pixel == 0)
                    visualTexture.SetPixel(i, j, Color.white);
                if(pixel == 1)
                    visualTexture.SetPixel(i, j, Color.black);
                if(pixel == 2)
                    visualTexture.SetPixel(i, j, Color.yellow);
                if((pixel & 4) != 0)
                    visualTexture.SetPixel(i, j, Color.green);
            }
        }
        visualTexture.Apply();
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", visualTexture);
    }

    void OnApplicationQuit()
    {
        if(pf != null)
            pf.Stop();
    }

    void OnDestroy()
    {
        if (pf != null)
            pf.Stop();
    }

    void OnDrawGizmos()
    {
        if (pf != null)
            pf.OnDrawGizmos();
    }
}
