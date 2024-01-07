// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;

public class RiskHeatMap : MonoBehaviour {

    float[,] HeatMap;

    public int MapSize = 500;
    [HideInInspector]
    public float CooldownPerTick = 0.0001f;
    public float Heat = 0.0005f;
    public float HeatRange = 50f;

    float _size;
    Vector3 _center;

    private MeshRenderer _renderer;
    private Vector3 _lowestEdge;

    struct PixelCoord
    {
        public PixelCoord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int x;
        public int y;
    }

    public void ToggleShow()
    {
        _renderer.enabled = !_renderer.enabled;
    }

    void Awake()
    {
        CooldownPerTick = Heat * 0.1f;
        _size = transform.localScale.x * 10f; // 10f=default plane size in unity
        _center = transform.position;
        _lowestEdge = _center - new Vector3(_size / 2f, 0f, _size / 2f);
    }

	void Start () {
        Heat += CooldownPerTick; // otherwise it wont increase if cooldown>heat

        HeatMap = new float[MapSize, MapSize];
        GameTime.OnTick += Tick;
        GameTime.OnReal100ms += RealTick;

        var texture = new Texture2D(MapSize, MapSize, TextureFormat.ARGB32, false);
        //texture.filterMode = FilterMode.Point;
        GetComponent<Renderer>().material.mainTexture = texture;

        _renderer = GetComponent<MeshRenderer>();
    }

    PixelCoord WorldToPixelCoordinates(Vector3 worldCoord)
    {
        Vector3 offsetFromLowest = worldCoord - _lowestEdge;
        float x = 1f-offsetFromLowest.x / _size;
        float z = 1f-offsetFromLowest.z / _size;

        return new PixelCoord(Mathf.Clamp((int)(x*MapSize),0,MapSize-1),Mathf.Clamp((int)(z*MapSize),0,MapSize-1));
    }

    Vector3 PixelToWoorldCoordinates(PixelCoord pixelCoord)
    {
        float x = Mathf.Clamp01(1f-( (float)(pixelCoord.x) / MapSize));
        float z = Mathf.Clamp01(1f-((float)(pixelCoord.y) / MapSize));                 

        return _lowestEdge + new Vector3(x*_size,transform.position.y, z*_size);
    }

    float UnitsPerPixel()
    {
        float size = transform.localScale.x * 10f;  // 10f=default plane size in unity
        return (float)MapSize/size;
    }

    // TODO: use a list of positions instead of external calls
    public void incPixel(Vector3 woorlPos)
    {
        Profiler.BeginSample("HeatmapGenHeat");
        float range = HeatRange;
        float sqRange = HeatRange * HeatRange;
        float halfRangeSq = (HeatRange / 2f) * (HeatRange / 2f);
        int pixelRange = Mathf.CeilToInt(range * UnitsPerPixel());
        if (pixelRange % 2 == 1) ++pixelRange; // don't wanna deal with uneven divide

        PixelCoord coord = WorldToPixelCoordinates(woorlPos);

        PixelCoord lowest = new PixelCoord(coord.x - pixelRange / 2, coord.y - pixelRange / 2);

        float trany = transform.position.y;

        for (int i = 0; i < pixelRange; i++)
        {
            for (int j = 0; j < pixelRange; j++)
            {
                int x = lowest.x + i;
                int y = lowest.y + j;

                if (x < 0 || y < 0 || x >= MapSize || y >= MapSize)
                    continue;

                Vector3 center = woorlPos;
                center.y = trany;

                //float distMult = Vector3.Distance(center, PixelToWoorldCoordinates(new PixelCoord(x, y)));
                //distMult = 1f - (distMult / (range / 2f));
                Vector3 to = PixelToWoorldCoordinates(new PixelCoord(x, y));
                float distMult = (center.x-to.x)*(center.x - to.x)+(center.z - to.z)*(center.z - to.z); // sqaure distance
                distMult = 1f - (distMult / (halfRangeSq));
                distMult = Mathf.Clamp01(distMult);

                float valToAdd = Heat * distMult;
                if (HeatMap[x, y] + valToAdd <= 1f)
                    HeatMap[x, y] += valToAdd;
            }
        }
        Profiler.EndSample();
    }

    public float GetHeat(Vector3 wpos)
    {
        var coord = WorldToPixelCoordinates(wpos);
        return HeatMap[coord.x, coord.y];
    }

    public Texture2D ToTexture()
    {
        Profiler.BeginSample("HeatmapTickToTexture");
        var texture = GetComponent<Renderer>().material.mainTexture as Texture2D;

        for (int i = 0; i < HeatMap.GetLength(0); i++)
        {
            for (int j = 0; j < HeatMap.GetLength(1); j++)
            {
                float H0, H1, H;
                float S0, S1, S;
                float V0, V1, V;

                Color.RGBToHSV(Color.green, out H0, out S0, out V0);
                Color.RGBToHSV(Color.red, out H1, out S1, out V1);

                H = Mathf.Lerp(H0, H1, HeatMap[i, j]);
                S = Mathf.Lerp(S0, S1, HeatMap[i, j]);
                V = Mathf.Lerp(V0, V1, HeatMap[i, j]);

                // TODO: Use Setpixels
                texture.SetPixel(i,j,Color.HSVToRGB(H,S,V));
                //texture.SetPixels()
            }
        }
        texture.Apply();

        GetComponent<Renderer>().material.mainTexture = texture;
        Profiler.EndSample();
        return texture;
    }

    void RealTick()
    {
        Profiler.BeginSample("HeatmapVisualUpdate");
        ToTexture();
        Profiler.EndSample();
    }
	
	void Tick () {
        Profiler.BeginSample("HeatmapTick");

        for (int i = 0; i < HeatMap.GetLength(0); i++)
        {
            for (int j = 0; j < HeatMap.GetLength(1); j++)
            {
                if(HeatMap[i,j] - CooldownPerTick >= 0f)
                    HeatMap[i, j] -= CooldownPerTick;
            }
        }

        Profiler.EndSample();
    }

    void OnDestroy()
    {
        GameTime.OnTick -= Tick;
        GameTime.OnReal100ms -= RealTick;
    }
}
