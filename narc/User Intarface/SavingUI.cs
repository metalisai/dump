// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.UI;

public class SavingUI : MonoBehaviour {

    public InputField saveName;
    public Dropdown saveList;

    StateSerializer _stateSer;

    void Start()
    {
        _stateSer = FindObjectOfType<StateSerializer>();
    }
    
    void Refresh()
    {
        if (_stateSer == null)
            _stateSer = FindObjectOfType<StateSerializer>();

        _stateSer.LoadSavegames();
        saveList.options.Clear();
        foreach (var save in _stateSer.GetSavegames())
        {
            Dropdown.OptionData option = new Dropdown.OptionData {text = save.Name};
            saveList.options.Add(option);
            saveList.captionText = saveList.captionText; // refresh hack thing (Unity 5.2.0f3)
        }
    }

	void OnEnable() {
        Refresh();
    }

    public void Save()
    {
        if(!string.IsNullOrEmpty(saveName.text))
            _stateSer.SaveGame(saveName.text);
        Refresh();
    }

    public void Load()
    {
        _stateSer.LoadGame(saveList.value);
        gameObject.SetActive(false);
    }
}
