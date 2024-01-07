// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Player : MonoBehaviour, ISaveable
{
    public int Cash { get; set; }
    public int BankMoney { get; set; }

    private readonly List<Housing> _infrastructures = new List<Housing>(); 
    private readonly List<Business> _businesses = new List<Business>();

    public ICollection<WeedDryer> WeedDryers { get { return _infrastructures.SelectMany(x => x.Dryers).ToList(); }}
    public ICollection<Storage> Storages { get { return _infrastructures.SelectMany(x => x.Storages).ToList(); }}
    public ICollection<GrowingPlant> WeedPlants { get { return _infrastructures.SelectMany(x => x.Plants).ToList(); } }
    public ICollection<Housing> Infrastructures { get { return _infrastructures; } }

    public ICollection<Business> Businesses { get { return _businesses; } }

    public Housing ActiveInfrastructure { get; set; }

    // TODO: UI stuff doesn't really fit here...
    private UIController _ui;

    void Start()
    {
        ActiveInfrastructure = Infrastructures.FirstOrDefault();
        _ui = FindObjectOfType<UIController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            _ui.ToggleActive();
        }
    }

    public void SellWeed(int amount)
    {
        Storage storage = Storages.FirstOrDefault(x => x.State != StorageState.Empty);
        if (storage != null)
        {
            storage.AddProduct(-amount);
            Cash += 800;
        }
        else
        {
            Debug.Log("You dont have anything to sell!");
        }
    }

    public void AddInfrastructure(Housing infrastructure)
    {

        Infrastructures.Add(infrastructure);
        infrastructure.Owner = this;
    }

    public System.Object GetState()
    {
        var state = new PlayerSave
        {
            Cash = Cash,
            BankMoney = BankMoney
        };

        foreach(var apartment in Infrastructures)
        {
            state.Apartments.Add(apartment.GetState());
        }

        return state;
    }

    public void SetState(System.Object state)
    {
        var data = state as PlayerSave;

        Cash = data.Cash;
        BankMoney = data.BankMoney;

        var infs = FindObjectsOfType<Housing>();

        // load apartments
        foreach (var savedInf in data.Apartments)
        {
            bool found = false;
            foreach (var comp in infs)
            {
                if (comp.Id == savedInf.Id)
                {
                    comp.SetState(savedInf);
                    Debug.Log("Loaded apartment " + savedInf.Id);
                    found = true;
                    break;
                }
            }
            if (!found)
                Debug.LogError("Save file contained an unknown apartment!");
        }
    }
}

[System.Serializable]
public class PlayerSave
{
    public int Cash;
    public int BankMoney;

    public List<ApartmentSave> Apartments = new List<ApartmentSave>();
}
