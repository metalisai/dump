// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Housing : MonoBehaviour
{

    public int Id = 0;
    public int Price = 100000;
    public int MaxPower = 1000;

    // TODO: not very performant
    public int PowerConsumption
    {
        get { return PowerConsumers.Values.Sum(); }
    }

    Dictionary<System.Object, int> PowerConsumers = new Dictionary<System.Object, int>();

    public List<WeedDryer> Dryers = new List<WeedDryer>();
    public readonly List<Storage> Storages = new List<Storage>();
    public readonly List<GrowingPlant> Plants = new List<GrowingPlant>();

    public List<PlaceableObject> PObjects = new List<PlaceableObject>();
    // placeable objects that are outside of the infrastructure itself (dealers)
    public List<PlaceableObject> ExtPObjects = new List<PlaceableObject>();

    public List<Dealer> Dealers = new List<Dealer>();
    public List<GardenerAI> Gardeners = new List<GardenerAI>();

    public Grid[] Grids { get; set; }

    public GardenerObserver GardenerObserver;

    int _storedGrams = 0;

    private Player _owner;

    public Player Owner {
        get
        {
            return _owner;
        }
        set
        {
            _owner = value;
            foreach(var grid in Grids)
            {
                grid.gameObject.SetActive(true);
            }
        }
      }

    public bool Buy(Player player)
    {
        if(player.BankMoney >= Price)
        {
            player.BankMoney -= Price;
            player.AddInfrastructure(this);
            return true;
        }
        return false;
    }

    void Awake()
    {
        GardenerObserver = new GardenerObserver(this);
        RegisterGrids();
        Dryers.ForEach(x => x.Infrastructure = this);
    }

    void Start()
    {
        foreach(var deal in FindObjectsOfType<Dealer>())
        {
            Dealers.Add(deal);
        }
    }

    private void RegisterGrids()
    {
        Component[] components = gameObject.GetComponentsInChildren(typeof(Grid)); // find all grid components in children
        Grids = new Grid[components.Length]; // allocate memory for the pointers 
        Debug.Log("Registered " + components.Length + "grids");

        int i = 0;
        foreach (var component in components) // loop through all grids
        {
            Grid grid = (Grid)component;
            Grids[i] = grid;
            grid.Infrastructure = this; // set grid's infrastructure this Infrastructure

            if(Owner == null)
            {
                grid.gameObject.SetActive(false);
            }

            i++;
        }

        if (Grids == null || Grids.Length == 0)
            Debug.LogError("Infrastructure should have a Grid attached!");
    }

    public bool AddPowerConsumer(System.Object obj, int power)
    {
        if (PowerConsumers.Values.Sum(x => x) + power <= MaxPower)
        {
            PowerConsumers.Add(obj, power);
            return true;
        }
        return false;
    }

    public void RemovePowerConsumer(System.Object obj)
    {
        bool rem = PowerConsumers.Remove(obj);
        Debug.Assert(rem);
    }

    public PlaceableObject AddObjectToPosition(PlaceableObjectType objectType, Vector3 position, GridShapeRotation rotation = GridShapeRotation.rot0)
    {
        var pObject = ObjectRegister.CreatePlacableObject(objectType);

        // anything that is attached to a grid cell
        if (pObject.InGrid)
        {
            var grid = Grids.FirstOrDefault(x => x.IsInGrid(position, pObject.GridType));
            pObject.SetRotation(rotation);
            if (pObject.Fits(grid, position))
            {
                pObject.Place(grid, position);
            }
            else
            {
                Destroy(pObject);
                pObject = null;
                // This should never occur, if it does then you need to check if object fits before you call this function!
                // This check is here only to catch bugs!
                Debug.LogError("Tried to put object where it would not fit!");
                return null;
            }
        }
        // stuff thats not attached to grid cells (staff)
        else
        {
            var grid = Grids.FirstOrDefault(x => x.IsInGrid(position, pObject.GridType));
            pObject.gameObject.transform.position = position;
            pObject.Infrastructure = grid.Infrastructure;
        }

        pObject.gameObject.SetActive(true);
        SearchComponents(pObject.gameObject);
        PObjects.Add(pObject);

        return pObject;
    }

    public void SearchComponents(GameObject gObject)
    {
        Component[] components = gObject.GetComponentsInChildren<Component>();

        GrowingPlant plant = (GrowingPlant)components.FirstOrDefault(x => x.GetType() == typeof(GrowingPlant));
        if (plant != null)
        {
            Plants.Add(plant);
            Debug.Log("found plant");
        }

        Storage storage = (Storage)components.FirstOrDefault(x => x.GetType() == typeof(Storage));
        if (storage != null)
        {
            Storages.Add(storage);
            Debug.Log("found storage");
        }

        GardenerAI gai = (GardenerAI)components.FirstOrDefault(x => x.GetType() == typeof(GardenerAI));
        if (gai != null)
        {
            Gardeners.Add(gai);
            Debug.Log("found gardener");
        }

        Dealer deal = (Dealer)components.FirstOrDefault(x => x.GetType() == typeof(Dealer));
        if (deal != null)
        {
            Dealers.Add(deal);
            Debug.Log("found dealer");
        }
    }

    public PlaceableObject AddObjectToCell(PlaceableObjectType objectType, GridCell cell)
    {
        return AddObjectToPosition(objectType, cell.Center);
    }

    public void PlaceableObjectDestroyed(PlaceableObject pobj)
    {
        Debug.Log("Placeable object destroyed! " + pobj.ObjectType);
        switch (pobj.ObjectType)
        {
            case PlaceableObjectType.WeedPlant:
                GrowingPlant plant = pobj.GetComponentsInChildren<GrowingPlant>(true).FirstOrDefault();
                if (plant != null)
                {
                    bool success = Plants.Remove(plant);
                    if (!success)
                        Debug.LogError("A plant was removed but something went wrong when trying to remove reference to it!");
                    Debug.Log("Removed a plant from apartment! ");
                }
                break;
            case PlaceableObjectType.Storage:
                Storage sto = pobj.GetComponentsInChildren<Storage>(true).FirstOrDefault();
                if(sto != null)
                {
                    bool success = Storages.Remove(sto);
                    if (!success)
                        Debug.LogError("A storage was removed but something went wrong when trying to remove reference to it!");
                }
                break;
            case PlaceableObjectType.Dealer:
                int count = ExtPObjects.Count;
                ExtPObjects.Remove(pobj);
                Debug.Assert(ExtPObjects.Count < count);
                break;
        }
        PObjects.Remove(pobj);
    }

    public int GetWeedAmount()
    {
        return Storages.Sum(x => x.AmountStored) * 200 + _storedGrams;
    }

    public bool AddWeed(int grams)
    {
        int gramsPerBlock = 200;

        _storedGrams += grams;

        int blocks = _storedGrams / gramsPerBlock;

        if (blocks >= 1)
        {
            for (int i = 0; i < blocks; i++) // I use loop beacause then its guaranteed that if more than 1 block comes in, all get assigned to storage with enough room
            {
                // TODO: also revert addproduct
                Storage storage = Storages.FirstOrDefault(x => x.State != StorageState.Full);
                if (storage != null)
                    storage.AddProduct(1);

                else // we couldnt store it
                {
                    _storedGrams -= grams; // revert to old value
                    return false;
                }

                _storedGrams -= gramsPerBlock;
            }
        }
        return true;
    }

    void Reset()
    {
        _storedGrams = 0;
        PObjects.ForEach(x => Destroy(x));
        ExtPObjects.ForEach(x => Destroy(x));
        PObjects = new List<PlaceableObject>();
        ExtPObjects = new List<PlaceableObject>();
    }

    public ApartmentSave GetState()
    {
        ApartmentSave state = new ApartmentSave
        {
            Id = Id,
            StoredWeed = _storedGrams
        };
        Dryers.ForEach(x => state.Dryers.Add(x.GetState()));
        foreach(var plobject in PObjects)
        {
            state.PObjects.Add(plobject.GetPobjState());
            Debug.Log("Serialized placeable object.");
        }
        ExtPObjects.ForEach(x => state.ExtPObjects.Add(x.GetPobjState()));
        return state;
    }

    public void SetState(ApartmentSave state)
    {
        Reset();

        _storedGrams = state.StoredWeed;
        // Dryers are currently static
        if (state.Dryers.Count != Dryers.Count)
        {
            Debug.LogError("Loaded apartment had different dryer count!");
        }
        else
        {
            for (int i = 0; i < state.Dryers.Count; i++)
            {
                Dryers[i].SetState(state.Dryers[i]);
            }

            for (int i = 0; i < state.PObjects.Count; i++)
            {
                var cur = state.PObjects[i];
                Vector3 pos;
                pos.x = cur.PosX;
                pos.y = cur.PosY;
                pos.z = cur.PosZ;

                var pobj = AddObjectToPosition(cur.Type, pos, cur.Rotation);
                ISaveable sav = pobj.GetComponentInChildren<ISaveable>();
                if (sav != null)
                {
                    sav.SetState(cur.State);
                }
                else
                {
                    Debug.LogWarning("Stateless object in savegame! " + pobj.name);
                }

                Debug.Log("Added "+cur.Type);
            }

            // external objects
            // TODO: this code should be compressed somewhere else (duplicate of ItemPlacer.Place)
            for (int i = 0; i < state.ExtPObjects.Count; i++)
            {
                var cur = state.ExtPObjects[i];
                Vector3 pos;
                pos.x = cur.PosX;
                pos.y = cur.PosY;
                pos.z = cur.PosZ;

                PlaceableObject pobj = ObjectRegister.CreatePlacableObject(cur.Type);
                if (!pobj.InGrid)
                    Debug.LogError("External pobj without grid not supported!");

                pobj.gameObject.SetActive(true);

                pobj.SetRotation(cur.Rotation);
                // TODO: this might be slow? do we care?
                var grid = FindObjectsOfType<Grid>().FirstOrDefault(x => x.IsInGrid(pos, pobj.GridType));
                Debug.Assert(grid != null);

                if (pobj.Fits(grid, pos))
                {
                    pobj.Place(grid, pos);
                }
                else
                {
                    Destroy(pobj);
                    // This should never occur, if it does then you need to check if object fits before you call this function!
                    // This check is here only to catch bugs!
                    Debug.LogError("Tried to put object where it would not fit!");
                    break;
                };

                ISaveable sav = pobj.GetComponentInChildren<ISaveable>();
                if (sav != null)
                {
                    sav.SetState(cur.State);
                }
                else
                {
                    Debug.LogWarning("Stateless object in savegame! " + pobj.name);
                }
                ExtPObjects.Add(pobj);
                pobj.Infrastructure = this;

                Debug.Log("Added " + cur.Type);
            }
        }
    }
}

[System.Serializable]
public class ApartmentSave
{
    public int Id;
    public int StoredWeed;
    public bool Owned;
    public List<WeedDryerSave> Dryers = new List<WeedDryerSave>();
    public List<PlaceableObjectSave> PObjects = new List<PlaceableObjectSave>();
    public List<PlaceableObjectSave> ExtPObjects = new List<PlaceableObjectSave>();
}
