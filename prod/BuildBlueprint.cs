using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class BuildBlueprint : MonoBehaviour, IGameItemOwner, IInteractableObject {

    [System.Serializable]
	public struct BlueprintRecipeComponent
	{
		public ObjectId item;
		public int count;
	}

	public ObjectId resultObject;
	protected List<GameItem> itemsAdded = new List<GameItem> ();

	public List<BlueprintRecipeComponent> recipe = new List<BlueprintRecipeComponent>();

	public virtual void TryAddGameItemToRecipe(GameItem item) // default behaviour
	{
		bool contains = recipe.Any (x => x.item == item.ObjectId);
		if (contains) 
		{
			BlueprintRecipeComponent comp = recipe.FirstOrDefault (x => x.item == item.ObjectId);
			if(itemsAdded.Count (x => x.ObjectId == item.ObjectId) < comp.count)
			{
				itemsAdded.Add (item);
				item.GiveOwnershipTo(this);
                CheckIfRecipeComplete();
			}
		}
	}

    private void CheckIfRecipeComplete()
    {
        bool complete = true;
        foreach(var recComp in recipe)
        {
            if((itemsAdded.Count(x => x.ObjectId == recComp.item) != recComp.count))
            {
                complete = false;
                break;
            }
        }

        Debug.Log("c " + complete);
        if(complete)
        {
            var go = ObjectDatabase.CreateObject(resultObject);
            go.transform.position = this.transform.position;
            go.transform.rotation = this.transform.rotation;
            Destroy(gameObject);
        }
    }

    public GameObject Create()
    {
        var go = GameObject.Instantiate<GameObject>(gameObject);
        go.SetActive(true);
        var renderer = go.GetComponent<MeshRenderer>();
        renderer.material = GlobalSettings.BlueprintMaterial;
        return go;
    }

    public void StartPlacing()
    {
        ObjectPlacer.current.StartPlacing(Create());
    }

	#region IGameItemOwner implementation
	public void OnLoseOwnership (GameItem item)
	{
		itemsAdded.Remove (item);
	}
	public void OnGainOwnership (GameItem item)
	{

	}
    public void OnOwnedItemTransform(GameItem oldItem, GameItem newItem)
    {
        throw new System.NotImplementedException();
    }
	#endregion

    public void OnInteraction(IInteractableObject otherObject)
    {
        throw new System.NotImplementedException();
    }

    new public InteractableObjectType GetType()
    {
        return InteractableObjectType.ItemBlueprint;
    }

    public void OnInteractionWithOtherObject(IInteractableObject otherObject)
    {
        switch(otherObject.GetType())
        {
            case InteractableObjectType.GameItem:
                TryAddGameItemToRecipe(otherObject as GameItem);
                break;
            default:
                Debug.LogError("Undefined interaction " + this + " and " + otherObject);
                break;
        }
    }

    public void OnPlayerPointingAt()
    {
        // show recipe?
    }

    public void OnPointingAtEnd()
    {
        // hide recipe?
    }

}
