// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class GridShape
{
    public bool[,] Shape;
    public GridShapeRotation Rotation = 0;
    public string ShapeString = "1";
    public GridEffect[] Effects = new GridEffect[0];

    public List<GridCell> Cells = new List<GridCell>();
    public List<GridCell> AffectedCells = new List<GridCell>();

    public GridShape(string shapestring)
    {
        string[] rows = shapestring.Split(' ');
        string longest = rows.OrderByDescending(s => s.Length).First();
        Shape = new bool[rows.Length, longest.Length];
        for (int i = 0; i < Shape.GetLength(0); i++)
        {
            for (int j = 0; j < Shape.GetLength(1); j++)
            {
                if (j < rows[i].Length && rows[i].ElementAt(j) == '1')
                    Shape[i, j] = true;
            }
        }
        ShapeString = shapestring;
    }

    public void RotateLeft()
    {
        Shape = rotateMatrixLeft(Shape);
        int rot = (int)Rotation;
        rot += 1;
        rot %= 4;
        Rotation = (GridShapeRotation)rot;
    }

    public void SetRotation(GridShapeRotation rot)
    {
        if ((int)rot >= 4)
            Debug.LogError("Wrong Rotation parameter!");

        while (Rotation != rot)
        { // yes it is a bad way..but performance isnt important here
            RotateLeft();
        }
    }

    public GridShape Clone()
    {
        var gs = new GridShape(this.ShapeString);
        gs.SetRotation(this.Rotation);
        return gs;
    }

    private bool[,] rotateMatrixLeft(bool[,] matrix)
    {
        /* W and H are already swapped */
        int w = matrix.GetLength(0);
        int h = matrix.GetLength(1);
        bool[,] ret = new bool[h, w];
        for (int i = 0; i < h; ++i)
        {
            for (int j = 0; j < w; ++j)
            {
                ret[i, j] = matrix[j, h - i - 1];
            }
        }
        return ret;
    }
}

public enum GridShapeRotation
{
    rot0,
    rot90,
    rot180,
    rot270
}
