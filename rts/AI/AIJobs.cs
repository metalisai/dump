using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.Diagnostics;
using UnityEngine.Profiling;

public class BoardJob : AIJob
{
    WorkerAI workerAI;

    UnitStorage storage;
    bool done = false;
    bool success = false;

    const float gatherDistance = 20.0f;
    public BoardJob(UnitStorage storage, WorkerAI wai)
    {
        AddConstraint(new RangeConstraint(storage.transform, gatherDistance));
        this.storage = storage;
        workerAI = wai;
    }

    public override bool IsJobDone(ref bool success)
    {
        if (done)
        {
            success = this.success;
            return true;
        }
        return false;
    }

    public override void JobTick()
    {
        if (!done)
        {
            workerAI.Board(storage);
            done = true;
            success = true;
        }
    }

    public override void StartJob()
    {
        /*if (workerAI.Inventory.Count < workerAI.InventoryCapacity)
            gathering = true;
        else
        {
            gathering = false;
            done = true;
            success = false;
        }*/
    }
}

public class PickupJob : AIJob
{
    WorkerAI _worker;
    IResourceStore _source;
    ResourceType _rtype;
    int _ramount;

    bool _done = false;
    bool _success = false;

    public PickupJob(WorkerAI worker, IResourceStore source, ResourceType resourceType, int amount)
    {
        _worker = worker;
        _source = source;
        _rtype = resourceType;
        _ramount = amount;
        AddConstraint(new RangeConstraint(source.rsgameObject.transform, 5.0f));
    }

    public override bool IsJobDone(ref bool success)
    {
        if (_done)
        {
            success = _success;
            return true;
        }
        return false;
    }

    public override void JobTick()
    {
        if(!_done)
        {
			int spare = Mathf.Min(_source.GetSpareResourceCount(_rtype), _ramount);
            int got = _source.TakeResource(_rtype, Mathf.Min(spare, _worker.InventoryCapacity));
			DebugUI.ShowMessage (_worker.transform.position, "Pickup "+got+" "+_rtype);
            _worker.Inventory.Add(new ResourcePack() { Amount = got, ResourceType = _rtype });
            _done = true;
            _success = got != 0;
        }
    }

    public override void StartJob()
    {
        // :L
    }
}

public static class Jobs
{
    public static void QueueDeliverJob(WorkerAI worker, IResourceStore source, IResourceStore destination, ResourceType rtype, int ramount)
    {
		var pj = new PickupJob (worker, source, rtype, ramount);
        worker.JobQueue.EnQueue(pj);
		var sj = new StoreJob (destination, worker);
		pj.AddDependentJob (sj);
		worker.JobQueue.EnQueue(sj);
    }

    public static AIJob GetMoveJob(BaseAI ai, Vector3 destination, float minDistance = 0.0f, bool allowDestroy = false)
    {
#if true
        var ret = new UNavMoveJob(ai.UNavAgent, destination, minDistance);
#else
        var ret = new MoveJob(destination, 5.0f, allowDestroy);
#endif
        return ret;
    }

    public static AIJob QueueMoveJob(BaseAI ai, Vector3 destination, float minDistance = 0.0f, bool allowDestroy = false)
    {
#if true
        var ret = new UNavMoveJob(ai.UNavAgent, destination, minDistance);
#else
        var ret = new MoveJob(destination, 5.0f, allowDestroy);
#endif
        ai.JobQueue.EnQueue(ret);
        return ret;
    }
}

public sealed class UNavMoveJob : AIJob
{
    NavMeshAgent _agent;
    Vector3 _destination;
    float _minDistance;

    public UNavMoveJob(NavMeshAgent agent, Vector3 destination, float minDistance)
    {
        _destination = destination;
        _agent = agent;
        _minDistance = minDistance;
    }

    public override bool IsJobDone(ref bool success)
    {
        // TODO: fail?
        success = true;
        if(!Mathf.Approximately(_minDistance, 0.0f) && (_agent.transform.position-_destination).magnitude <= _minDistance)
        {
            if (_agent.pathPending || _agent.hasPath)
                _agent.ResetPath();
            success = true;
            return true;
        }
        else if(!_agent.pathPending && !_agent.hasPath)
        {
            success = true;
            return true;
        }
        return false;
    }

    public override void JobTick()
    {
    }

    public override void StartJob()
    {
        _agent.SetDestination(_destination);
    }

    public override void OnFinish(bool success)
    {
        _agent.ResetPath();
        base.OnFinish(success);
    }
}


