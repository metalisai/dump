using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(Wall))]
public class BezierCurveInspector : Editor
{

    private Wall curve;
    private Transform handleTransform;
    private Quaternion handleRotation;

    private void OnSceneGUI()
    {
        curve = target as Wall;
        handleTransform = curve.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        Vector3[] points = new Vector3[curve.sourcePoints.Count];
        for (int i = 0; i < curve.sourcePoints.Count; i++)
        {
            points[i] = ShowPoint(i);
        }

        /*Handles.color = Color.white;
        for(int i = 0; i < curve.points.Length; i++)
        {
            if (i >= curve.points.Length - 1)
                continue;
            Handles.DrawLine(points[i], points[i+1]);
        }*/
    }

    private Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(curve.sourcePoints[index]);
        EditorGUI.BeginChangeCheck();
        point = Handles.DoPositionHandle(point, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curve, "Move Point");
            EditorUtility.SetDirty(curve);
            curve.sourcePoints[index] = handleTransform.InverseTransformPoint(point);
            curve.RecalculateControlPoints();
            curve.UpdateMesh();
        }
        return point;
    }
}

public class Wall : ManagedMonoBehaviour
{
    public List<Vector3> sourcePoints = new List<Vector3>();
    public Vector3[] points;
    bool generated = false;
    public bool loop = false;
    const int segments = 30;

