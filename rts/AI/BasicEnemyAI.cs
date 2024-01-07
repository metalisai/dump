using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Profiling;

public class BasicEnemyAI : BaseAI{

    AudioSource audioSource;
    Destroyable currentTarget;
    public float range = 50.0f;
    float reloadTimer = 0.0f;
    public float reloadTime = 2.0f;

    bool stop = false;

	// Use this for initialization
	void Start () {
        audioSource = GetComponent < AudioSource >();
        GameTime.SlowTick += SlowTick;
	}
    
    new void OnDestroy()
    {
        GameTime.SlowTick -= SlowTick;
        base.OnDestroy();
    }

    void SlowTick(float dt)
    {
        Profiler.BeginSample("BasicEnemy FindTarget");
        if (currentTarget == null)
        {
            // TODO: SLOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOW
            Destroyable searchR = Game.Instance.GetDestroyableAlliesInRange(transform.position, range)
                .OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault();
            if (searchR != null)
                currentTarget = searchR;
        }
        Profiler.EndSample();
    }

    bool IsTargetAttackable()
    {
        if (currentTarget == null)
            return false;
        bool ret = Vector3.Distance(currentTarget.transform.position, transform.position) <= range
            && currentTarget.gameObject.activeInHierarchy;
        return ret;
    }
	
	// Update is called once per frame
	public override void ManagedUpdate ()
    {
        reloadTimer += Time.deltaTime;

        if (!IsTargetAttackable())
            currentTarget = null;

        if(currentTarget != null)
        {
            if(IsTargetAttackable() && reloadTimer >= reloadTime)
            {
                ShootCurrentTarget();
            }
            stop = true;
        }
        else
        {
            stop = false;
        }

        if(!stop && !HasJob())
        {
            var nexus = Game.Instance.Nexus;
            if (nexus != null)
            {
                Jobs.QueueMoveJob(this, Game.Instance.Nexus.transform.position);
            }
        }
        else if(stop && HasJob())
        {
            RemoveAllWork();
        }

        base.ManagedUpdate();
    }

    void OnHit()
    {
        if (currentTarget != null)
            currentTarget.Hit(100);
    }

    void ShootCurrentTarget()
    {
        reloadTimer = 0.0f;
        audioSource.PlayOneShot(audioSource.clip);
        Game.BulletManager.FireBullet(transform.position, currentTarget.transform.position, 100.0f, OnHit);
        //Debug.Log("Turret shot");
    }
}