public sealed class MoveJob : AIJob
{
    Vector3 destination;
    Vector3 velocity = Vector3.zero;

    bool moving = false;
    bool _success = true;
    Vector3 currentTarget;

    Vector3 curdirection = Vector3.zero;
    Vector3 avoidanceForce = Vector3.zero;

    Vector3 direction;
    //Vector3 avoidDir;

    PathQueryResult pqResult;
    bool waitingPath = false;
    bool _allowDestroy = false;

    List<Vector3> wayPoints;
    float speed = 10.0f;
    float acceleration = 25.0f;

    bool moveToDistance = false;
    float distance = 0.0f;

    public MoveJob(Vector3 dest, bool allowDestroy)
    {
        destination = dest;
        _allowDestroy = allowDestroy;
    }

    public MoveJob(Vector3 dest, float distance, bool allowDestroy)
    {
        destination = dest;
        moveToDistance = true;
        this.distance = distance;
        _allowDestroy = allowDestroy;
    }

    public override bool IsJobDone(ref bool success)
    {
        if (!moving && !waitingPath)
        {
            success = _success;
            return true;
        }
        return false;
    }

    const float lookAheadDistance = 10.0f;
    const float radius = 1.5f;
    const float radius2 = radius * radius;
    const float avoidanceForceAmount = 15.0f;
    const float minIntersectionSqDist = 100.0f;

    public override void MediumTick()
    {
        Vector3 forceFrom = Vector3.zero;
        float lookAhead = lookAheadDistance * (velocity.magnitude / speed);

        // calculate vector to avoid other workers
        var units = Game.Instance.AiUnits;
        Vector3 lookaheadPos = AI.transform.position + curdirection * lookAhead;
#if false// draws lookahead line
        Debug.DrawLine(AI.transform.position + Vector3.up, lookaheadPos + Vector3.up, Color.blue);
#endif

        Profiler.BeginSample("MoveJob AvoidanceVector");
        float minDist = float.MaxValue;
        int closestUnitIndex = -1;
        Vector3 closestUnitPos = Vector3.zero;
        Vector3 aiPos = AI.transform.position;
        for(int i = 0; i < units.Count; i++)
        {
            if (units[i] == AI)
                continue;
            Vector3 workerPos = units[i].transform.position;
            if ((workerPos - aiPos).sqrMagnitude > minIntersectionSqDist) // too far, can't possibly intersect
                continue;
            if (RTSMath.LineIntersectsCircle(ref aiPos, ref lookaheadPos, ref workerPos, radius2, out forceFrom))
            {
                float dist = Vector3.SqrMagnitude(aiPos-workerPos);
                if (minDist > dist)
                {
                    minDist = dist;
                    closestUnitIndex = i;
                    closestUnitPos = workerPos;
                }
            }
        };
        Profiler.EndSample();

        avoidanceForce = Vector3.zero;
        if (closestUnitIndex != -1)
        {
            avoidanceForce = (forceFrom - closestUnitPos).normalized * avoidanceForceAmount;
#if false // draws line from AI to closest unit and avoidance force line
            Debug.DrawLine(AI.transform.position, closestUnitPos, Color.red);
            Debug.DrawLine(forceFrom, AI.transform.position + avoidanceForce, Color.yellow);
#endif
        }
    }

    public override void JobTick()
    {
        if (waitingPath && pqResult.done)
        {
            waitingPath = false;
            moving = true;
            wayPoints = pqResult.waypoints;
            NextWayPoint();
        }
        else if (waitingPath)
            return;

        Profiler.BeginSample("MoveJob Update");
        Vector3 pos = AI.transform.position;
        //float distToCurrent = (currentTarget - pos).magnitude;
        curdirection = (currentTarget - pos).normalized;
        if (Vector3.Distance(pos, destination) <= 2.0f)
        {
            moving = false;
        }
        else if (moving)
        {
            float dt = Time.deltaTime;
            velocity += dt* acceleration * curdirection;
            velocity += dt * avoidanceForce;
            velocity -= velocity * 0.5f*dt; // drag
            if (velocity.magnitude > speed)
                velocity = velocity.normalized * speed;
            AI.transform.position += dt * velocity;
			// Put back on terrain in case it goes through it
			// TODO: might be slow
			pos = AI.transform.position;
			pos.y = Game.customTerrain.SampleHeight (pos);
			AI.transform.position = pos;

            if (Vector3.Dot(currentTarget - pos, direction) <= 0)
            {
                NextWayPoint();
            }
        }
        Profiler.EndSample();

#if false // draws current path
        for(int i = 0; i < wayPoints.Count; i++)
        {
            if (i == 0)
                continue;
            Debug.DrawLine(wayPoints[i]+Vector3.up, wayPoints[i - 1]+Vector3.up, Color.blue);
        }
#endif
    }

