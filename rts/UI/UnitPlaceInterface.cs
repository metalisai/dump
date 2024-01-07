using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.IO;

public class UnitPlaceInterface : ManagedMonoBehaviour {

    class PlaceMenuData
    {
        public string ObjectName;
        public UnityAction OnClick;
    }

    void OnPlacingEnd()
    {
        Game.Instance.ItemPlacer.PlacingEnded -= OnPlacingEnd;
        Game.Instance.ItemPlacer.ObjectPlaced -= OnAttemptPlace;
    }

    void OnAttemptPlace(PlaceableObject obj, GameObjectID objId)
    {
        var shop = Game.Instance.ItemShop;
        if(shop.CanBuyItem(objId))
        {
            if(Game.Instance.ItemPlacer.AttemptPlace())
            {
                shop.BuyItem(objId);
            }
        }
    }

    public void SetCategory(int category)
    {
        PlaceableObjectCategory cat = (PlaceableObjectCategory)category;

        List<GameObjectID> fromList = null;

        switch(cat)
        {
            case PlaceableObjectCategory.Electricity:
                fromList = Game.CurrentResourceInfo.ElectricityPlaceables;
                break;
            case PlaceableObjectCategory.Utility:
                fromList = Game.CurrentResourceInfo.UtilityPlaceables;
                break;
            case PlaceableObjectCategory.Workers:
                fromList = Game.CurrentResourceInfo.UnitPlaceables;
                break;
            case PlaceableObjectCategory.Defense:
                fromList = Game.CurrentResourceInfo.DefensePlaceables;
                break;
        }

        Transform contentTrans = transform.FindChild("Content");
        int childCount = contentTrans.childCount;

        int objectCount = fromList.Count;

        List<PlaceMenuData> pmdata = new List<PlaceMenuData>();
        for(int i = 0; i < fromList.Count; i++)
        {
            PlaceMenuData data = new PlaceMenuData();
            GameObjectID objId = fromList[i];
            UnityAction onClick = () =>
            {
                Game.Instance.ItemPlacer.PlacingEnded += OnPlacingEnd;
                Game.Instance.ItemPlacer.ObjectPlaced += OnAttemptPlace;
                Game.Instance.ItemPlacer.PlaceObject(objId);
                gameObject.SetActive(false);
            };
            data.OnClick = onClick;
            data.ObjectName = ObjectRegister.GetResourceName(objId);
            pmdata.Add(data);
        }

        switch(cat)
        {
            case PlaceableObjectCategory.Electricity:
                UnityAction onClick = () =>
                {
                    WallPlacer.StartPlacing();
                    gameObject.SetActive(false);
                };
                pmdata.Add(new PlaceMenuData() { ObjectName = "Wall", OnClick = onClick});
                break;
        }

        for (int i = 0; i < childCount; i++)
        {
            if (i < pmdata.Count)
            {
                contentTrans.GetChild(i).gameObject.SetActive(true);
                var text = contentTrans.GetChild(i).GetComponentInChildren<Text>();
                var button = contentTrans.GetChild(i).GetComponentInChildren<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(pmdata[i].OnClick);
                text.text = pmdata[i].ObjectName;
            }
            else
                contentTrans.GetChild(i).gameObject.SetActive(false);
        }
    }

	void Start () {
        SetCategory((int)PlaceableObjectCategory.Defense);
	}
}
