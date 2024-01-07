using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class ProjectileManager
{
    enum ProjectileType
    {
        Bullet,
        Bombshell,

        Count
    }

    struct FProjectile
    {
        public GameObject go;
        public Vector3 end;
        public float sign;
        public Vector3 velocity;
        public Action onComplete;
        public bool hasGravity;
    }

    class ProjectilePool
    {
        public List<GameObject> instances;
        public List<int> freeInstances;
    }

    const int MAX_BULLETS = 100;
    StructList<FProjectile> Projectiles = new StructList<FProjectile>();
    GameObject bulletReference;
    GameObject bombReference;

    ProjectilePool[] _objectPools;

    public ProjectileManager(GameObject bullet, GameObject bomb)
    {
        bulletReference = bullet;
        bombReference = bomb;

        _objectPools = new ProjectilePool[(int)ProjectileType.Count];
        for (int i = 0; i < _objectPools.Length; i++)
        {
            _objectPools[i] = new ProjectilePool()
            {
                freeInstances = new List<int>(),
                instances = new List<GameObject>()
            };
            for (int j = 0; j < 20; j++)
            {
                _objectPools[i].freeInstances.Add(_objectPools[i].instances.Count);
                _objectPools[i].instances.Add(CreateInstanceFor((ProjectileType)i));
            }
        }
    }

    GameObject CreateInstanceFor(ProjectileType type)
    {
        GameObject ret = null;
        switch(type)
        {
            case ProjectileType.Bullet:
                {
                    ret = GameObject.Instantiate(bulletReference);
                }break;
            case ProjectileType.Bombshell:
                {
                    ret = GameObject.Instantiate(bombReference);
                }
                break;
        }
        ret.SetActive(false);
        Game.Instance.RegisterDynamicObject(ret, true);
        return ret;
    }

    void GrowPool(ProjectileType type)
    {
        for(int i = 0; i < 10; i++)
        {
            _objectPools[(int)type].freeInstances.Add(_objectPools[(int)type].instances.Count);
            _objectPools[(int)type].instances.Add(CreateInstanceFor(type));
        }
    }

    GameObject GetInstanceFor(ProjectileType type)
    {
        int index = (int)type;
        ProjectilePool pool = _objectPools[index];
        if (pool.freeInstances.Count == 0)
            GrowPool(type);
        Assert.IsTrue(pool.freeInstances.Count != 0);
        int freeIndex = pool.freeInstances[pool.freeInstances.Count-1]; // take from end because of how arrays work
        var ret = pool.instances[freeIndex];
        pool.freeInstances.RemoveAt(pool.freeInstances.Count - 1);
        ret.SetActive(true);
        return ret;
    }

    // TODO: if Game destroys all objets, the pools must be reset
    void ReturnInstance(GameObject instance)
    {
#if DEBUG
        bool found = false;
#endif
        for(int i = 0; i < _objectPools.Length; i++)
        {
            int instanceCount = _objectPools[i].instances.Count;
            for (int j = 0; j < instanceCount; j++)
            {
                if(_objectPools[i].instances[j] == instance)
                {
                    _objectPools[i].freeInstances.Add(j);
                    instance.SetActive(false);
#if DEBUG
                    found = true;
#endif
                }
            }
        }
#if DEBUG
        Assert.IsTrue(found, "An unknown instance was returned to pool");
#endif
    }

    public void FireBullet(Vector3 start, Vector3 end, float speed, Action onComplete)
    {
        Vector3 dir = (end - start).normalized;
        var bullet = GetInstanceFor(ProjectileType.Bullet);
        bullet.transform.rotation = Quaternion.LookRotation(dir);
        bullet.transform.position = start;
        var abullet = new FProjectile()
        {
            go = bullet,
            end = end,
            sign = Vector3.Dot(start, end),
            velocity = speed * dir,
            onComplete = onComplete,
            hasGravity = false
        };
        Projectiles.Add(ref abullet);
        bullet.SetActive(true);
    }

    public void FireArtilleryProjectile(Vector3 start, Vector3 end, float angleDeg, Action onComplete)
    {
        float dist = (start - end).magnitude;
        Vector2 targetOffset = new Vector2(dist, end.y - start.y);
        float forceMagnitude = CalculateForceMagnitude(targetOffset, angleDeg);
        float flyTime = CalculateTime(forceMagnitude, angleDeg);
        Vector2 force = CalculateForce(flyTime, targetOffset);
        Vector3 dir = end - start;
        dir.y = 0.0f;
        dir.Normalize();
        Vector3 force3D = force.x * dir+Vector3.up*force.y;

        // create the bullet
        var bullet = GetInstanceFor(ProjectileType.Bombshell);
        bullet.transform.rotation = Quaternion.LookRotation(dir);
        bullet.transform.position = start;
        var abullet = new FProjectile()
        {
            go = bullet,
            end = end,
            sign = Vector3.Dot(start, end),
            velocity = force3D,
            onComplete = onComplete,
            hasGravity = true
        };
        Projectiles.Add(ref abullet);
        bullet.SetActive(true);
    }

    Vector2 CalculateForce(float time, Vector2 target)
    {
        return (target - 0.5f * new Vector2(0.0f, -9.8f) * time * time) / time;
    }

    float CalculateTime(float forceMagnitude, float angle)
    {
        float cosa = Mathf.Cos(Mathf.Deg2Rad * angle);
        return 50.0f / (forceMagnitude * cosa);
    }

    float CalculateForceMagnitude(Vector2 s, float angle)
    {
        float cosa = Mathf.Cos(angle * Mathf.Deg2Rad);
        float f2 = -(9.8f * s.x * s.x) / (2 * s.y * cosa * cosa - s.x * Mathf.Sin(angle * Mathf.Deg2Rad * 2.0f));
        return Mathf.Sqrt(f2);
    }

    public void Update()
    {
        List<GameObject> remove = new List<GameObject>();
        var barray = Projectiles.Array; // this is dangerous if you add things while using the array
        for (int i = 0; i < Projectiles.Count; i++)
        {
            if(barray[i].hasGravity)
            {
                barray[i].velocity += new Vector3(0.0f, -9.8f * Time.deltaTime, 0.0f);
            }
            Projectiles.Array[i].go.transform.position += barray[i].velocity * Time.deltaTime;
            if (Vector3.Dot(barray[i].end - barray[i].go.transform.position, barray[i].velocity) <= 0.0f)
            {
                barray[i].onComplete();
                ReturnInstance(barray[i].go);
                remove.Add(barray[i].go);
            }
        }
        Projectiles.RemoveAll(x => remove.Contains(x.go));
    }
}
