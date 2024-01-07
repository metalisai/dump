// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public class DaynightCycle : MonoBehaviour 
{
    public Light Sunlight;
    public AnimationCurve LightIntensity;
    public AnimationCurve SkyBlend;

    public AnimationCurve SunDirectionX;
    public AnimationCurve SunDirectionY;
    public AnimationCurve SunDirectionZ;

    public Color SkyBox1Color = Color.white;
    public Color SkyBox2Color = Color.white;

    public Color AmbientColor1 = Color.white;
    public Color AmbientColor2 = Color.white;

    [Range(0f,2f)]
    public float SunIntensityMultiplier = 1f;

    void Start()
    {
        GameTime.OnTick += Tick;
    }

    void Tick()
    {
        float dayProgress = GameTime.DayProgress;

        float sbProgress = SkyBlend.Evaluate(dayProgress);
        Sunlight.intensity = SunIntensityMultiplier*LightIntensity.Evaluate(dayProgress);
        Sunlight.color = Color.Lerp(SkyBox1Color, SkyBox2Color, sbProgress);
        RenderSettings.skybox.SetFloat("_Blend", sbProgress);
        Sunlight.transform.rotation = Quaternion.Euler(SunDirectionX.Evaluate(dayProgress)*360f, SunDirectionY.Evaluate(dayProgress) * 360f, SunDirectionZ.Evaluate(dayProgress) * 360f);

        RenderSettings.ambientSkyColor = Color.Lerp(AmbientColor1, AmbientColor2, sbProgress);
    }

    void OnDestroy()
    {
        GameTime.OnTick -= Tick;
    }
}
