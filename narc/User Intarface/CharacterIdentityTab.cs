// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterIdentityTab : MonoBehaviour {

    public Text NameText;
    public Text SexText;
    public Text AgeText;
    public Text JobText;
    public Text HiredText;
    public Text BiographyText;
    public Text AssignedText;

    public void SetDataSource(StaffMember member)
    {
        var personData = member.PersonData;
        NameText.text = personData.FirstName + " " + member.PersonData.LastName;
        SexText.text = personData.Gender == Gender.Male ? "M" : "F";
        AgeText.text = personData.Age.ToString();
        JobText.text = member.JobTitle;
        // TODO: better scale
        HiredText.text = ((int)((GameTime.Time - member.HiredTime)/60f/24f)).ToString()+" days ago";
        BiographyText.text = personData.Biography;
        AssignedText.text = member.Apartment.name;
    }
}
