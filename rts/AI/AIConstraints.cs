using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RangeConstraint : AIWorkConstraint
{
    float _range;
    Transform _rangeTo;

    public RangeConstraint(Transform rangeTo, float range)
    {
        _range = range;
        _rangeTo = rangeTo;
    }

    public override bool AttemptFillConstraint()
    {
        Debug.Log("Attempting fill range constraing");
        // TODO: this can fail
        AI.DelayCurrentJobWithDependency(Jobs.GetMoveJob(AI, _rangeTo.transform.position, _range));
        return true;
    }

    public override bool IsConstraintFilled()
    {
        bool ret = Vector3.Distance(AI.transform.position, _rangeTo.transform.position) < _range;
        Debug.Log("Range constraing was: " + ret);
        return ret;
    }
}
