using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnMob : MonoBehaviour
{
    [SerializeField] MobAI friendly;
    [SerializeField] int spawnDist = 15;
    public Tilemap ground;
    public Tilemap collidable;
    GameObject player;
    int spawnCount = 3;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("char");
        StartCoroutine(ActualSpawn(spawnCount)); ;
        InvokeRepeating("SpawnNPC", 30f, 30f);
    }

    // Update is called once per frame
    void SpawnNPC()
    {
        if(playerInProximity())
        {
            StartCoroutine(ActualSpawn(spawnCount));
        }
    }
    IEnumerator ActualSpawn(int num)
    {

        for (int i = 0; i < num; i++)
        {
            yield return new WaitForSeconds(1.0f);
            MobAI mob = Instantiate(friendly, transform.position, Quaternion.identity);
            mob.ground = ground;
            mob.collidable = collidable;
        }
        yield return null;
    }
    bool playerInProximity()
    {
        if(Vector2.Distance(player.transform.position,transform.position) < spawnDist)
        {
            return true;
        }
        return false;
    }
}

