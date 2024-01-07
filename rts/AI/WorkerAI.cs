using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum ResourceType
{
    Wood,
    Stone,
    Iron,
    Gold,
    Oil
}
public struct ResourcePack
{
    public ResourceType ResourceType;
    public int Amount;
}
	
[ObjectLogic(ActiveObject.ObjectTag.WorkerAI)]
public sealed class WorkerAI : BaseAI
{
    public int InventoryCapacity = 6;
    public List<ResourcePack> Inventory = new List<ResourcePack>();
    int? debugGUid = null;

    bool scared = false;

	public bool IsInventoryFull()
	{
		return Inventory.Count >= 6;
	}

    public bool IsInventoryEmpty()
    {
        return Inventory.Count <= 0;
    }

    new void Awake()
    {
        base.Awake();
        GameTime.SlowTick += JobUpdate;
    }

    void OnScaredStateChange(bool newValue)
    {
        if (newValue == true)
        {
            var nexus = Game.Instance.Nexus;
            if (nexus != null)
            {
                RemoveAllWork();
                JobQueue.EnQueue(new BoardJob(Game.Instance.Nexus.UnitStorage, this));
				WorkerWorkScheduler.LeaveWork(this);
            }
        }
    }

    void JobUpdate(float timeDelta)
    {
        int hour = GameTime.Hour;
        if (hour >= 21 || hour < 6)
        {
            if (!scared)
                OnScaredStateChange(true);
            scared = true;
        }
        else
        {
            if (scared)
                OnScaredStateChange(false);
            scared = false;
        }

        if (!scared && _curJob == null && JobQueue.JobCount == 0)
        {
            WorkerWorkScheduler.AskJob(this);
        }
    }

    public void Board(UnitStorage unitStorage)
    {
        unitStorage.AddUnit(this);
    }
		
    public override void ManagedUpdate()
    {
        base.ManagedUpdate();
    }

    new void OnDestroy()
    {
        GameTime.SlowTick -= JobUpdate;
		WorkerWorkScheduler.LeaveWork(this);
        if (debugGUid != null)
            DebugUI.RemoveParameter(debugGUid.Value);
        base.OnDestroy();
    }

    public void NoJob()
    {
        GetComponentInChildren<MeshRenderer>().material.color = Color.red;
    }

    public void Working()
    {
        GetComponentInChildren<MeshRenderer>().material.color = Color.green;
    }

    void OnDrawGizmos()
    {
        var col = Color.cyan;
        col.a = 0.5f;
        Gizmos.color = col;
        Gizmos.DrawSphere(transform.position, 3.0f);
    }
}
