// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class StateSerializer : MonoBehaviour {

    public static StateSerializer current;

    List<GameStateSave> _loadedSaves;

    Player _player;

    void Start()
    {
        current = this;
        _player = FindObjectOfType<Player>();
    }

    public void LoadSavegames()
    {
        _loadedSaves = new List<GameStateSave>();
        var files = Directory.GetFiles(Application.persistentDataPath, "*.narc");

        foreach(var fi in files)
        {
            var loaded = LoadSave(fi);
            if(loaded != null)
                _loadedSaves.Add(loaded);
        }
    }

    public ICollection<GameStateSave> GetSavegames()
    {
        return _loadedSaves;
    }

    public void SaveGame(string name)
    {
        var gamestate = new GameStateSave
        {
            Name = name,
            TimeCreated = System.DateTime.Now,
            PlayerState = current._player.GetState(),
            CurrentTime = GameTime.Time
        };

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/"+GetUniqueID()+".narc");
        bf.Serialize(file, gamestate);
        file.Close();
    }

    GameStateSave LoadSave(string filename)
    {
        string path = filename;
        //string path = Application.persistentDataPath + "/" + filename;
        try
        {
            if (File.Exists(path))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(path, FileMode.Open);
                GameStateSave state = (GameStateSave)bf.Deserialize(file);
                file.Close();

                //GameTime.Time = state.CurrentTime;
                //_player.SetState(state.PlayerState);
                return state;
            }
            else
                return null;
        }
        catch
        {
            return null;
        }
    }

    public void LoadGame(int id)
    {
        var state = _loadedSaves[id];
        GameTime.Time = state.CurrentTime;
        _player.SetState(state.PlayerState);
    }

    public static string GetUniqueID()
    {
        var random = new System.Random();
        string uniqueID =
                string.Format("{0:yyyy-MM-dd_hh-mm-ss}-", System.DateTime.Now)
        + string.Format("{0:X}", random.Next(1000000000));                            //random number
        return uniqueID;
    }
}

[System.Serializable]
public class GameStateSave
{
    // header
    public System.DateTime TimeCreated;
    public string Name;

    public System.Object PlayerState;
    public float CurrentTime;
}
