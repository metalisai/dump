// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public abstract class BaseAI : MonoBehaviour 
{
    [HideInInspector]
    public NavMeshAgent NavAgent;
    [HideInInspector]
    public AIJobQueue JobQueue;
    [HideInInspector]
    public Animator Animator;

    protected AIJob _curJob;

    void Awake()
    {
        NavAgent = GetComponent<NavMeshAgent>();
        JobQueue = new AIJobQueue(this);
        Animator = GetComponent<Animator>();
    }

    protected void Update()
    {
        if(_curJob == null)
        {
            _curJob = JobQueue.DeQueue();
            //Debug.Log("dqresult" + _curJob);
            if(_curJob != null)
            {
                Debug.Log("Job started " + _curJob);
                _curJob.StartJob();
            }
        }
        
        if(_curJob != null)
        {
            _curJob.JobTick();

            bool success = false;
            if(_curJob.IsJobDone(ref success))
            {
                Debug.Log("Job done " + _curJob);
                _curJob.OnFinish(success);
                _curJob = null;
            }
        }
    }

    public void CancelCurrentJob()
    {
        _curJob = null;
    }
}