    void NextWayPoint()
    {
        // TODO: use linked list or something (removing slow)
        if (wayPoints.Count > 0)
        {
            currentTarget = wayPoints[wayPoints.Count - 1];
            wayPoints.RemoveAt(wayPoints.Count - 1);
            direction = (currentTarget - AI.transform.position).normalized;
        }
        else
        {
            if(Vector3.Distance(AI.transform.position, destination) <= 5.0f)
            {
                moving = false;
                _success = true;
            }
            else if(Vector3.Distance(AI.transform.position, destination) >= 20.0f)
            {
                moving = false;
                _success = false;
            }
            else
            {
                currentTarget = destination;
            }
        }
    }

    public override void StartJob()
    {
        if (moveToDistance)
        {
            pqResult = Game.pathFinding.findPathWithDistanceAsync(AI.transform.position, destination, distance, _allowDestroy);
        }
        else
        {
            throw new NotImplementedException();
        }
        waitingPath = true;
    }
}

public sealed class GatherJob : AIJob
{
    ResourceSource rsrc;
    WorkerAI workerAI;

    bool gathering = true;
    float gatherTimer = 0.0f;
    float gatherGoal = 10.0f;
    bool done = false;
    bool success = false;

    const float gatherDistance = 10.0f;
    public GatherJob(ResourceSource source, WorkerAI wai)
    {
        AddConstraint(new RangeConstraint(source.MoveTarget, source.ValidDistanceFromTarget));
        rsrc = source;
        workerAI = wai;
        rsrc.OutOfResources += RSOutOfResources;
    }

    public override void OnFinish(bool success)
    {
        rsrc.OutOfResources -= RSOutOfResources;
        base.OnFinish(success);
    }

    public void RSOutOfResources()
    {
        done = true;
        success = true;
        gathering = false;
    }

    public override bool IsJobDone(ref bool success)
    {
        if (done)
        {
            success = this.success;
            return true;
        }
        return false;
    }

    public override void JobTick()
    {
        if (gathering)
        {
            gatherTimer += Time.deltaTime;
            if (gatherTimer >= gatherGoal)
            {
				DebugUI.ShowMessage (workerAI.transform.position, "Gather 1 " + rsrc.Type);
                Debug.Log("Resource gathered!");
                gatherTimer = 0.0f;
                workerAI.Inventory.Add(rsrc.GetResource(1));
                if (workerAI.Inventory.Count >= workerAI.InventoryCapacity)
                {
                    gathering = false;
                    done = true;
                    success = true;
                }
            }
        }
    }

    public override void StartJob()
    {
        if (workerAI.Inventory.Count < workerAI.InventoryCapacity)
            gathering = true;
        else
        {
            gathering = false;
            done = true;
            success = false;
        }
    }
}

public class StoreJob : AIJob
{
    IResourceStore _store;
    WorkerAI workerAI;

    bool storing = true;
    float storeTimer = 0.0f;
    float storeGoal = 3.0f;
    bool done = false;
    bool success = false;

    public StoreJob(IResourceStore store, WorkerAI wai)
    {
        Assert.IsNotNull(store, "StoreJob job can't be null");
        AddConstraint(new RangeConstraint(store.rsgameObject.transform, 10.0f));
        _store = store;
        workerAI = wai;
    }

    public override bool IsJobDone(ref bool success)
    {
        if (done)
        {
            success = this.success;
            return true;
        }
        return false;
    }

    public override void JobTick()
    {
        if (storing)
        {
            storeTimer += Time.deltaTime;
            if (storeTimer >= storeGoal)
            {
                Debug.Log("Resource stored!");
				DebugUI.ShowMessage (AI.transform.position, "Store 1 " + workerAI.Inventory [workerAI.Inventory.Count - 1].ResourceType);
                storeTimer = 0.0f;
                ResourcePack storePack = workerAI.Inventory[workerAI.Inventory.Count - 1];
                workerAI.Inventory.RemoveAt(workerAI.Inventory.Count - 1);
                _store.StoreResource(storePack);
                if (workerAI.Inventory.Count <= 0)
                {
                    storing = false;
                    done = true;
                    success = true;
                }
            }
        }
    }

    public override void StartJob()
    {
        if (workerAI.Inventory.Count > 0)
            storing = true;
        else
        {
            storing = false;
            done = true;
            success = false;
        }
    }
}