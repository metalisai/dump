using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[ObjectLogic(ActiveObject.ObjectTag.PFDummy)]
public class PathfindingDummy : BaseAI
{
    public override void ManagedUpdate()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                //JobQueue.EnQueue(new MoveJob(hit.point, 5.0f, false));
                JobQueue.EnQueue(new UNavMoveJob(UNavAgent, hit.point, 0.0f));
            }
        }
        base.ManagedUpdate();
    }
}
