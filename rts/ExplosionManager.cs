using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class ExplosionManager
{
    public static void ExplosionAt(Vector3 position)
    {
        var go = GameObject.Instantiate(Game.Instance.Explosion);
        foreach(var ps in go.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }
        go.transform.position = position;
        GameObject.Destroy(go, 10.0f);
    }

    public static void Update()
    {

    }
}
