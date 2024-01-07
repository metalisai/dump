using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GardenerObserver
{
    private Housing _apartment;
    private Store _store;

    public GardenerObserver(Housing apartment)
    {
        _apartment = apartment;
        _store = MonoBehaviour.FindObjectOfType<Store>();
    }

    private List<GridCell> ReplantList = new List<GridCell>();
    private List<GrowingPlant> HarvestPendingPlants = new List<GrowingPlant>();

    public void Harvested(GrowingPlant plant)
    {
        int bef = HarvestPendingPlants.Count;
        HarvestPendingPlants.Remove(plant);
        Debug.Assert(bef > HarvestPendingPlants.Count);
    }

    public void QueueForReplant(GridCell _replantTarget)
    {
        ReplantList.Add(_replantTarget);
    }

    public void AskJob(GardenerAI gardener)
    {
        float bestDist = float.MaxValue;

        int jobtype = 0;
        GrowingPlant curTarget = null;
        GridCell curTarCell = null;

        // TODO: only supports 1 dryer
        if (_apartment.Dryers[0].State != WeedDryerState.Full)
        {
            foreach (var plant in _apartment.Plants)
            {
                if (plant.grown && !HarvestPendingPlants.Contains(plant))
                {
                    float comp = Vector3.Distance(plant.transform.position, gardener.transform.position);
                    if (bestDist > comp)
                    {
                        bestDist = comp;
                        jobtype = 1;
                        curTarget = plant;
                    }
                }
            }
        }

        if (_store.PeekItem(_store.GetStoreItemOfItem(PlaceableObjectType.WeedPlant)))
        {
            foreach (var cell in ReplantList)
            {
                float comp = Vector3.Distance(cell.Center, gardener.transform.position);
                if (bestDist > comp)
                {
                    bestDist = comp;
                    jobtype = 2;
                    curTarCell = cell;
                }
            }
        }

        switch (jobtype)
        {
            case 1:
                {
                    Debug.Log("gave 1");
                    Debug.Assert(curTarget != null, "curTarget != null");

                    /*Vector3 dest = curTarget.transform.position + ((transform.position - curTarget.transform.position).normalized * 0.325f);
                    var mj = new MoveJob(dest);*/
                    var hj = new HarvestJob(curTarget);
                    //hj.AddDependency(mj);
                    //JobQueue.EnQueue(mj);
                    gardener.JobQueue.EnQueue(hj);
                    HarvestPendingPlants.Add(curTarget);
                }
                break;
            case 2:
                {
                    Debug.Log("gave 2");
                    Debug.Assert(curTarCell != null, "curTarCell != null");
                    Vector3 dest = curTarCell.Center + ((gardener.transform.position - curTarCell.Center).normalized * 0.325f);
                    var mj = new MoveJob(dest);
                    var hj = new ReplantJob(curTarCell);
                    hj.AddDependency(mj);
                    gardener.JobQueue.EnQueue(mj);
                    gardener.JobQueue.EnQueue(hj);
                    ReplantList.RemoveAt(ReplantList.IndexOf(curTarCell));
                }
                break;
        }
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
