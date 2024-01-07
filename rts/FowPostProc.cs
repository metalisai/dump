using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FowPostProc : ManagedMonoBehaviour {

    public Material posProcMat;
    ComputeBuffer buf;
    const int MAX_AGENTS = 200;
    int currentSize = MAX_AGENTS;

    // Use this for initialization
    void Awake () {
        Vector4[] agents = new Vector4[30];
        posProcMat.SetVectorArray("_fowAgents", agents);
        buf = new ComputeBuffer(200, 12); // 12 = sizeof(Vector3)
    }

    void OnDestroy()
    {
        buf.Release();
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        List<Vector3> agents = Game.Instance.FogOfWar.GetSightAgents();

        if(agents.Count >= currentSize) // resize buffer if neccessary
        {
            buf.Release();
            currentSize *= 2;
            buf = new ComputeBuffer(currentSize, 12);
        }

        buf.SetData(agents.ToArray());
        posProcMat.SetBuffer("_fowAgentBuf", buf);

        posProcMat.SetInt("_fowAgentCount", agents.Count);
        posProcMat.SetMatrix("_cam2World", Camera.main.cameraToWorldMatrix);

        Graphics.Blit(src, dest, posProcMat);
    }
}
