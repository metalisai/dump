using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

public static class WorkerWorkScheduler
{
	public enum WorkerType
	{
		Unknown = 0,
		Laborer = 1,
		Lumberjack,
		IronMiner,
		StoneMiner,
		GoldMiner
	}

	static List<ResourceSource> GatherList = new List<ResourceSource>();

	static List<WorkerAI> Laborers = new List<WorkerAI>();
	static List<WorkerAI> Lumberjacks = new List<WorkerAI>();
	static List<WorkerAI> IronMiners = new List<WorkerAI>();
	static List<WorkerAI> StoneMiners = new List<WorkerAI>();
	static List<WorkerAI> GoldMiners = new List<WorkerAI>();

	static Dictionary<WorkerAI, WorkerType> WorkerRoles = new Dictionary<WorkerAI, WorkerType> ();

	static int[] roleLimits;

	static WorkerWorkScheduler ()
	{
		int roleCount = Enum.GetNames (typeof(WorkerType)).Length;
		roleLimits = new int[roleCount];
		for (int i = 0; i < roleCount; i++) {
			roleLimits [i] = 0;
		}
        Func<string> str = () => {
            if (availableDeliverJobs == null)
                return "";
            StringBuilder strb = new StringBuilder();
            availableDeliverJobs.OrderBy(x => x.priorityValue).ToList().ForEach(x => strb.Append(x.priorityValue+" "));
            return strb.ToString();
        };
        DebugUI.AddParameter("Priorities", str);
	}

	public static int CountInRole(WorkerType role)
	{
		switch(role)
		{
		case WorkerType.Laborer:
			return Laborers.Count;
		case WorkerType.Lumberjack:
			return Lumberjacks.Count;
		case WorkerType.IronMiner:
			return IronMiners.Count;
		case WorkerType.StoneMiner:
			return StoneMiners.Count;
		case WorkerType.GoldMiner:
			return GoldMiners.Count;
		default: return 0;
		}
	}

	static List<WorkerAI> GetArrayFromRole(WorkerType role)
	{
		switch(role)
		{
		case WorkerType.Laborer:
			return Laborers;
		case WorkerType.Lumberjack:
			return Lumberjacks;
		case WorkerType.IronMiner:
			return IronMiners;
		case WorkerType.StoneMiner:
			return StoneMiners;
		case WorkerType.GoldMiner:
			return GoldMiners;
		default: return null;
		}
	}

	public static int RoleLimits(WorkerType role)
	{
		return roleLimits[(int)role];
	}

	static WorkerType GetMostNeededType()
	{
		// TODO: instead of stopping on first match, pick a role that has no workers or significantly less workers than other roles
		for (int i = (int)WorkerType.Laborer+1; i < roleLimits.Length; i++) {
			WorkerType role = (WorkerType)i;
			if (CountInRole (role) < roleLimits [i])
				return role;
		}
		return WorkerType.Laborer;
	}

	public static WorkerType AssignRole(WorkerAI worker)
	{
		var wtype = GetMostNeededType ();
		Debug.Log ("Assigned "+wtype);
		var arr = GetArrayFromRole (wtype);
		arr.Add (worker);
		WorkerRoles.Add (worker, wtype);
		UpdateLaborerLimit (); // TODO: needed?
		return wtype;
	}

	public static void AddGatheObject(ResourceSource obj)
	{
		if(!GatherList.Contains(obj))
			GatherList.Add(obj);
	}

	public static void RemoveGatheObject(ResourceSource obj)
	{
		GatherList.Remove(obj);
	}

	static bool CheckIfNeedStoreResources(WorkerAI worker, bool checkFull = true)
	{
        bool canStore = checkFull ? worker.IsInventoryFull() : !worker.IsInventoryEmpty();
		if (canStore) {
            // TODO: check if warehouse is full
            var genericStore = Game.FindClosestGenericStore(worker.transform.position);
            if (genericStore != null)
            {
                worker.JobQueue.EnQueue(new StoreJob(genericStore, worker));
                return true;
            }
            else
                return false;
		}
		return false;
	}

	static bool CheckIfCanGatherResource(ResourceType rtype, WorkerAI worker)
	{
		// TODO: limit number of workers per source
		float minSqDist = float.MaxValue;
		ResourceSource rsource = null;
		for (int i = 0; i < GatherList.Count; i++) {
			var rs = GatherList [i];
			if (rs.Type == rtype) {
				float sqDist = Vector3.SqrMagnitude (worker.transform.position - rs.transform.position);
				if (sqDist < minSqDist) {
					minSqDist = sqDist;
					rsource = rs;
				}
			}
		}
		if (rsource != null) {
			worker.JobQueue.EnQueue(new GatherJob(rsource, worker));
			return true;
		} else
			return false;
	}

	public static void AskJob(WorkerAI worker)
	{
		//Debug.Log ("Ask job");
		WorkerType wtype;
		if (WorkerRoles.TryGetValue (worker, out wtype)) {
			// worker is already assigned
		} else {
			wtype = AssignRole (worker);
		}

		switch (wtype) {
		case WorkerType.Laborer:
            if (CheckIfNeedStoreResources(worker, false))
                return;
            CheckIfCanDeliver (worker);
			break;

		case WorkerType.GoldMiner:
			if (CheckIfNeedStoreResources (worker))
				return;
			if(CheckIfCanGatherResource(ResourceType.Gold, worker))
				return;
            if (CheckIfNeedStoreResources(worker, false))
                return;
            break;

		case WorkerType.StoneMiner:
			if (CheckIfNeedStoreResources (worker))
				return;
			if(CheckIfCanGatherResource(ResourceType.Stone, worker))
				return;
            if (CheckIfNeedStoreResources(worker, false))
                return;
			break;

		case WorkerType.Lumberjack:
			if (CheckIfNeedStoreResources (worker))
				return;
			if(CheckIfCanGatherResource(ResourceType.Wood, worker))
				return;
            if (CheckIfNeedStoreResources(worker, false))
                return;
            break;
		}
	}

