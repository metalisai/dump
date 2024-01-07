using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ResourceSource : ManagedMonoBehaviour
{
    public ResourceType Type;
    public int ResourceLeft;

    public delegate void ResourceSourceEvent();
    public event ResourceSourceEvent OutOfResources;
    public Transform MoveTarget;
    public float ValidDistanceFromTarget = 0.5f;
    bool added = false;

    void Start()
    {
        if(MoveTarget == null)
        {
            MoveTarget = transform;
            Debug.LogWarningFormat("Resource source {0} is missing MoveTarget", name);
        }
    }

    void AddToGather()
    {
        added = true;
        WorkerWorkScheduler.AddGatheObject(this);
    }

    void OnDestroy()
    {
        if(OutOfResources != null)
            OutOfResources.Invoke();
        if (added)
            WorkerWorkScheduler.RemoveGatheObject(this);
    }

    public ResourcePack GetResource(int amount)
    {
        int ramount = Mathf.Min(amount, ResourceLeft);
        ResourceLeft -= ramount;
        if(ResourceLeft == 0)
        {
            Game.Instance.DestroyDynamicObject(this.gameObject);
        }
        return new ResourcePack() { Amount = ramount, ResourceType = Type };
    }

    public void  ShowMineButton()
    {
        UI.Button.SetActive(true);
        var button = UI.Button.GetComponent<Button>();
        var text = UI.Button.GetComponentInChildren<Text>();
        text.text = "Mine";
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(AddToGather);
    }

    public void HideMineButton()
    {
        UI.Button.SetActive(false);
    }

    public void OnMouseEnter()
    {
        UI.RequestPopup(this, ShowMineButton);
    }

    public void OnMouseExit()
    {
        UI.HidePopup(this, HideMineButton);
    }

    public void OnMouseOver()
    {
        UI.Button.transform.position = Camera.main.WorldToScreenPoint(transform.position);
    }
}
