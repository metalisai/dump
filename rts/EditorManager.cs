using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class EditorManager
{
    public static EditorManager I;
    MapLoader _mapLoader;

    public EditorManager()
    {
        I = this;
        _mapLoader = new MapLoader();
    }

    public void LoadMap(string mapName)
    {
        _mapLoader.LoadMap(mapName, true);
    }

    public void UnloadMap()
    {

    }
}
