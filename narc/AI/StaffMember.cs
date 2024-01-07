// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StaffMember : MonoBehaviour, IItemSelectHandler {

    [HideInInspector]
    public BaseAI AI;
    [HideInInspector]
    public PersonData PersonData;

    private float _hiredTime;
    public float HiredTime { get { return _hiredTime; } }
    [HideInInspector]
    public Housing Apartment;
    public string JobTitle = "";

    CharacterWindow _charWindow;

    private static List<string> _names;

    public int DailySalary;

    void Awake()
    {
        AI = GetComponentInChildren<BaseAI>();
        if(AI == null)
        {
            Debug.LogWarning("Staff member "+name+" doesn't have BaseAI component!");
        }

        // Note: only works for placed objects, since they will have no parent
        Apartment = transform.root.gameObject.GetComponentInChildren<PlaceableObject>().Infrastructure;

        _charWindow = FindObjectOfType<CharacterWindow>();

        // TODO: this is shitty
        if (_names == null)
        {
            TextAsset namesFile = Resources.Load("CharacterNames") as TextAsset;
            _names = namesFile.text.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        _hiredTime = GameTime.Time;

        GenerateIdentity();
    }

    void Start()
    {
        if (Apartment == null)
        {
            Debug.LogWarning("StaffMember.Apartment should not be null!");
        }
    }


    void GenerateIdentity()
    {
        PersonData.Age = Random.Range(18, 40);
        PersonData.Gender = Gender.Male;
        PersonData.Biography = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris. Fusce nec tellus sed augue semper porta. Mauris massa. Vestibulum lacinia arcu eget nulla. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Curabitur sodales ligula in libero. Sed dignissim lacinia nunc. Curabitur tortor. Pellentesque nibh. Aenean quam. In scelerisque sem at dolor.";

        string name = GenerateName();

        PersonData.FirstName = name.Split(' ')[0];
        PersonData.LastName = name.Split(' ')[1];
    }

    public bool OnSelected()
    {
        _charWindow.Show(this);
        return true;
    }

    public string GenerateName()
    {
        int index = Random.Range(0, _names.Count);
        string name = _names[index];
        //_names.RemoveAt(index);
        return name;
    }

    public void OnDeSelecded()
    {
        _charWindow.Hide();
    }

    public void StartPlacing()
    {
        var plObj = this.GetComponent<PlaceableObject>();
        if(plObj != null)
            ItemPlacer.Current.StartPlacing(plObj.ObjectType, PlacerEvent);
    }

    public void PlacerEvent(PlacerEventId eventId)
    {
        switch(eventId)
        {
            case PlacerEventId.OnTryingPlace:
                // TODO: potato
                var shire = FindObjectOfType<StaffHire>();
                if (shire.PeekItem(shire.ByStaffMember(this)))
                {
                    shire.BuyItem(shire.ByStaffMember(this));
                    ItemPlacer.Current.Place();
                }
                break;
            default:
                break;
        }
    }
}

public struct PersonData
{
    public string FirstName;
    public string LastName;
    public Gender Gender;
    public int Age;
    public string Biography;
}

public enum Gender
{
    Male,
    Female
}