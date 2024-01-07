using System;
using UnityEngine;

public class BeamRenderer : MonoBehaviour
{
	public const int Quads = 3;

	MeshFilter _mf;
	float _size = 1.0f;
	Material _material;

	public void Awake()
	{
		_mf = GetComponent<MeshFilter> ();
	}

	public static BeamRenderer CreateBeam(Vector3 start, Vector3 finish,float size, Color color)
	{
		Vector3 dir = (finish - start).normalized;

		Vector3[] sides = new Vector3[Quads];
		sides[0]= Vector3.Cross (dir, Vector3.up).normalized*size;
		for(int i = 0; i < Quads-1; i++)
			sides[i+1] = Quaternion.AngleAxis ((180.0f/Quads)*(i+1), dir) * sides[0];

		Vector3[] verts = new Vector3[4*sides.Length];
		Vector2[] uvs = new Vector2[4*sides.Length];
		int[] indices = new int[12*sides.Length];
		for(int i = 0; i < sides.Length; i++)
		{
			Vector3 side = sides[i];
			verts[4*i+0] = start + side;
			verts[4*i+1] = start - side;
			verts[4*i+2] = finish + side;
			verts[4*i+3] = finish - side;

			uvs [4*i+0] = new Vector2 (0.0f, 0.0f);
			uvs [4*i+1] = new Vector2 (1.0f, 0.0f);
			uvs [4*i+2] = new Vector2 (0.0f, 1.0f);
			uvs [4*i+3] = new Vector2 (1.0f, 1.0f);

			indices [12*i+0] = 4*i+0;
			indices [12*i+1] = 4*i+3;
			indices [12*i+2] = 4*i+1;
			indices [12*i+3] = 4*i+0;
			indices [12*i+4] = 4*i+2;
			indices [12*i+5] = 4*i+3;

			indices [12*i+6] = 4*i+1;
			indices [12*i+7] = 4*i+3;
			indices [12*i+8] = 4*i+0;
			indices [12*i+9] = 4*i+3;
			indices [12*i+10] = 4*i+2;
			indices [12*i+11] = 4*i+0;
		}

		Mesh mesh = new Mesh ();
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.triangles = indices;
		mesh.MarkDynamic ();
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds ();

		GameObject go = new GameObject ("Beam");
		var mf = go.AddComponent<MeshFilter> ();
		var mr = go.AddComponent<MeshRenderer> ();
		mr.material = new Material (Game.Instance.BeamShader);
		mr.material.SetColor ("_Color", color);
		var beam = go.AddComponent<BeamRenderer> ();
		beam._size = size;
		beam._material = mr.material;

		mf.mesh = mesh;

		return beam;
	}

	public void SetColor(Color color)
	{
		_material.SetColor ("_Color", color);
	}

	public void UpdateBeam(Vector3 start, Vector3 finish, float? size = null)
	{
		if (size != null)
			_size = size.Value;
		
		Mesh mesh = _mf.mesh;
		Vector3[] verts = mesh.vertices;

		Vector3 dir = (finish - start).normalized;
		Vector3[] sides = new Vector3[Quads];
		sides[0]= Vector3.Cross (dir, Vector3.up).normalized*_size;
		for(int i = 0; i < Quads-1; i++)
			sides[i+1] = Quaternion.AngleAxis ((180.0f/Quads)*(i+1), dir) * sides[0];
		for (int i = 0; i < Quads; i++) {
			verts[4*i+0] = start + sides[i];
			verts[4*i+1] = start - sides[i];
			verts[4*i+2] = finish + sides[i];
			verts[4*i+3] = finish - sides[i];
		}
		mesh.vertices = verts;
        _material.SetFloat("_Length", (start - finish).magnitude);
		mesh.RecalculateBounds ();
	}
}
