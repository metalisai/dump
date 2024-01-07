using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public class CustomTerrain : ManagedMonoBehaviour
{
    public float Size
    {
        get { return size; }
    }

    public Mesh TerrainMesh = null;
    public Material TerrainMaterial;

    void Start()
    {
        var mapFile = Resources.Load<TextAsset>("Maps/" + name);
        var heightmapFile = Resources.Load<TextAsset>("Maps/" + name + "-hmap");
        var splatmapFile = Resources.Load<TextAsset>("Maps/" + name + "-splat");

        if (TerrainMaterial == null)
            TerrainMaterial = new Material(Shader.Find("Standard"));

        Debug.Log(" " + mapFile != null + " " + heightmapFile);
        if (mapFile != null && heightmapFile != null && splatmapFile != null)
        {
            var mapText = mapFile as TextAsset;
            GameMap map = JsonUtility.FromJson<GameMap>(mapText.text);
            if (map != null)
            {
                Debug.LogFormat("Map '{0} loaded, size: {1}'", name, map.Size);
                if (!Application.isEditor)
                {
                    Game.localPlayer.SetCameraPosition(map.InitialCameraPosition);
                    Game.localPlayer.SetCameraRotation(map.InitialCameraRotation);
                }
                Load(map.Size, heightmapFile.bytes, splatmapFile.bytes);
            }
            else
            {
                Create(1000, 64);
            }
        }
        else if (mapFile == null)
        {
            Debug.LogError("Map named <" + name + ">not found!");
            Create(1000, 64);
        }
        else if (heightmapFile == null)
        {
            Debug.LogError("Map named <" + name + "> doesn't have heighmap!");
            Create(1000, 64);
        }
        else
        {
            Debug.LogError("Map named <" + name + "> doesn't have splat!");
            Create(1000, 64);
        }
        gameObject.layer = 8;
    }

	public float SampleHeight(Vector3 position)
	{
		if (heightMap == null)
			return 0.0f;

		int res = heightmapResolution;
		float unitsPerPixel = this.size / res;
		float repeatX = (position.x) / unitsPerPixel;
		float repeatY = (position.z) / unitsPerPixel;
		int ucx = Mathf.FloorToInt(repeatX);
		int ucy = Mathf.FloorToInt(repeatY);
		int x = Mathf.Clamp(ucx, 0, res-1);
		int y = Mathf.Clamp(ucy, 0, res-1);
		if (x == res - 1 || y == res - 1)
			return 0.0f;

		Vector3 bottomLeftVertex = new Vector3 (x * unitsPerPixel, heightMap [y, x]*maxHeight, y*unitsPerPixel);
		Vector3 bottomRightVertex = new Vector3 ((x+1) * unitsPerPixel, heightMap [y, x+1]*maxHeight, y*unitsPerPixel);
		Vector3 topRightVertex = new Vector3 ((x+1) * unitsPerPixel, heightMap [y+1, x+1]*maxHeight, (y+1)*unitsPerPixel);
		Vector3 topLeftVertex = new Vector3 (x * unitsPerPixel, heightMap [y+1, x]*maxHeight, (y+1)*unitsPerPixel);

		float distToOrig = Vector3.SqrMagnitude(position-bottomLeftVertex);
		float distToTopRight = Vector3.SqrMagnitude (position - topRightVertex);
		Vector3 p = new Vector3(position.x, 0.0f, position.z);
		Vector3 a, b, c;

		if (distToOrig < distToTopRight) { // bottom left triangle is closer
			a = bottomLeftVertex;
			b = bottomRightVertex;
			c = topLeftVertex;
		} else { // top right triangle is closer
			
			a = bottomRightVertex;
			b = topRightVertex;
			c = topLeftVertex;
		}
		float um, vm, wm;
		um = a.y;
		vm = b.y;
		wm = c.y;
		a.y = 0.0f;
		b.y = 0.0f;
		c.y = 0.0f;
		float u, v, w;
		Barycentric (p, a, b, c, out u, out v, out w);
		float height = (u * um + v * vm + w * wm);
		//Vector3 posnh = new Vector3 (position.x, height, position.z);
		return height;
	}
		
	void Barycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out float u, out float v, out float w)
	{
		Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
		float d00 = Vector3.Dot(v0, v0);
		float d01 = Vector3.Dot(v0, v1);
		float d11 = Vector3.Dot(v1, v1);
		float d20 = Vector3.Dot(v2, v0);
		float d21 = Vector3.Dot(v2, v1);
		float denom = d00 * d11 - d01 * d01;
		v = (d11 * d20 - d01 * d21) / denom;
		w = (d00 * d21 - d01 * d20) / denom;
		u = 1.0f - v - w;
	}
	
    [SerializeField]
    float size;
    [SerializeField]
    float maxHeight;
    [SerializeField]
    int heightmapResolution;

    float[,] _heightMap;
    [SerializeField]
    float[,] heightMap
    {
        get
        {
            if (_heightMap == null)
            {
                var heightmapFile = Resources.Load<TextAsset>("Maps/" + name + "-hmap");
                var heightMapData = heightmapFile.bytes;
                int heightmapResolution = Mathf.RoundToInt(Mathf.Sqrt(heightMapData.Length / sizeof(float)));
                _heightMap = new float[heightmapResolution, heightmapResolution];
                Buffer.BlockCopy(heightMapData, 0, heightMap, 0, heightMapData.Length);
                if (_heightMap == null || _heightMap.GetLength(0) == 0)
                    Debug.LogError("Loading heightmap for map "+name+" failed");
            }
            return _heightMap;
        }
    }
    Texture2D splatMap;

    public Texture2D GetBrush()
    {
        int brushSize = 64;
        Color[] brushTexture = new Color[brushSize * brushSize];
        for (int i = 0; i < brushSize; i++)
        {
            for(int j = 0; j < brushSize; j++)
            {
                float intensity = Mathf.Clamp01(Vector2.Distance(new Vector2(i, j), new Vector2(brushSize / 2, brushSize / 2)) / 30.0f);
                brushTexture[i + j * brushSize] = new Color(0.0f, 1.0f, 1.0f, 1.0f-intensity);
            }
        }
        Texture2D bt = new Texture2D(brushSize, brushSize);
        bt.SetPixels(brushTexture);
        bt.Apply();
        return bt;
    }

    
    public enum TerrainBrushMode
    {
        ModifyTerrain,
        ModifySplat
    }

    public byte[] GetHeightmapBytes()
    {
        if(heightMap == null)
        {
            Debug.LogError("GetHeightmapBytes when no heightmap!");
            return null;
        }
        int numBytes = heightMap.GetLength(0) * heightMap.GetLength(1) * sizeof(float);
        byte[] bytes = new byte[numBytes];
        Buffer.BlockCopy(heightMap, 0, bytes, 0, numBytes);
        Debug.Log(bytes.Length);
        return bytes;
    }

    public byte[] GetSplatmapBytes()
    {
        if(splatMap == null)
        {
            Debug.LogError("GetSplatmapBytes when no splatmap!");
        }
        return splatMap.EncodeToPNG();
    }

    public void ApplyBrush(Vector3 position, TerrainBrushMode mode)
    {
        float brushSize = 50.0f;

        var brush = GetBrush();

        int res = 1024;
        switch(mode)
        {
            case TerrainBrushMode.ModifyTerrain:
                res = heightmapResolution;
                break;
            case TerrainBrushMode.ModifySplat:
                res = splatMap.width;
                break;
        }

        float unitsPerPixel = this.size / res;
        int ucx = Mathf.RoundToInt(position.x / unitsPerPixel);
        int ucy = Mathf.RoundToInt(position.z / unitsPerPixel);
        int x = Mathf.Clamp(ucx, 0, res-1);
        int y = Mathf.Clamp(ucy, 0, res-1);

        int offset = Mathf.CeilToInt(brushSize / unitsPerPixel);
        ucx -= offset;
        ucy -= offset;

        int startx = Mathf.Max(0, x - offset);
        int starty = Mathf.Max(0, y - offset);
        int endx = Mathf.Min(x + offset, res - 1);
        int endy = Mathf.Min(y + offset, res - 1);

        for(int i = startx; i < endx; i ++)
        {
            for (int j = starty; j < endy; j++)
            {
                float brushX = ((float)i - ucx) / ((float)offset * 2.0f);
                float brushY = ((float)j - ucy) / ((float)offset * 2.0f);
                float brushIntensity = brush.GetPixelBilinear(brushX, brushY).a;
                switch (mode)
                {
                    case TerrainBrushMode.ModifyTerrain:
                        heightMap[j, i] += brushIntensity * Time.deltaTime * 0.2f;
                        break;
                    case TerrainBrushMode.ModifySplat:
                        var pixel = splatMap.GetPixel(i, j);
                        pixel.b += brushIntensity * Time.deltaTime;
                        pixel.r -= brushIntensity * Time.deltaTime;
                        pixel.b = Mathf.Clamp01(pixel.b);
                        pixel.r = Mathf.Clamp01(pixel.r);
                        splatMap.SetPixel(i, j, pixel);
                        break;
                }
            }
        }

        if(mode == TerrainBrushMode.ModifySplat)
        {
            splatMap.Apply();
        }
        UpdateMeshFlat();
    }

    public void UpdateMesh()
    {
        var omf = GetComponent<MeshFilter>();

        Mesh mesh = omf.mesh;
        int width = heightmapResolution;
        int height = heightmapResolution;

        Vector3[] vertices;
        int[] triangles;
        Vector2[] uv;

        vertices = omf.mesh.vertices;
        triangles = omf.mesh.triangles;
        uv = omf.mesh.uv;

        float scale = size / heightmapResolution;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float terrainHeight = heightMap[i, j] * maxHeight;
                vertices[j + i * height] = new Vector3(j * scale, terrainHeight, i * scale);
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    public void UpdateMeshFlat()
    {
        var omf = GetComponent<MeshFilter>();

        Mesh mesh = omf.mesh;
        int width = heightmapResolution;
        int height = heightmapResolution;

        Vector3[] vertices;
        int[] triangles;
        Vector2[] uv;

        vertices = omf.mesh.vertices;
        triangles = omf.mesh.triangles;
        uv = omf.mesh.uv;

        float scale = size / heightmapResolution;
        int indices = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float terrainHeight = 0.0f;
                if (i != (height - 1) && j != (width - 1))
                {
                    terrainHeight = heightMap[i,j] * maxHeight;
                    vertices[indices] = new Vector3(j * scale, terrainHeight, i * scale);
                    indices++;
                    terrainHeight = heightMap[i+1, j] * maxHeight;
                    vertices[indices] = new Vector3(j * scale, terrainHeight, (i + 1) * scale);
                    indices++;
                    terrainHeight = heightMap[i+1, j+1] * maxHeight;
                    vertices[indices] = new Vector3((j + 1) * scale, terrainHeight, (i + 1) * scale);
                    indices++;
                    terrainHeight = heightMap[i, j] * maxHeight;
                    vertices[indices] = new Vector3(j * scale, terrainHeight, i * scale);
                    indices++;
                    terrainHeight = heightMap[i+1, j+1] * maxHeight;
                    vertices[indices] = new Vector3((j + 1) * scale, terrainHeight, (i + 1) * scale);
                    indices++;
                    terrainHeight = heightMap[i, j+1] * maxHeight;
                    vertices[indices] = new Vector3((j + 1) * scale, terrainHeight, +i * scale);
                    indices++;
                }
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    public void GenerateMesh()
    {
        Mesh mesh;
        int width = heightmapResolution;
        int height = heightmapResolution;

        Vector3[] vertices;
        int[] triangles;
        Vector2[] uv;
        mesh = new Mesh();
        vertices = new Vector3[width * height];
        triangles = new int[(width - 1) * (height - 1) * 6];
        uv = new Vector2[width * height];
        int indices = 0;

        float scale = size / heightmapResolution;

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float terrainHeight = heightMap[i, j] * maxHeight;
                vertices[j + i * height] = new Vector3(j * scale, terrainHeight, i * scale);

                uv[j + i * height] = new Vector2((float)j, (float)i);

                if (i != (height - 1) && j != (width - 1))
                {
                    triangles[indices] = j + i * height;
                    triangles[indices + 2] = (j + 1) + (i + 1) * height;
                    triangles[indices + 1] = j + (i + 1) * height;

                    triangles[indices + 3] = j + i * height;
                    triangles[indices + 5] = (j + 1) + i * height;
                    triangles[indices + 4] = (j + 1) + (i + 1) * height;

                    indices += 6;
                }
            }
        }

#if DEBUG
        mesh.MarkDynamic();
#endif

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        meshFilter.mesh = mesh;
        mesh.RecalculateNormals();

        TerrainMesh = mesh;

        if(gameObject.GetComponent<MeshCollider>() == null)
            gameObject.AddComponent<MeshCollider>();
    }

    public void GenerateFlatMesh()
    {
        Mesh mesh;
        int width = heightmapResolution;
        int height = heightmapResolution;

        Vector3[] vertices;
        int[] triangles;
        Vector2[] uv;
        mesh = new Mesh();
        vertices = new Vector3[(width - 1) * (height - 1) * 6];
        triangles = new int[(width - 1) * (height - 1) * 6];
        uv = new Vector2[(width - 1) * (height - 1) * 6];
        int indices = 0;

        float scale = size / heightmapResolution;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if(meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //float terrainHeight = heightMap[i, j] * maxHeight;
                float terrainHeight = 0.0f;
                if (i != (height - 1) && j != (width - 1))
                {
                    // 0,0 0,1 1,1 0,0 1,1 1,0
                    terrainHeight = heightMap[i,j]*maxHeight;
                    vertices[indices] = new Vector3(j * scale, terrainHeight, i * scale);
                    triangles[indices] = indices;
                    uv[indices] = new Vector2((float)j, (float)i);
                    indices++;
                    terrainHeight = heightMap[i+1, j] * maxHeight;
                    vertices[indices] = new Vector3(j * scale, terrainHeight, (i+1) * scale);
                    uv[indices] = new Vector2((float)j, (float)i+1);
                    triangles[indices] = indices;
                    indices++;
                    terrainHeight = heightMap[i+1, j+1] * maxHeight;
                    vertices[indices] = new Vector3((j+1) * scale, terrainHeight, (i+1) * scale);
                    uv[indices] = new Vector2((float)j+1, (float)i+1);
                    triangles[indices] = indices;
                    indices++;
                    terrainHeight = heightMap[i, j] * maxHeight;
                    vertices[indices] = new Vector3(j * scale, terrainHeight, i * scale);
                    uv[indices] = new Vector2((float)j, (float)i);
                    triangles[indices] = indices;
                    indices++;
                    terrainHeight = heightMap[i+1, j+1] * maxHeight;
                    vertices[indices] = new Vector3((j+1) * scale, terrainHeight, (i+1) * scale);
                    uv[indices] = new Vector2((float)j+1, (float)i+1);
                    triangles[indices] = indices;
                    indices++;
                    terrainHeight = heightMap[i, j+1] * maxHeight;
                    vertices[indices] = new Vector3((j+1) * scale, terrainHeight, + i * scale);
                    uv[indices] = new Vector2((float)j+1, (float)i);
                    triangles[indices] = indices;
                    indices++;
                }
            }
        }

