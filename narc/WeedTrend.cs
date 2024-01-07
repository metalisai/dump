// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;

public class WeedTrend : MonoBehaviour {

    LineRenderer lrend;

    float[] TrendHistory;

    int steps = 100;
    public float basePrice = 25f;

    public AnimationCurve WeedSalesVsPrice;

    void Start () {

        TrendHistory = new float[steps];

        lrend = GetComponent<LineRenderer>();
        lrend.SetVertexCount(steps);

        //GameTime.OnTick += Step;
        GameTime.OnTick += Step;

        rnd();
	}

    public float GetSalesMultiplier(float price)
    {
        float OptimalPrice = Current;
        float precOptimal = price/OptimalPrice;
        return WeedSalesVsPrice.Evaluate(precOptimal);
    }


    float lt = 0f; // long trend
    float st = 0f; // short trend
    float ltAcc = 0f; // long trend accumulator
    float stAcc = 0f; // short trend accumulator
    float genNext()
    {
        float ltChance = 0.2f; // chance that long trend changes this step
        float stChance = 0.8f; // chance that short trend changes this step


        var ltR = Random.Range(0f, 1f);
        var stR = Random.Range(0f, 1f);

        var rndR = Random.Range(-0.01f*basePrice, 0.01f * basePrice);

        float maxLtDev = 0.7f*basePrice; // maximum deviation from base value via long trend
        float maxStDev = 0.05f*basePrice; // maximum deviation from base value via short trend

        float maxLtChangePerStep = 0.075f*basePrice; // the largest amount long trend can change per step 
        float maxStChangePerStep = 0.025f*basePrice; // the largest amount short trend can change per step

        // idk what this does or is, good luck

        if (ltR <= ltChance || Mathf.Abs(ltAcc) > maxLtDev)
        {
            float max = Mathf.Clamp((ltAcc / maxLtDev) * maxLtChangePerStep, -maxLtChangePerStep, maxLtChangePerStep);
            lt = Random.Range(-maxLtChangePerStep - (max>0f?max:0f), maxLtChangePerStep - (max<0f?max:0f));
        }

        if (stR <= stChance || Mathf.Abs(stAcc) > maxStDev)
        {
            float max = Mathf.Clamp((stAcc / maxStDev) * maxStChangePerStep, -maxStChangePerStep, maxStChangePerStep);
            st = Random.Range(-maxStChangePerStep - (max>0f?max:0f), maxStChangePerStep - (max<0f?max:0f));
        }

        ltAcc += lt;
        stAcc += st;


        return basePrice + ltAcc + stAcc + rndR;
    }

    public float Current
    {
        get
        {
            return TrendHistory[TrendHistory.Length - 1];
        }
    }
	
	public void rnd ()
    {
        for(int i = 0; i < steps; i++)
        {
            TrendHistory[i] = genNext();
        }

        UpdateGraph();
    }

    public void Step()
    {
        for(int i = 0; i < TrendHistory.Length-1; i++)
        {
            TrendHistory[i] = TrendHistory[i + 1];
        }
        TrendHistory[TrendHistory.Length - 1] = genNext();

        UpdateGraph();
    }

    void UpdateGraph()
    {
        if (lrend == null)
        {
            Debug.Log("No line renderer!");
            return;
        }

        Vector3 center = transform.position;
        float size = transform.localScale.x * 10f;  // 10f=default plane size in unity
        Vector3 lowestEdge = center - new Vector3(size / 2f, size / 2f, 0.1f);

        Vector3 highestEdge = center + new Vector3(size / 2f, size / 2f, 0.1f);
        float stepSize = (highestEdge.x - lowestEdge.x) / steps;

        for (int i = 0; i < TrendHistory.Length; i++)
        {
            float res = (TrendHistory[i]) / 50f;

            lrend.SetPosition(i, lowestEdge + new Vector3(i * stepSize, (highestEdge.y - lowestEdge.y) * res, 0f));
        }
    }

    void OnDestroy()
    {
        GameTime.OnHour -= Step;
    }
}
