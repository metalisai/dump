using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UnitStorage : ManagedMonoBehaviour
{
    List<WorkerAI> storedUnits = new List<WorkerAI>();
    public int Capacity = 9998;
	bool _exiting = false;

    public void AddUnit(WorkerAI unit)
    {
        storedUnits.Add(unit);
        unit.gameObject.SetActive(false);
    }

    public WorkerAI EjectUnit()
    {
        if (storedUnits.Count > 0)
        {
            var unit = storedUnits[storedUnits.Count - 1];
            unit.gameObject.SetActive(true);
            storedUnits.RemoveAt(storedUnits.Count - 1);
            return unit;
        }
        else
        {
            return null;
        }
    }

    void OnDestroy()
    {
		if (_exiting)
			return;
		
        int count = storedUnits.Count;
        for(int i = 0; i < count; i ++)
        {
            EjectUnit();
        }
    }

	void OnApplicationQuit()
	{
		_exiting = true;
	}
}