#if DEBUG
        mesh.MarkDynamic();
#endif

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        meshFilter.mesh = mesh;
        mesh.RecalculateNormals();
        TerrainMesh = mesh;

        if (gameObject.GetComponent<MeshCollider>() == null)
            gameObject.AddComponent<MeshCollider>();
    }

    void Create(float size, int heightmapResolution)
    {
        float maxHeight = 100.0f;

        this.size = size;
        this.heightmapResolution = heightmapResolution;
        this.maxHeight = maxHeight;

        var splat = new Texture2D(256, 256);
        this.splatMap = splat;
        for(int i = 0; i < 256; i++)
        {
            for (int j = 0; j < 256; j++)
            {
                splat.SetPixel(i, j, Color.red);
            }
        }
        splat.Apply();
        

        this._heightMap = new float[heightmapResolution, heightmapResolution];
        /*for(int i = heightmapResolution-1; i > heightmapResolution-50; i--)
        {
            for (int j = heightmapResolution - 1; j > heightmapResolution - 50; j--)
            {
                newTerrain.heightMap[i, j] = 0.1f;
            }
        }*/

        this.GenerateFlatMesh();

        var renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = TerrainMaterial;
        renderer.sharedMaterial.mainTexture.wrapMode = TextureWrapMode.Repeat;
        renderer.sharedMaterial.SetTexture("_Splat", splat);
        renderer.sharedMaterial.SetFloat("_heightmapSize", (float)heightmapResolution);

        //return newTerrain;
    }

    bool Load(float size, byte[] heightMapData, byte[] splatMapData)
    {
        if (heightMapData == null)
        {
            Debug.LogError("Heightmap can't be null!");
            return false;
        }
        if (splatMapData == null)
        {
            Debug.LogError("Heightmap can't be null!");
            return false;
        }

        int heightmapResolution = Mathf.RoundToInt(Mathf.Sqrt(heightMapData.Length / sizeof(float)));
        float[,] heightMap = new float[heightmapResolution, heightmapResolution];
        Buffer.BlockCopy(heightMapData, 0, heightMap, 0, heightMapData.Length);

        float maxHeight = 100.0f;

        this.size = size;
        this.heightmapResolution = heightmapResolution;
        this.maxHeight = maxHeight;

        splatMap = new Texture2D(2, 2);
        splatMap.LoadImage(splatMapData);

        this._heightMap = heightMap;

        GenerateFlatMesh();

        var renderer = GetComponent<MeshRenderer>();
        if(renderer == null)
            renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = TerrainMaterial;
        renderer.sharedMaterial.mainTexture.wrapMode = TextureWrapMode.Repeat;
        renderer.sharedMaterial.SetTexture("_Splat", splatMap);
        renderer.sharedMaterial.SetFloat("_heightmapSize", (float)heightmapResolution);

        return true;
    }
}
