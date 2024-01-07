// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Linq;
using System;

public class Configuration : MonoBehaviour {

    [System.Serializable]
    class JsonBusiness
    {
        public string BusinessName;
        public int IncomePerPayday;
        public int ExpenditurePerPayday;
        public int MaxLaundering;
        public string Description;
        public int Cost;
        public int PaymentType;
    }

    [System.Serializable]
    class JsonStoreItem
    {
        public string ItemName;
        public int Price;
        public string Description;
    }

    [System.Serializable]
    class ConfFormat
    {
        public int StartingMoney;
        public float PlantStageTime;
        public float PlantDryingTime;
        public List<JsonBusiness> Businesses = new List<JsonBusiness>();
        public List<JsonStoreItem> StoreItems = new List<JsonStoreItem>();
    }

    public GrowingPlant PlantReferece;

	// Use this for initialization
	void Start () {
        Load();
	}

    public void SaveCurrent()
    {
#if UNITY_EDITOR
        ConfFormat conf = new ConfFormat();
        conf.StartingMoney = FindObjectOfType<Player>().Cash;

        conf.PlantStageTime = GrowingPlant.StageTime;
        conf.PlantDryingTime = WeedDryer.DryingTime;

        conf.Businesses = FindObjectsOfType<Business>().ToList().Select(x => new JsonBusiness {
            BusinessName = x.BusinessName,
            Cost = x.Cost,
            Description = x.Description,
            IncomePerPayday = x.IncomePerPayday,
            MaxLaundering = x.MaxLaundering,
            PaymentType = (int)x.PaymentType,
            ExpenditurePerPayday = x.ExpenditurePerPayday
        }).ToList();

        conf.StoreItems = FindObjectOfType<Store>().StoreItems.ToList().Select(x => new JsonStoreItem
        {
            ItemName = x.Name,
            Price = x.Price,
            Description = x.Description
        }).ToList();

        using (var fstream = File.Open("Assets/Resources/Configuration.json", FileMode.OpenOrCreate))
        {
            var content = JsonUtility.ToJson(conf,true);
            var bytes = new UTF8Encoding(true).GetBytes(content);
            fstream.Write(bytes,0,bytes.Length);
        }
#endif
    }

    public void Load()
    {
        var asset = Resources.Load<TextAsset>("Configuration");
        if (asset != null)
        {
            ConfFormat conf;
            try
            {
                conf = JsonUtility.FromJson<ConfFormat>(asset.text);
            }
            catch (Exception)
            {
                Debug.LogError("Loading configuration failed");
                return;
            }

            // TODO: all this is very slow

            FindObjectOfType<Player>().Cash = conf.StartingMoney;

            GrowingPlant.StageTime = conf.PlantStageTime;
            WeedDryer.DryingTime = conf.PlantDryingTime;

            var businesses = FindObjectsOfType<Business>();
            foreach(var business in businesses)
            {
                var src = conf.Businesses.FirstOrDefault(x => x.BusinessName == business.BusinessName);
                business.MaxLaundering = src.MaxLaundering;
                business.Cost =                 src.Cost;
                business.Description =          src.Description;
                business.IncomePerPayday =        src.IncomePerPayday;
                business.MaxLaundering =        src.MaxLaundering;
                business.PaymentType =          (BusinessBuyingPaymentType)src.PaymentType;
                business.ExpenditurePerPayday = src.ExpenditurePerPayday;
            }

            var storeItems = FindObjectOfType<Store>().StoreItems;
            foreach(var item in storeItems)
            {
                var src = conf.StoreItems.FirstOrDefault(x => x.ItemName == item.Name);
                item.Price = src.Price;
                item.Description = src.Description;
            }
        }
        else
        {
            Debug.LogWarning("No configuration file found! (Resources/Configuration.json)");
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
