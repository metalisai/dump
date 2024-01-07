using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class PhysicalWireManager
{
    public const float WIRE_WIDTH = 0.2f;

    class WireSegment
    {
        public Vector3 positionLast;
        //public Vector3 position; // moved to sepparate array
        public bool pinned;
    }

    class PWire
    {
        public Vector3 start;
        public Vector3 end;
        public WireSegment[] segments;
        public Vector3[] segmentPositions;
        public float segmentLength;
        public void SetSegment(int index, Vector3 position, bool pinned)
        {
            segmentPositions[index] = position;
            segments[index] = new WireSegment() { /*position = position,*/ pinned = pinned, positionLast = position };
        }
        public LineRenderer lineRenderer;
    }

    static Dictionary<int, PWire> _wires = new Dictionary<int, PWire>();

    static float segmentMass = 1.0f;
    //float segmentLength = 1.0f;
    public static Vector3 outsideForce = new Vector3(0.0f, -9.8f, 0.0f);
    static int _wireHandles = 0;
    public static Material  WireMaterial;

    static int AddWire(PWire wire)
    {
        int ret = _wireHandles++;
        _wires.Add(ret, wire);
        return ret;
    }

    static PhysicalWireManager()
    {
        WireMaterial = new Material(Shader.Find("Standard"));
        WireMaterial.color = Color.black;
    }

    public static int CreateWire(Vector3 start, Vector3 end)
    {
        Vector3 startToEnd = end - start;
        float length = startToEnd.magnitude;
        int numsegments = 10;

        var pwire = new PWire();
        pwire.start = start;
        pwire.end = end;
        pwire.segmentLength = length / (float)numsegments;
        pwire.segments = new WireSegment[numsegments+1];
        pwire.segmentPositions = new Vector3[numsegments+1];

        var go = new GameObject("PhysicalWire");
        pwire.lineRenderer = go.AddComponent<LineRenderer>();
        pwire.lineRenderer.sharedMaterial = WireMaterial;
        pwire.lineRenderer.startWidth = WIRE_WIDTH;
        pwire.lineRenderer.endWidth = WIRE_WIDTH;

        Vector3 dir = startToEnd.normalized;
        for (int i = 0; i <= numsegments; i++)
        {
            Vector3 pos = start + i * dir;
            pwire.SetSegment(i, pos, true);
        }

        pwire.lineRenderer.positionCount = pwire.segmentPositions.Length;
        pwire.lineRenderer.SetPositions(pwire.segmentPositions);
        int handle = AddWire(pwire);
        return handle;
    }

    public static void DestroyWire(int handle)
    {
        var wire = _wires[handle];
        if(wire.lineRenderer != null)
            GameObject.Destroy(wire.lineRenderer.gameObject);
        _wires.Remove(handle);
    }

    public static void DestroyAll()
    {
        foreach(var wire in _wires.Values)
        {
            if (wire.lineRenderer != null)
                GameObject.Destroy(wire.lineRenderer.gameObject);
        }
        _wires.Clear();
        //Graphics.drw
    }

    public static void FixedUpdate()
    {
        foreach (var wire in _wires.Values)
        {
            var segments = wire.segments;
            var segmentPositions = wire.segmentPositions;
            int segc = segments.Length;
            segmentPositions[0] = wire.start;
            segmentPositions[segc - 1] = wire.end;
            float segmentLength = wire.segmentLength;

            float dt2 = Time.deltaTime * Time.deltaTime;
            for (int i = 0; i < segc; i++)
            {
                Vector3 buffer = segmentPositions[i];
                segmentPositions[i] = 2.0f * segmentPositions[i] - segments[i].positionLast + outsideForce * dt2;
                segments[i].positionLast = buffer;

                if (i == segc - 1)
                    continue;
                float myDistSqr = Vector3.SqrMagnitude(segmentPositions[i] - segmentPositions[i+1]);
                if (myDistSqr < Mathf.Epsilon)
                    continue;
                float alpha = Mathf.Sqrt((segmentLength * segmentLength) / (myDistSqr));
                float oneminusalpha = 1.0f - alpha;
                float oneoversum = 1.0f / (segmentMass + segmentMass);
                float cmx = (segmentMass * segmentPositions[i].x + segmentMass * segmentPositions[i + 1].x) * oneoversum;
                float cmy = (segmentMass * segmentPositions[i].y + segmentMass * segmentPositions[i + 1].y) * oneoversum;
                float cmz = (segmentMass * segmentPositions[i].z + segmentMass * segmentPositions[i + 1].z) * oneoversum;
                segmentPositions[i] = new Vector3(cmx * oneminusalpha + alpha * segmentPositions[i].x, cmy * oneminusalpha + alpha * segmentPositions[i].y, cmz * oneminusalpha + alpha * segmentPositions[i].z);
                segmentPositions[i + 1] = new Vector3(cmx * oneminusalpha + alpha * segmentPositions[i + 1].x, cmy * oneminusalpha + alpha * segmentPositions[i + 1].y, cmz * oneminusalpha + alpha * segmentPositions[i + 1].z);
            }
            wire.lineRenderer.SetPositions(segmentPositions);
        }
    }

    public static void OnDrawGizmos()
    {
        /*foreach (var wire in _wires.Values)
        {
            var segments = wire.segments;
            int lcount = segments.Count;
            Gizmos.color = Color.red;
            for (int i = 0; i < lcount; i++)
            {
                Gizmos.DrawSphere(segments[i].position, 0.1f);
                if (i != lcount - 1)
                {
                    Gizmos.DrawLine(segments[i].position, segments[i + 1].position);
                }
            }
        }*/
    }
}
