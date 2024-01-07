// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StoreMenuElement : MonoBehaviour
{
    public Text Title;
    public Text Description;
    public Text Price;
    public Button Button;
    public Image Image;

    public void SetElementDataSource(ItemInStore value, UnityAction[] onClick)
    {
        Title.text = value.Name;
        Price.text = value.Price.ToString() + "$";
        Button.onClick.RemoveAllListeners();
        Image.sprite = value.Image;
        Description.text = value.Description;

        for(int i = 0; i<onClick.Length; i++)
        {
            Button.onClick.AddListener(onClick[i]);
        }
    }

    public void SetElementDataSource(StaffMember value, UnityAction[] onClick)
    {
        Title.text = value.JobTitle;
        // TODO: !!!!!
        Price.text = 0.ToString() + "$";
        Button.onClick.RemoveAllListeners();
        //Image.sprite = Sprite.Create(;
        Description.text = "";

        for (int i = 0; i < onClick.Length; i++)
        {
            Button.onClick.AddListener(onClick[i]);
        }
    }
}
