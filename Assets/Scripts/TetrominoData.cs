using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

//added my piece the U
public enum Tetronimo { I, O, T, J, L, S, Z , U }

[Serializable]
public struct TetronimoData
{
    public Tetronimo tetronimo;
    public Vector2Int[] cells;
    public Tile tile;

}