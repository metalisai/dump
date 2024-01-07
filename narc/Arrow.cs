// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public class Arrow : MonoBehaviour {

    GameObject arrowHead;
    GameObject arrowBody;

    float scale = 1f;

    void Awake()
    {
        {
            arrowHead = new GameObject();
            Mesh mesh = new Mesh();
            Vector3[] verts = new Vector3[3];
            verts[0] = new Vector3(-0.5f, 0f, -0.5f);
            verts[1] = new Vector3(-0.5f, 0f, 0.5f);
            verts[2] = new Vector3(0.5f, 0f, 0f);
            mesh.vertices = verts;
            int[] indices = new int[3];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            mesh.triangles = indices;
            mesh.RecalculateNormals();

            var mf = arrowHead.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            var meshRenderer = arrowHead.AddComponent<MeshRenderer>();
            meshRenderer.material = GlobalVariables.ArrowMat;

            arrowHead.transform.SetParent(this.gameObject.transform);
        }

        {
            arrowBody = new GameObject();
            Mesh mesh = new Mesh();
            Vector3[] verts = new Vector3[4];
            verts[0] = new Vector3(0f, 0f, -0.25f);
            verts[1] = new Vector3(0f, 0f, 0.25f);
            verts[2] = new Vector3(1f, 0f, -0.25f);
            verts[3] = new Vector3(1f, 0f, 0.25f);
            mesh.vertices = verts;
            int[] indices = new int[6];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 3;
            indices[3] = 0;
            indices[4] = 3;
            indices[5] = 2;
            mesh.triangles = indices;
            mesh.RecalculateNormals();

            var mf = arrowBody.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            var meshRenderer = arrowBody.AddComponent<MeshRenderer>();
            meshRenderer.material = GlobalVariables.ArrowMat;

            arrowBody.transform.SetParent(this.gameObject.transform);
            arrowBody.transform.localPosition = Vector3.zero;
        }
    }

    public void SetCoordinates(Vector3 orig, Vector3 dest)
    {
        gameObject.transform.position = orig;
        float dist = Vector3.Distance(dest, orig);
        scale = Mathf.Min(dist/2f, 1f);
        dist -= scale;
        arrowBody.transform.localScale = new Vector3(dist, 1f, scale);
        Vector3 dir = transform.position - dest;
        float y = Quaternion.LookRotation(dir).eulerAngles.y;
        Quaternion quat = transform.rotation;
        Vector3 eul = quat.eulerAngles;
        eul.y = y + 90f;
        quat.eulerAngles = eul;
        arrowHead.transform.rotation = quat;
        arrowBody.transform.rotation = quat;
        var pos = dest + scale/2 * dir.normalized;
        pos.y = transform.position.y;
        arrowHead.transform.position = pos;
        arrowHead.transform.localScale = new Vector3(scale,scale,scale);
    }

    public void SetDestination(Vector3 dest)
    {
        SetCoordinates(transform.position, dest);
    }

    public static Arrow CreateArrow(Vector3 origin, Vector3 dest)
    {
        var go = new GameObject();
        var arr = go.AddComponent<Arrow>();
        arr.SetCoordinates(origin, dest);
        arr.gameObject.SetActive(true);
        return arr;
    }
}
