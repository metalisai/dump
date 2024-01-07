using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


[System.Serializable]
public class GameMap
{
    [System.Serializable]
    public struct PlacedObject
    {
        public CellCoord Position;
        public int Rotation;
        public GameObjectID ObjectID;
    }

    public List<CellCoord> OccupiedCells = new List<CellCoord>();
    public List<PlacedObject> PlacedObjects = new List<PlacedObject>();
    public Vector3 InitialCameraPosition;
    public Quaternion InitialCameraRotation;
    public int Size;
    public string Name;

    PathFindingGrid grid;
}
