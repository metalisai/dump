using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class MapLoader
{
    public CustomTerrain Terrain;
    public GameMap LoadedMap;

    /// <summary>
    /// Serializes/Saves currently loaded map to Resources/Maps. (UnityEditor only)
    /// </summary>
    public void Serialize()
    {
        if(Terrain == null)
        {
            Debug.LogError("Tried to serialize a map, but no Terrain set!");
            return;
        }
        // TODO: change
        LoadedMap.InitialCameraPosition = new Vector3(LoadedMap.Size / 2, 10.0f, LoadedMap.Size / 2);
        LoadedMap.InitialCameraRotation = Quaternion.identity;
        string map = JsonUtility.ToJson(LoadedMap, true);
#if UNITY_EDITOR
        string path = "Assets/Resources/Maps/"+LoadedMap.Name+".json";
        string hmapPath = "Assets/Resources/Maps/" + LoadedMap.Name + "-hmap.bytes";
        string splatPath = "Assets/Resources/Maps/" + LoadedMap.Name + "-splat.bytes";

        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(map);
            }
        }

        using (FileStream fs = new FileStream(hmapPath, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                writer.Write(Terrain.GetHeightmapBytes());
            }
        }

        using (FileStream fs = new FileStream(splatPath, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                writer.Write(Terrain.GetSplatmapBytes());
            }
        }

        Debug.Log("Map saved!");
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    /// <summary>
    /// Called by Game on unload
    /// </summary>
    public void Unload()
    {
        Terrain = null;
        LoadedMap = null;
    }

    /// <summary>
    /// Loads a map by name from Resources/Maps
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool LoadMap(string name, bool isEditor = false)
    {
        var mapFile = Resources.Load<TextAsset>("Maps/"+name);
        var heightmapFile = Resources.Load<TextAsset>("Maps/" + name + "-hmap");
        var splatmapFile = Resources.Load<TextAsset>("Maps/" + name + "-splat");

        Debug.Log(" "+mapFile!=null+" "+heightmapFile);
        if (mapFile != null && heightmapFile != null && splatmapFile != null)
        {
            var mapText = mapFile as TextAsset;
            GameMap map = JsonUtility.FromJson<GameMap>(mapText.text);
            if (map != null)
            {
                Debug.LogFormat("Map '{0} loaded, size: {1}'", name, map.Size);
                if (!isEditor)
                {
                    Game.localPlayer.SetCameraPosition(map.InitialCameraPosition);
                    Game.localPlayer.SetCameraRotation(map.InitialCameraRotation);
                }
                return LoadMap(map, heightmapFile.bytes, splatmapFile.bytes, isEditor);
            }
            else
            {
                Debug.LogError("Map was null!");
                return false;
            }
        }
        else if (mapFile == null)
        {
            Debug.LogError("Map named <" + name + ">not found!");
            return false;
        }
        else if (heightmapFile == null)
        {
            Debug.LogError("Map named <" + name + "> doesn't have heighmap!");
            return false;
        }
        else
        {
            Debug.LogError("Map named <" + name + "> doesn't have splat!");
            return false;
        }
    }

    public GameMap CreateMap(int size, string name)
    {
        var map = new GameMap();
        map.Size = size;
        map.Name = name;

        if (LoadedMap != null)
            Game.Instance.UnloadMap();

        LoadedMap = map;

        //CustomTerrain customTerrain = CustomTerrain.Create(map.Size, 64);
        /*Game.Instance.RegisterDynamicObject(customTerrain.gameObject, false);
        Terrain = customTerrain;
        Game.customTerrain = customTerrain;
        customTerrain.gameObject.layer = 8;

        Game.pathFinding = new PathFinding(customTerrain);*/

        return map;
    }

    /// <summary>
    /// Loads a map from given map data
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    public bool LoadMap(GameMap map, byte[] heightMap, byte[] splatMap, bool isEditor = false)
    {
        if (map == null)
            return false;

        if(!isEditor)
        {
            if(LoadedMap != null)
                Game.Instance.UnloadMap();
        }
#if UNITY_EDITOR
        else
        {
            if(LoadedMap != null)
                EditorManager.I.UnloadMap();
        }
#endif

        LoadedMap = map;
        /*CustomTerrain customTerrain = CustomTerrain.Load(map.Size, heightMap, splatMap);
        if(!isEditor)
        {
            Game.Instance.RegisterDynamicObject(customTerrain.gameObject, false);
            Game.customTerrain = customTerrain;
            Game.pathFinding = new PathFinding(customTerrain);
        }
        customTerrain.gameObject.layer = 8;
        Terrain = customTerrain;*/

        /*foreach (var item in map.PlacedObjects)
        {
            Debug.LogFormat("Object placed at {0} {1}", item.Position.x, item.Position.y);
            Game.Instance.ItemPlacer.PlaceObject(item.ObjectID, item.Position, item.Rotation);
        }*/

        return true;
    }
}