	public static void SetLimit(WorkerType role, int limit)
	{
		if (role == WorkerType.Laborer)
			return;
		roleLimits [(int)role] = limit;
		CheckIfNeedReassign ();
	}

	static void UpdateLaborerLimit()
	{
		int otherCount = 0;
		for (int i = (int)WorkerType.Laborer + 1; i < roleLimits.Length; i++)
			otherCount += CountInRole((WorkerType)i);
		roleLimits[(int)WorkerType.Laborer] = Mathf.Max(0, WorkerRoles.Count-otherCount);
		Debug.Log ("Laborer count set to "+roleLimits[(int)WorkerType.Laborer]);
	}

	static void CheckIfNeedReassign()
	{
		// check if need to move workers from one job to another
		// TODO: should reassigned worker stop work immidiately and switch?
		for (int i = (int)WorkerType.Laborer + 1; i < roleLimits.Length; i++) {
			WorkerType wtype = (WorkerType)i;
			if (CountInRole (wtype) > roleLimits [i]) { // too many in role, reassign
				int removeCount = CountInRole (wtype) - roleLimits [i];
				Debug.LogFormat ("Too many {0}, reassigning {1}", wtype, removeCount);
				var arr = GetArrayFromRole (wtype);
				for (int remIt = 0; remIt < removeCount; remIt++) {
					var worker = arr [arr.Count - 1];
					WorkerRoles.Remove (worker);
					arr.RemoveAt (arr.Count - 1); // remove last
					AssignRole(worker);
				}

			} else if (CountInRole (wtype) < roleLimits [i]) { // not enough in role, check if any laborers to reassign to this role
				int missingCount = roleLimits [i] - CountInRole (wtype);
				Debug.LogFormat ("Too litte {0}, reassigning {1} laborers", wtype, missingCount);
				for (int addIt = 0; addIt < missingCount; addIt++) {
					if (CountInRole (WorkerType.Laborer) > 0) {
						var worker = Laborers [Laborers.Count - 1];
						WorkerRoles.Remove (worker);
						Laborers.RemoveAt(Laborers.Count - 1);
						AssignRole (worker);
					} else
						break;
				}
			}
		}
	}

	public static void LeaveWork(WorkerAI worker)
	{
		WorkerType wtype;
		if (WorkerRoles.TryGetValue (worker, out wtype)) {
			var arr = GetArrayFromRole (wtype);
			arr.Remove (worker);
			WorkerRoles.Remove (worker);
			CheckIfNeedReassign ();
		} else {
			Debug.LogWarning ("Worker that never signed up for work should not leave work!");
		}
	}

    class DeliverJobData
    {
        public IResourceStore source;
        public IResourceStore destination;
        public ResourceType resourceToDeliver;
        public int amount;
        public float priorityValue;
    }

    static List<DeliverJobData> availableDeliverJobs = new List<DeliverJobData>();
    static void UpdateDeliverJobs() // loops in loops in loop
    {
        availableDeliverJobs.Clear();
        foreach (var wh in Game.ResourceStores)
        {
            foreach (var constraint in wh.GetConstraints())
            {
                if (constraint.resourceType == null)
                    continue;
                int resourceInFirst = wh.GetResourceCount(constraint.resourceType.Value);
                if (constraint.resourceAmount > resourceInFirst)
                {
                    foreach (var wh2 in Game.ResourceStores)
                    {
                        if (wh2 == wh)
                            continue;
                        int spare = wh2.GetSpareResourceCount(constraint.resourceType.Value);
                        if (spare > 0)
                        {
                            int neededAmount = constraint.resourceAmount - resourceInFirst;
                            float fromEmpty = resourceInFirst == 0 ? 10000.0f : 0.0f; // filling empty stores is most important by far
                            float fromMissing = (neededAmount / constraint.resourceAmount)*10.0f; // filled precentage secondary priority (lower precentage = higher priority)
                            float priority = fromEmpty + fromMissing;
                            availableDeliverJobs.Add(new DeliverJobData() {
                                source = wh2,
                                destination = wh,
                                resourceToDeliver = constraint.resourceType.Value,
                                amount = neededAmount,
                                priorityValue = priority
                            });
                        }
                    }
                }
            }
        }
    }

	static bool CheckIfCanDeliver(WorkerAI worker)
	{
        // TODO: use better data structure and dont update every time
        UpdateDeliverJobs();
        if (availableDeliverJobs.Count > 0)
        {
            availableDeliverJobs.Sort(delegate (DeliverJobData c1, DeliverJobData c2) { return c1.priorityValue.CompareTo(c2.priorityValue); });
            var job = availableDeliverJobs[availableDeliverJobs.Count - 1];
            //availableDeliverJobs.RemoveAt(availableDeliverJobs.Count - 1);
            Jobs.QueueDeliverJob(worker, job.source, job.destination, job.resourceToDeliver, job.amount);
            return true;
        }
        return false;
	}
}