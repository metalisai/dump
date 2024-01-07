using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public interface IProgressable
{
    float GetProgress();
}

[ObjectLogic(ActiveObject.ObjectTag.Progressable)]
public class ProgressUI : ManagedMonoBehaviour {

    string _name = null;

    PlaceableObject po;
    IProgressable progressable;
	
    public void SetName(string name)
    {
        _name = name;
    }

    void OnMouseEnter()
    {
        progressable = GetComponent<IProgressable>();
        if (progressable == null)
            po = GetComponent<PlaceableObject>();
        if(po == null && progressable == null)
            Debug.LogError("Progressable tag, but no IProgressable component!");
        UI.RequestPopup(this, UI.ProgressInterface.Show);
    }

	void OnMouseOver () {
        string nname;
        float progress;
        if(progressable == null)
        {
            nname = _name == null ? po.name : _name;
            progress = po.buildProgress;
        }
        else
        {
            nname = _name == null ? (progressable as Component).name : _name;
            progress = progressable.GetProgress();
        }
        UI.ProgressInterface.SetData(transform.position, nname, progress);
    }

    void OnMouseExit()
    {
        UI.HidePopup(this, UI.ProgressInterface.Hide);
    }
}
