// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Wrapper around System.Collections.Queue<AIJob/>
/// </summary>
/// 
public class AIJobQueue 
{
    public BaseAI AI;
    LinkedList<AIJob> _jq = new LinkedList<AIJob>();

    public AIJobQueue(BaseAI baseAI)
    {
        AI = baseAI;
    }

    public int JobCount
    {
        get
        {
            return _jq.Count;
        }
    }

	public void EnQueue(AIJob job)
    {
        //Debug.Log("Queued job " + job);
        job.JobQueue = this;
        _jq.AddLast(job);
    }

    public void Push(AIJob job)
    {
        //Debug.Log("Queued job " + job);
        job.JobQueue = this;
        _jq.AddFirst(job);
    }

    public AIJob DeQueue()
    {
        if (_jq.Count > 0)
        {
            var j = _jq.First.Value;
            _jq.RemoveFirst();
            return j;
        }
        else
            return null;
    }

    public AIJob Peek()
    {
        return _jq.Count > 0 ? _jq.First.Value : null;
    }
}
