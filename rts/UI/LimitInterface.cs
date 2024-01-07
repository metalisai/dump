using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class LimitInterface : ManagedMonoBehaviour {

	// Use this for initialization
	void Start () {
		var tnames = Enum.GetNames(typeof(WorkerWorkScheduler.WorkerType));
        var reference = transform.GetChild(0).GetChild(0);
        for(int i = 0; i < tnames.Length; i++)
        {
            var newn = GameObject.Instantiate(reference.gameObject);
            newn.transform.SetParent(reference.parent);
            int locali = i;
            UnityAction<string> lm = (inval) =>
            {
                WorkerWorkScheduler.SetLimit((WorkerWorkScheduler.WorkerType)locali, int.Parse(inval));
				Debug.Log("Limit of "+Enum.GetName(typeof(WorkerWorkScheduler.WorkerType), (WorkerWorkScheduler.WorkerType)locali)+" set to "+inval);
            };
            newn.GetComponentInChildren<InputField>().onEndEdit.RemoveAllListeners();
            newn.GetComponentInChildren<InputField>().onEndEdit.AddListener(lm);
            newn.transform.FindChild("Label").GetComponent<Text>().text = tnames[i];
        }
        Destroy(reference.gameObject);
	}

	public override void ManagedUpdate()
	{
		var tnames = Enum.GetNames(typeof(WorkerWorkScheduler.WorkerType));
		for (int i = 0; i < tnames.Length; i++) {
			var trans = transform.GetChild (0).GetChild (i);
			trans.gameObject.GetComponentInChildren<InputField> ().text = WorkerWorkScheduler.RoleLimits((WorkerWorkScheduler.WorkerType)i).ToString();
		}
	}
}
