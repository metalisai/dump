// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public class HarvestJob : AIJob
{
    GrowingPlant _target;

    bool _done = false;
    bool _success = false;

    const float MAX_RANGE = 2f;

    public HarvestJob(GrowingPlant target)
    {
        _target = target;
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
        
    }

    public override void OnFinish(bool success)
    {
        // TODO: apparently there is a bug here
        _target.PlaceableObject.Infrastructure.GardenerObserver.Harvested(_target);
        base.OnFinish(success);
    }

    public override void StartJob()
    {
        // TODO: if the target is unrechable, this will end with an endless loop
        // TODO: null check because the plant can be destroyed on previous frame, this should now happen!
        if (_target != null && Vector3.Distance(AI.transform.position, _target.transform.position) > MAX_RANGE)
        {
            AI.CancelCurrentJob();
            Vector3 dest = _target.transform.position + ((AI.transform.position - _target.transform.position).normalized * 0.325f);
            var mj = new MoveJob(dest);
            AI.JobQueue.Push(this);
            AI.JobQueue.Push(mj);
        }
        else
        {
            AIEvents.OnAnimatorEvent += OnEvent;
            AI.Animator.SetBool("Harvest", true);
        }
    }

    void OnEvent(AIEvents.AIEventId eventId, Animator animator)
    {
        switch(eventId)
        {
            case AIEvents.AIEventId.Gardener_Harvest_Done:
                if(animator == AI.Animator)
                {
                    Debug.Log("Harvest animation finished");
                    _success = _target.Harvest();
                    _done = true;
                    AIEvents.OnAnimatorEvent -= OnEvent;
                }
                break;
        }
    }
}
