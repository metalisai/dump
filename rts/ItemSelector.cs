using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public interface ISelectable
{
    void OnSelected();
    void OnDeSelected();
}

public class ItemSelector
{
    public GameObject SelectedObject;
    ItemHighligher highligher;

    public ItemSelector(ItemHighligher highlighter)
    {
        this.highligher = highlighter;
    }

    void ItemSelected(GameObject item)
    {
        var ao = item.GetComponent<ActiveObject>();
        if (ao == null)
            return;

        if (SelectedObject != null)
            CancelSelection();
        highligher.HighlightGameObject(item, Color.yellow);
        SelectedObject = item;
        Game.TrackLifetime(item, OnSelectedObjectDestroyed);
        UI.ItemSelectInterface.Show(item.name, DestroySelection);
        if(ao.HasTag(ActiveObject.ObjectTag.Selectable))
        {
            var selectable = item.GetComponent<ISelectable>();
            Assert.IsNotNull(selectable, "Selectable tag set, but no ISelectable attached!");
            selectable.OnSelected();
        }
    }

    void DestroySelection()
    {
        if (SelectedObject == null)
            return;
        Game.Instance.DestroyDynamicObject(SelectedObject);
    }

    void OnSelectedObjectDestroyed(GameObject go)
    {
        Debug.LogWarning("Item destroyed during selection");
        Assert.AreEqual(SelectedObject, go, "OnSelectedObjectDestroyed should be unsubscribed from lifetime tracking on selection change!");
        CancelSelection();
    }

    public void CancelSelection()
    {
        if (SelectedObject == null)
            return;
        Game.StopTracking(SelectedObject, OnSelectedObjectDestroyed);
        UI.ItemSelectInterface.Hide();
        highligher.RemoveHighlight(SelectedObject);

        var ao = SelectedObject.GetComponent<ActiveObject>();
        Assert.IsNotNull(ao);
        if (ao.HasTag(ActiveObject.ObjectTag.Selectable))
        {
            var selectable = SelectedObject.GetComponent<ISelectable>();
            Assert.IsNotNull(selectable, "Selectable tag set, but no ISelectable attached!");
            selectable.OnDeSelected();
        }

        SelectedObject = null;
    }

    public void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f))
        {
            if(Extensions.GetMouseButtonDownNoUI(0))
            {
                if(hit.collider.transform.root == hit.collider.transform) // is root object
                    ItemSelected(hit.collider.gameObject);
            }
            if (Extensions.GetMouseButtonDownNoUI(1))
            {
                CancelSelection();
            }
        }
    }
}