    Vector3 CurveQuadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float omt = 1.0f - t;
        return omt * omt * p0 + 2.0f * omt * t * p1 + t * t * p2;
    }

    Vector3 CurveCubic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float omt = 1.0f - t;
        return omt*omt*omt*p0 + 3.0f*omt*omt*t*p1 + 3.0f*omt*t*t*p2 + t*t*t*p3;
    }

    Vector3 CubicDeriv(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float omt = 1.0f - t;
        return 3.0f * omt * omt * (p1 - p0) + 6.0f * omt * t * (p2 - p1) + 3.0f * t * t *(p3 - p2);
    }

    void Start()
    {
        
        var mf = gameObject.AddComponent<MeshFilter>();
        var mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Standard"));
        if(sourcePoints.Count >= 2)
        {
            var mesh = GenerateMesh();
            mf.mesh = mesh;
        }
        generated = true;
    }

    public void AddPoint(Vector3 point)
    {
        sourcePoints.Add(point);
        if(sourcePoints.Count >= 2)
        {
            RecalculateControlPoints();
            UpdateMesh();
        }
    }

    static void GetControlPoints(Vector3 point1, Vector3 point2, Vector3 point3, out Vector3 cp1, out Vector3 cp2, float t = 0.5f)
    {
        var d01 = (point1 - point2).magnitude;
        var d12 = (point2 - point3).magnitude;
        var fa = t * d01 / (d01 + d12);
        var fb = t * d12 / (d01 + d12);
        cp1 = point2 - fa * (point3 - point1);
        cp2 = point2 + fb * (point3 - point1);
    }

    public void RecalculateControlPoints()
    {
        if (sourcePoints.Count < 2)
            return;

        points = new Vector3[(sourcePoints.Count-1) * 3 + 1];
        for(int i = 0; i < sourcePoints.Count; i++)
        {
            points[i * 3] = sourcePoints[i];
        }

        for (int i = 0; i < points.Length; i++)
        {
            if (i%3 == 0)
            {
                Vector3 prev, next, cp1, cp2;
                if (i != 0 && i != points.Length - 1) // somewhere in the middle
                {
                    prev = points[i - 3];
                    next = points[i + 3];
                    GetControlPoints(prev, points[i], next, out cp1, out cp2);
                    points[i - 1] = cp1;
                    points[i + 1] = cp2;
                }
                else if (i == 0) // start
                {
                    if (!loop)
                        prev = points[i];
                    else
                        prev = points[points.Length - 1];
                    next = points[i + 3];
                    GetControlPoints(prev, points[i], next, out cp1, out cp2);
                    points[i + 1] = cp2;
                }
                else // end
                {
                    prev = points[i - 3];
                    if (!loop)
                        next = points[i];
                    else
                        next = points[0];
                    GetControlPoints(prev, points[i], next, out cp1, out cp2);
                    points[i - 1] = cp1;
                }
            }
        }
    }

    public void UpdateMesh()
    {
        if (!generated)
            return;
        // TODO: try to reuse old mesh?
        var mesh = GenerateMesh();
        var mf = GetComponent<MeshFilter>();
        Destroy(mf.sharedMesh);
        mf.mesh = mesh;
    }

    HashSet<CellCoord> cells = new HashSet<CellCoord>();
    public Mesh GenerateMesh()
    {
        // free all cells
        foreach (var cell in cells)
        {
            Game.pathFinding.grid.ForceFreeCell(cell);
        }
        cells.Clear();

        float step = 1.0f / segments;
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        int curveCount = (points.Length - 1) / 3;
        for (int curveIndex = 0; curveIndex < curveCount || curveIndex == curveCount && loop; curveIndex++)
        {
            for (int i = 0; i < segments; i++)
            {
                int curIndex = vertices.Count;
                int si = curveIndex * 3;
                Vector3 curvePoint, derivative;
                if (curveIndex == curveCount && loop)
                {
                    Vector3 cp1 = points[points.Length - 1] + (points[points.Length - 1] - points[points.Length - 2]);
                    Vector3 cp2 = points[0] + (points[0] - points[1]);
                    curvePoint = CurveCubic(points[points.Length-1], cp1, cp2, points[0], i * step);
                    derivative = CubicDeriv(points[points.Length - 1], cp1, cp2, points[0], i * step);
                }
                else
                {
                    curvePoint = CurveCubic(points[si + 0], points[si + 1], points[si + 2], points[si + 3], i * step);
                    derivative = CubicDeriv(points[si + 0], points[si + 1], points[si + 2], points[si + 3], i * step);
                }
                Vector3 toSide = Vector3.Cross(derivative, Vector3.up).normalized;
                cells.Add(Game.pathFinding.WorldCoordToGrid(curvePoint + toSide + transform.position));
                cells.Add(Game.pathFinding.WorldCoordToGrid(curvePoint - toSide + transform.position));
                vertices.Add(curvePoint + toSide);
                vertices.Add(curvePoint - toSide);
                vertices.Add(curvePoint + toSide + Vector3.up * 4.0f);
                vertices.Add(curvePoint - toSide + Vector3.up * 4.0f);
                int lastIndex = loop ? curveCount : curveCount - 1;
                if (curveIndex == lastIndex && i == (segments-1)) // if last curve and last segment then add ending segment (otherwise don't need because the first of next will be there)
                {
                    if(loop)
                    {
                        Vector3 cp1 = points[points.Length - 1] + (points[points.Length - 1] - points[points.Length - 2]);
                        Vector3 cp2 = points[0] + (points[0] - points[1]);
                        curvePoint = CurveCubic(points[points.Length - 1], cp1, cp2, points[0], segments * step);
                        derivative = CubicDeriv(points[points.Length - 1], cp1, cp2, points[0], segments * step);
                    }
                    else
                    {
                        curvePoint = CurveCubic(points[si + 0], points[si + 1], points[si + 2], points[si + 3], segments * step);
                        derivative = CubicDeriv(points[si + 0], points[si + 1], points[si + 2], points[si + 3], segments * step);
                    }
                    toSide = Vector3.Cross(derivative, Vector3.up).normalized;
                    vertices.Add(curvePoint + toSide);
                    vertices.Add(curvePoint - toSide);
                    vertices.Add(curvePoint + toSide + Vector3.up*4.0f);
                    vertices.Add(curvePoint - toSide + Vector3.up * 4.0f);
                }
            }
        }

        int count = vertices.Count;
        for (int i = 0; i < count; i += 4)
        {
            if (i != vertices.Count - 4)
            {
                // left side quad
                indices.Add(i + 4); indices.Add(i + 6); indices.Add(i + 2);
                indices.Add(i + 4); indices.Add(i + 2); indices.Add(i + 0);
                // right side quad
                indices.Add(i + 1); indices.Add(i + 3); indices.Add(i + 5);
                indices.Add(i + 5); indices.Add(i + 3); indices.Add(i + 7);
                // top quad
                indices.Add(i + 3); indices.Add(i + 2); indices.Add(i + 6);
                indices.Add(i + 7); indices.Add(i + 3); indices.Add(i + 6);
            }
        }

        if (!loop) // if not looped then fill start and end
        {
            // start quad
            indices.Add(0); indices.Add(2); indices.Add(1);
            indices.Add(1); indices.Add(2); indices.Add(3);
            // end quad
            int index = vertices.Count - 4;
            indices.Add(index + 1); indices.Add(index + 2); indices.Add(index + 0);
            indices.Add(index + 3); indices.Add(index + 2); indices.Add(index + 1);
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        mesh.uv = new Vector2[vertices.Count];
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // mark all passed cells as occupied
        foreach(var cell in cells)
        {
            Debug.LogError("MARK OWNER");
            Game.pathFinding.grid.MarkOccupied(cell, true, null);
        }

        return mesh;
    }

    void OnDestroy()
    {
        foreach (var cell in cells)
        {
            Game.pathFinding.grid.ForceFreeCell(cell);
        }
    }

    void OnDrawGizmos()
    {
        if (sourcePoints.Count < 2)
            return;

        float step = 1.0f / (float)segments;

        int curveCount = (points.Length-1) / 3;
        for(int curveIndex = 0; curveIndex < curveCount; curveIndex++)
        {
            for (int i = 0; i < segments; i++)
            {
                int si = curveIndex * 3;
                Vector3 start = CurveCubic(points[si+0], points[si+1], points[si+2], points[si+3], i * step);
                Vector3 end = CurveCubic(points[si+0], points[si+1], points[si+2], points[si+3], (i + 1) * step);
                Gizmos.DrawLine(transform.position + start, transform.position + end);
            }
        }

        for(int i = 0; i < points.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(points[i] + transform.position, 1.0f);
        }

        var gmcells = Game.pathFinding.grid.GetMetaCells();
        foreach (var cell in cells)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(gmcells[cell.x, cell.y].center, gmcells[cell.x, cell.y].center+Vector3.up);
        }
    }
}

public static class CurvedWallPlacer
{
    public static Wall currentWall;

    public static void StartPlacing()
    {
        var go = new GameObject("Wall");
        currentWall = go.AddComponent<Wall>();
    }

    public static void EndPlacing()
    {
        currentWall = null;
    }

    public static void Update()
    {
        RaycastHit hit;
        if (currentWall != null && Input.GetMouseButtonDown(1))
            EndPlacing();
        if (currentWall != null && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f))
        {
            if (Input.GetMouseButtonDown(0))
            {
                
                Vector3 snapped = Game.pathFinding.GridCoordToWorld(Game.pathFinding.WorldCoordToGrid(hit.point));
                currentWall.AddPoint(snapped);
            }
        }
    }
}
