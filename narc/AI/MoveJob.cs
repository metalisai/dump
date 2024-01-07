// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System;

public class MoveJob : AIJob
{
    Vector3 _target;

    public MoveJob(Vector3 target)
    {
        _target = target;
    }

    public override bool IsJobDone(ref bool success)
    {
        var navAgent = AI.NavAgent;
        bool doneTrying = !navAgent.pathPending
            /*&& navAgent.remainingDistance <= navAgent.stoppingDistance*/
            && (!navAgent.hasPath || Math.Abs(navAgent.velocity.sqrMagnitude) < 0.01f && navAgent.remainingDistance <= navAgent.stoppingDistance);

        Debug.DrawLine(AI.transform.position, AI.transform.position+Vector3.up);
        Debug.DrawLine(_target, _target + Vector3.up);

        if (doneTrying)
        {
            if(navAgent.hasPath)
            {
                navAgent.ResetPath();
            }
            //Debug.Log("dist: "+ Vector3.Distance(AI.transform.position, _target));
            success = Vector3.Distance(AI.transform.position, _target) < 4f;
            if (!success)
                Debug.LogWarning("MoveJob failed!");
            return true;
        }

        return false;
    }

    public override void JobTick()
    {
        AI.Animator.SetFloat("WalkSpeed", Mathf.Max(AI.NavAgent.desiredVelocity.magnitude/AI.NavAgent.speed,0.05f));
    }

    public override void StartJob()
    {
        AI.NavAgent.SetDestination(_target);
        AI.Animator.SetBool("Walk", true);
        //Debug.Log("walk set to true");
    }

    public override void OnFinish(bool success)
    {
        AI.Animator.SetBool("Walk", false);
        base.OnFinish(success);
        //Debug.Log("walk set to false");
    }
}
