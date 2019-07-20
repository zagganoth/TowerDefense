using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AIWorldModifier : WorldModifier
{

    protected override void Start()
    {
        base.Start();
        collidable = GameObject.Find("Collidable").GetComponent<Tilemap>();
        ground = GameObject.Find("Tilemap").GetComponent<Tilemap>();
    }
    public int GetBlockHealthAtTile(Vector3Int tile)
    {
        return blockHealth.ContainsKey(tile) ? blockHealth[tile] : defaultPlaceTileHealth;
    }
    public override void AttemptTileDestroy(Vector3 breakPos)
    {
        Vector3Int sanitizedPos = new Vector3Int(Mathf.FloorToInt(breakPos.x), Mathf.FloorToInt(breakPos.y),0);
        if(hasCollidable(breakPos))
        {
            //Debug.Log("Attempting destroy at " + breakPos);
            if (!blockHealth.ContainsKey(sanitizedPos))
            {
                blockHealth[sanitizedPos] = defaultTileHealth;
            }
            else
            {
                blockHealth[sanitizedPos]--;
                //Debug.Log("Doing damage at " + breakPos + " current health is " + blockHealth[sanitizedPos]);
            }
            if(blockHealth[sanitizedPos]<=0)
            {
                BreakTile(breakPos);
            }
        }
    }
    private void Update()
    {
        
    }

}
