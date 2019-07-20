using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName ="New WallTile",menuName ="Tiles/tile")]
public class GameTile : Tile
{

    public int health;
    public int cost;
    public bool isCollidable;
}
