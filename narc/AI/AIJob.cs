// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;

public abstract class AIJob
{
    public AIJobQueue JobQueue;
    protected BaseAI AI
    {
        get
        {
            return JobQueue.AI;
        }
    }

    protected List<AIJob> SubJobs = new List<AIJob>();

    public abstract void StartJob();
    public abstract void JobTick();
    public abstract bool IsJobDone(ref bool success);

    public virtual void OnFinish(bool success)
    {
        // dequeue all jobs that are dependant on this one if the job failed
        if (!success)
        {
            for (int i = 0; i < SubJobs.Count; i++)
            {
                if (JobQueue.Peek() == SubJobs[i])
                {
                    //Debug.Log("Removed job due to dependency failure! "+ SubJobs[i]);
                    JobQueue.DeQueue();
                    SubJobs.RemoveAt(i);
                    i = 0; // run the loop again
                }
            }
        }
    }

    public void AddDependency(AIJob dependency)
    {
        dependency.SubJobs.Add(this);
    }
}
