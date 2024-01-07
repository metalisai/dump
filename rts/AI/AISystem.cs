using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;

public abstract class AIWorkConstraint
{
    public BaseAI AI;
    public abstract bool IsConstraintFilled();
    public abstract bool AttemptFillConstraint();
}

public abstract class AIJob
{
    public AIJobQueue JobQueue;
    Queue<AIWorkConstraint> constraints = new Queue<AIWorkConstraint>();

    protected BaseAI AI
    {
        get
        {
            return JobQueue.AI;
        }
    }

    protected List<AIJob> SubJobs = new List<AIJob>();

    public bool CheckConstraints()
    {
        while (constraints.Count > 0)
        {
            var constr = constraints.Dequeue();
            constr.AI = AI;
            if (constr.IsConstraintFilled()) // already filled?
                continue;
            else if (constr.AttemptFillConstraint()) // attempt fix
                continue;
            else // no fix, can't do this job
            {
                Debug.Log("Unfixable job constraint");
                AI.CancelCurrentJob(true);
                return false;
            }
        }
        return true;
    }

    public abstract void StartJob();

    public void AddConstraint(AIWorkConstraint constraint)
    {
        constraints.Enqueue(constraint);
    }

    public abstract void JobTick();
    public virtual void MediumTick() { }
    public abstract bool IsJobDone(ref bool success);

    private static void RemoveDependenciesRecursive(AIJob job)
    {
        for (int i = 0; i < job.SubJobs.Count; i++)
        {
            if (job.JobQueue.Peek() == job.SubJobs[i])
            {
                Debug.Log("Removed job due to dependency failure! " + job.SubJobs[i]);
                Debug.LogFormat("That had {0} subjobs", job.SubJobs[i].SubJobs.Count);
                job.JobQueue.DeQueue();
                RemoveDependenciesRecursive(job.SubJobs[i]);
                job.SubJobs.RemoveAt(i);
                i = 0;
            }
        }
    }

    public virtual void OnFinish(bool success)
    {
        // dequeue all jobs that are dependant on this one if the job failed
        if (!success)
        {
            DebugUI.ShowMessage(AI.transform.position, "Job failed "+this);
            Debug.LogFormat("Job failed ({0}) subjobs = {1}",this,SubJobs.Count);
            RemoveDependenciesRecursive(this);
        }
        else
        {
            DebugUI.ShowMessage(AI.transform.position, "Job done " + this);
        }
    }

    public void AddDependentJob(AIJob dependency)
    {
        this.SubJobs.Add(dependency);
    }
}

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

    public void PushTop(AIJob job)
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

    public List<AIJob> ToList()
    {
        return _jq.ToList();
    }

    public void Clear()
    {
        _jq.Clear();
    }
}

public abstract class BaseAI : ManagedMonoBehaviour
{
    [HideInInspector]
    public AIJobQueue JobQueue;
    public NavMeshAgent UNavAgent;

    protected AIJob _curJob;
    public AIJob CurrentJob { get { return _curJob; } }

    /// <summary>
    /// If you override/hide then still do base.Awake()!
    /// </summary>
    protected void Awake()
    {
        JobQueue = new AIJobQueue(this);
        GameTime.MediumTick += MediumTick;
        UNavAgent = GetComponent<NavMeshAgent>();
    }

    protected void OnDestroy()
    {
        GameTime.MediumTick -= MediumTick;
    }

    void MediumTick(float dt)
    {
        if(_curJob != null)
        {
            _curJob.MediumTick();
        }
    }

    void CheckForNewJob()
    {
        if (_curJob == null)
        {
            _curJob = JobQueue.DeQueue();
            //Debug.Log("dqresult" + _curJob);
            if (_curJob != null)
            {
                Debug.Log("Job started " + _curJob);
                if (_curJob.CheckConstraints())
                    _curJob.StartJob();
            }
        }
    }

    /// <summary>
    /// If you override/hide then still do base.ManagedUpdate()!
    /// </summary>
    public override void ManagedUpdate()
    {
        CheckForNewJob();

        if (_curJob != null)
        {
            Profiler.BeginSample("BaseAI JobTick");
            _curJob.JobTick();
            Profiler.EndSample();

            bool success = false;
            if (_curJob.IsJobDone(ref success))
            {
                Debug.Log("Job done " + _curJob);
                _curJob.OnFinish(success);
                _curJob = null;
            }
        }
    }

    protected bool HasJob()
    {
        return _curJob != null || JobQueue.JobCount > 0;
    }

    /// <summary>
    /// Stops current job
    /// </summary>
    /// <param name="finished">True if the job won't be requeued, false if it will be.</param>
    public void CancelCurrentJob(bool finished)
    {
        if(finished)
            _curJob.OnFinish(false);
        _curJob = null;
    }

    /// <summary>
    /// Pushes another job on top of current job
    /// </summary>
    /// <param name="replaceJob"></param>
    public void DelayCurrentJobWithDependency(AIJob replaceJob)
    {
        var old = _curJob;
        CancelCurrentJob(false);
		if (old != null)
			JobQueue.PushTop(old);
		JobQueue.PushTop(replaceJob);
		replaceJob.AddDependentJob (old);
        CheckForNewJob();
    }

    public void RemoveAllWork()
    {
        if(_curJob != null)
            CancelCurrentJob(true);
        JobQueue.Clear();
    }
}
