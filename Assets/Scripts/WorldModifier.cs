using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class WorldModifier : MonoBehaviour
{
    protected Rigidbody2D myRigidBody;
    [SerializeField] protected int clickRange;
    [SerializeField] protected Tilemap ground;
    [SerializeField] protected Tilemap collidable;
    [SerializeField] protected Tile breakTile;
    [SerializeField] protected GameTile wallTile;
    Defense curPlaceDefense;
    int curPlaceCost;
    Toggle activeButton;
    [SerializeField] Toggle defaultActiveButton;
    GameTile defaultWallTile;
    [SerializeField] protected int defaultTileHealth = 3;
    CoinCollector collector;
    protected int defaultPlaceTileHealth = 2;
    int currentPlaceTileHealth;
    GroundSpawner s;
    protected static Dictionary<Vector3Int, int> blockHealth = new Dictionary<Vector3Int, int>();
    // Start is called before the first frame update
    protected virtual void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        currentPlaceTileHealth = defaultPlaceTileHealth;
        collector = GetComponent<CoinCollector>();
        defaultWallTile = wallTile;
        curPlaceCost = 0;
        activeButton = defaultActiveButton;
        s = GameObject.Find("Tilemap").GetComponent<GroundSpawner>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            AttemptTileDestroy(clickPos);

        }
        if (Input.GetButtonDown("Fire2"))
        {
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            AttemptTilePlace(clickPos);

        }
    }
    void AttemptTilePlace(Vector3 placePos)
    {
        if(withinTileDistance(myRigidBody.position, clickRange, placePos) && !hasCollidable(placePos) && collector.getCoins() >= curPlaceCost)
        {
            collector.spendCoins(curPlaceCost);
            PlaceTile(placePos);
        }//If you cannot afford the tile, use the default one
        else if(collector.getCoins() < curPlaceCost)
        {
            ResetPlaceTile();
            AttemptTilePlace(placePos);
        }
    }
    void ResetPlaceTile()
    {
        Debug.Log("Resetting place tile");
        wallTile = defaultWallTile;
        currentPlaceTileHealth = defaultPlaceTileHealth;
        curPlaceCost = 0;
        activeButton.isOn = false;
        activeButton = defaultActiveButton;
        activeButton.isOn = true;
    }
    void PlaceTile(Vector3 clickPos)
    {
        Vector3Int tileLoc = new Vector3Int(Mathf.FloorToInt(clickPos.x), Mathf.FloorToInt(clickPos.y), 0);
        if (wallTile != null)
        {

            if (wallTile.isCollidable)
            {
                collidable.SetTile(tileLoc, wallTile);
                blockHealth[tileLoc] = currentPlaceTileHealth;
                //ground.SetTile(tileLoc, null);
            }
            else
            {
                ground.SetTile(tileLoc, wallTile);
            }
        }else
        {
            Vector3 placeLoc = new Vector3(Mathf.FloorToInt(clickPos.x)+0.5f, Mathf.FloorToInt(clickPos.y)+0.5f, 0);
            Defense spawner = Instantiate(curPlaceDefense, placeLoc, Quaternion.identity);
            blockHealth[tileLoc] = spawner.health;
        }
    }
    public void SetActiveButton(Toggle button)
    {
        //If changing button, disable the current button and switch
        if (button != activeButton)
        {

            activeButton.isOn = false;
            activeButton = button;
        }

    }
    public void SetActivePlace(GameTile tile)
    {
        curPlaceCost = tile.cost;
        currentPlaceTileHealth = tile.health;
        wallTile = tile;
    }
    public void SetActiveDefense(Defense def)
    {
        curPlaceDefense = def;
        curPlaceCost = def.cost;
        if (wallTile != null) wallTile = null;
    }
    public virtual void AttemptTileDestroy(Vector3 breakPos)
    {
        if (withinTileDistance(myRigidBody.position, clickRange, breakPos) && hasCollidable(breakPos))
        {
            BreakTile(breakPos);
        }
    }

    protected bool hasCollidable(Vector3 clickPos)
    {
        Vector3 adjustedPos = adjustBottomCornerToZero(clickPos);
        return collidable.GetTile(new Vector3Int(Mathf.FloorToInt(adjustedPos.x),Mathf.FloorToInt(adjustedPos.y),0)) != null;
    }
    bool withinTileDistance(Vector3 playerPos,int permittedDistance,Vector3 mousePos)
    {
        playerPos = adjustBottomCornerToZero(playerPos);
        mousePos = adjustBottomCornerToZero(mousePos);
        if(Vector2.Distance(playerPos,mousePos) <= permittedDistance)
        {

            return true;
        }
        else
        {
            return false;
        }
    }
    Vector3 adjustBottomCornerToZero(Vector3 unadjustedVector)
    {
        /*unadjustedVector.x += 8f;
        unadjustedVector.y += 5f;*/
        return unadjustedVector;
    }
    float roundToNearestHalf(float pos)
    {
        pos = Mathf.RoundToInt(pos * 2);
        pos /= 2;
        return pos;
    }
    public void BreakTile(Vector3 clickPos)
    {
        clickPos = adjustBottomCornerToZero(clickPos);
        Vector3Int tileLoc = new Vector3Int(Mathf.FloorToInt(clickPos.x), Mathf.FloorToInt(clickPos.y),0);
        collidable.SetTile(tileLoc, null);
        blockHealth[tileLoc] = 0;
        if (!ground.GetTile(tileLoc))
        {
            ground.SetTile(tileLoc, breakTile);
        }
        Vector3Int up = new Vector3Int(Mathf.FloorToInt(clickPos.x), Mathf.FloorToInt(clickPos.y)+1, 0);
        Vector3Int right = new Vector3Int(Mathf.FloorToInt(clickPos.x)+1, Mathf.FloorToInt(clickPos.y), 0);
        Vector3Int left = new Vector3Int(Mathf.FloorToInt(clickPos.x) - 1, Mathf.FloorToInt(clickPos.y), 0);
        Vector3Int bottom = new Vector3Int(Mathf.FloorToInt(clickPos.x), Mathf.FloorToInt(clickPos.y) - 1, 0);
        s.artWallTile(up);
        s.artWallTile(bottom);
        s.artWallTile(left);
        s.artWallTile(right);
    }
}
