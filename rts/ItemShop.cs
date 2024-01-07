using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class ItemShop
{
    public struct StoreItem
    {
        public GameObjectID itemId;
        public int[] ResourceCosts;
    }

    [System.Serializable]
    public class ItemStoreSerializedItem
    {
        public GameObjectID ObjectId;
        public int[] ResourceValues;
        [System.NonSerialized]
        public bool expanded = false;
    }

    [System.Serializable]
    public class ItemStoreSerializeClass
    {
        public List<ItemStoreSerializedItem> Items;
    }

    List<StoreItem> items = new List<StoreItem>();
    public bool disabled = true;

    public void LoadItems()
    {
        var textAsset = Resources.Load<TextAsset>("storeitems");
        if(textAsset != null)
        {
            items.Clear();
            ItemStoreSerializeClass sc = JsonUtility.FromJson<ItemStoreSerializeClass>(textAsset.text);
            foreach(var thing in sc.Items)
            {
                items.Add(new StoreItem() { itemId = thing.ObjectId, ResourceCosts = thing.ResourceValues });
            }
        }
        else
        {
            Debug.LogError("No storeitems.json found!");
        }
    }

    void PurchaseFailed()
    {
        if (Game.Instance.PurchaseFailed != null)
            Game.Instance.UIAudioSource.PlayOneShot(Game.Instance.PurchaseFailed);
        else
            Debug.Log("No purchase failed sound!");
    }

    public bool CanBuyItem(GameObjectID item)
    {
        if (disabled)
            return true;
        try
        {
            var buyItem = items.Where(x => x.itemId == item).First();
            bool canBuy = true;
            for (int i = 0; i < buyItem.ResourceCosts.Length; i++)
            {
                ResourceType type = (ResourceType)i;
                int storedAmount = Game.Instance.GetResourceAmount(type);
                if (storedAmount < buyItem.ResourceCosts[i])
                {
                    canBuy = false;
                    break;
                }
            }
            return canBuy;
        }
        catch
        {
            Debug.LogErrorFormat("Tried to check item that isn't listed in the shop! ({0})", Enum.GetName(typeof(GameObjectID), item));
            return false;
        }
    }

    public bool BuyItem(GameObjectID item)
    {
        if (disabled)
            return true;
        try
        {
            var buyItem = items.Where(x => x.itemId == item).First();
            bool canBuy = CanBuyItem(item);

            if(canBuy)
            {
                for (int i = 0; i < buyItem.ResourceCosts.Length; i++)
                {
                    ResourceType type = (ResourceType)i;
                    Game.Instance.TakeResource(type, buyItem.ResourceCosts[i]);
                }
            }
            else
            {
                PurchaseFailed();
            }
            return canBuy;
        }
        catch
        {
            Debug.LogErrorFormat("Tried to buy item that isn't listed in the shop! ({0})", Enum.GetName(typeof(GameObjectID), item));
            PurchaseFailed();
            return false;
        }
    }
}