using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GroundSpawner : MonoBehaviour
{
    float[,] noiseValues;
    Tilemap tilemap;

    GameObject playerCharacter;

    [SerializeField] SpawnMob house;
    [SerializeField] SpawnMob badHouse;
    [SerializeField] int worldWidth;
    [SerializeField] int worldHeight;
    [SerializeField] Tile wall;
    [SerializeField] Tile[] wallTiles;
    [SerializeField] Tile ground;
    [SerializeField] Tilemap colliderMap;

    [SerializeField] int seed;
    [SerializeField] float freq;
    [SerializeField] float amplitude;
    [SerializeField] float lacunarity;
    [SerializeField] float persistence;
    [SerializeField] int octaves;
    [SerializeField] float timeToBlock;
    float groundNoiseValue = 0.4f;
    enum tileDirections
    {
        center = 0,
        bottomLeftCorner,
        left,
        bottom,
        right,
        topRightCorner,
        topLeftCorner,
        top,
        bottomRightCorner,
        rightFull,
        leftFull,
        topFull,
        bottomFull,
        topAndBottom,
        leftAndRight,
        fourCorners
    }
    float initTime;
    Noise noise;
    // Start is called before the first frame update
    void Start()
    {
        seed = Random.Range(0, 20000000);
        playerCharacter = GameObject.Find("char");
        tilemap = GetComponent<Tilemap>();
        noise = new Noise(seed,freq,amplitude,lacunarity,persistence,octaves);
        noiseValues = noise.GetNoiseValues(worldWidth, worldHeight);
        GenerateWorld(noiseValues, worldWidth, worldHeight);
        FixWallTiles();
    }
    void GenerateWorld(float[,] noiseValues,int worldWidth, int worldHeight)
    {
        Vector3Int spawnPos = new Vector3Int(0,0,0);
        for (int w = 0; w < worldWidth; w++)
        {
            for(int h = 0; h < worldHeight; h++)
            {
                spawnPos.x = w;
                spawnPos.y = h;
                if(Vector2.Distance(new Vector2(w,h),playerCharacter.transform.position) < 5)
                {
                    tilemap.SetTile(spawnPos, ground);
                }
                else if(noiseValues[w,h] < groundNoiseValue)
                {
                    tilemap.SetTile(spawnPos, ground);
                    if (Random.Range(0, 100) == 3 && w > 50)
                    {
                        createHouse(new Vector3(w+0.5f, h+0.5f, 0),house);
                    }
                    else if(noiseValues[w, h] < 0.3f &&  Random.Range(0,20) == 5 && w <= 50)
                    {
                        createHouse(new Vector3(w+0.5f, h+0.5f, 0), badHouse);
                    }
                }
                else
                {
                    colliderMap.SetTile(spawnPos, wall);

                }
            }
        }
    }
    void FixWallTiles()
    {
        Vector3Int spawnPos = new Vector3Int(0, 0, 0);
        //colliderMap.SetTile(spawnPos, wall);
        for (int w = 0; w < worldWidth; w++)
        {
            for (int h = 0; h < worldHeight; h++)
            {
                spawnPos.x = w;
                spawnPos.y = h;
                artWallTile(spawnPos);
            }
        }
    }
    public void artWallTile(Vector3Int spawnPos)
    {
        Vector3Int leftOffset = new Vector3Int(-1, 0, 0);
        Vector3Int rightOffset = new Vector3Int(1, 0, 0);
        Vector3Int topOffset = new Vector3Int(0, 1, 0);
        Vector3Int bottomOffset = new Vector3Int(0, -1, 0);


        if (colliderMap.GetTile(spawnPos) != null && wallTiles.Contains(colliderMap.GetTile(spawnPos)))
        {
            //all four empty
            if (colliderMap.GetTile(spawnPos + rightOffset) == null && colliderMap.GetTile(spawnPos + topOffset) == null && colliderMap.GetTile(spawnPos + bottomOffset) == null && colliderMap.GetTile(spawnPos + leftOffset) == null)
            {
                Debug.Log(colliderMap.GetTile(spawnPos).GetType());
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.fourCorners]);

            }
            //Full right
            else if (colliderMap.GetTile(spawnPos + rightOffset) == null && colliderMap.GetTile(spawnPos + topOffset) == null && colliderMap.GetTile(spawnPos + bottomOffset) == null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.rightFull]);
            }
            //Full Left
            else if (colliderMap.GetTile(spawnPos + leftOffset) == null && colliderMap.GetTile(spawnPos + topOffset) == null && colliderMap.GetTile(spawnPos + bottomOffset) == null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.leftFull]);
            }
            //Full bottom
            else if (colliderMap.GetTile(spawnPos + bottomOffset) == null && colliderMap.GetTile(spawnPos + leftOffset) == null && colliderMap.GetTile(spawnPos + rightOffset) == null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.bottomFull]);
            }
            //Full top
            else if (colliderMap.GetTile(spawnPos + topOffset) == null && colliderMap.GetTile(spawnPos + leftOffset) == null && colliderMap.GetTile(spawnPos + rightOffset) == null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.topFull]);
            }
            //Top and bottom empty
            else if (colliderMap.GetTile(spawnPos + topOffset) == null && colliderMap.GetTile(spawnPos + bottomOffset) == null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.topAndBottom]);
            }
            //Left and right empty
            else if(colliderMap.GetTile(spawnPos + leftOffset) == null && colliderMap.GetTile(spawnPos + rightOffset) == null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.leftAndRight]);
            }
            //Top right
            else if (colliderMap.GetTile(spawnPos + rightOffset) == null && colliderMap.GetTile(spawnPos + topOffset) == null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.topRightCorner]);
            }
            //Bottom right
            else if (colliderMap.GetTile(spawnPos + rightOffset)==null && colliderMap.GetTile(spawnPos + bottomOffset) == null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.bottomRightCorner]);
            }
            //Bottom left
            else if (colliderMap.GetTile(spawnPos + bottomOffset)==null && colliderMap.GetTile(spawnPos + leftOffset) == null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.bottomLeftCorner]);
            }
            //Top left
            else if (colliderMap.GetTile(spawnPos + topOffset)==null && colliderMap.GetTile(spawnPos + leftOffset)==null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.topLeftCorner]);
            }
            //Right
            else if (colliderMap.GetTile(spawnPos + rightOffset)==null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.right]);
            }
            //Left
            else if (colliderMap.GetTile(spawnPos + leftOffset)==null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.left]);
            }
            //Top
            else if (colliderMap.GetTile(spawnPos + topOffset)==null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.top]);
            }
            //Bottom
            else if (colliderMap.GetTile(spawnPos + bottomOffset)==null)
            {
                colliderMap.SetTile(spawnPos, wallTiles[(int)tileDirections.bottom]);
            }
        }
    }
    void createHouse(Vector3 pos,SpawnMob house)
    {
        SpawnMob h = Instantiate(house,pos,Quaternion.identity);
        h.collidable = colliderMap;
        h.ground = tilemap;

    }
    // Update is called once per frame
    void Update()
    {


    }
    void scrap()
    {
        timeToBlock -= Time.deltaTime;
        if (timeToBlock <= 0f)
        {
            int locX = Random.Range(0, 16);
            int locY = Random.Range(0, 10);
            Vector3Int tilePos = new Vector3Int(locX, locY, 0);
            tilemap.SetTile(tilePos, ground);
            if (colliderMap.GetTile(tilePos) == null)
            {
                Debug.Log("The tile is empty");
            } else
            {
                colliderMap.SetTile(tilePos, null);
            }
            timeToBlock = initTime;
        } 
    }
}
