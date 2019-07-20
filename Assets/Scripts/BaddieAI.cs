using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Priority_Queue;
using System.Collections;

public class BaddieAI : MobAI
{
    GameObject playerCharacter;
    IList<Vector2> path;
    AIWorldModifier mod;
    [SerializeField] GameObject lootReward;
    int pathIndex;
    private bool destroyCooldownActive;
    private bool shouldIMove;
    private bool colliding;
    private int pathFindOrder;
    private bool updatingOrder;
    private Vector2 lastTarget;
    private float baseSpeed = 3.5f;
    IDictionary<Vector2, Vector2> nodeParents = new Dictionary<Vector2, Vector2>();
    IDictionary<Vector2, Vector2> Parents = new Dictionary<Vector2, Vector2>();
    Vector2 spawnPos;
    int damageLevel = 1;
    int health = 2;
    // Start is called before the first frame update
    void Start()
    {
        playerCharacter = GameObject.Find("char");
        path = new List<Vector2>();
        updatePath();
        InvokeRepeating("updatePath", 5f, 1f);
        mod = GetComponent<AIWorldModifier>();
        updatingOrder = false;
        destroyCooldownActive = false;
        shouldIMove = true;
        spawnPos = transform.position;
        lastTarget = spawnPos;
        pathFindOrder = Random.Range(0, 24);
        InvokeRepeating("shufflePathOrder", 25f, 25f);
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldIMove)
        {
            followPath();
        }
        else
        {
            retracePath();
        }
    }
    void updatePath()
    {
        if (Vector2.Distance(transform.position, playerCharacter.gameObject.transform.position) < 20f)
        {

            Vector2 curLoc = transform.position;

            Vector2 playerLoc;
            //If I just hit another enemy, back off to the spawn, otherwise pursue the player
            if (shouldIMove)
            {
                playerLoc = playerCharacter.gameObject.transform.position;
            }
            else
            {
                playerLoc = spawnPos;
            }
            if (!nodeParents.ContainsKey(playerLoc))
            {
                //dictionary for heuristic score + distance and raw distance from origin
                Dictionary<Vector2, int> AStarScore = new Dictionary<Vector2, int>();
                Dictionary<Vector2, int> djikstraDist = new Dictionary<Vector2, int>();

                AStarScore[curLoc] = EuclideanEstimate(curLoc, playerLoc);
                djikstraDist[curLoc] = 0;


                SimplePriorityQueue<Vector2, int> priorityQueue = new SimplePriorityQueue<Vector2, int>();
                priorityQueue.Enqueue(curLoc, AStarScore[curLoc]);
                int neighborsExplored = 0;
                while (priorityQueue.Count > 0)
                {
                    Vector2 curr = priorityQueue.Dequeue();
                    //If at destination, stop
                    if (new Vector2Int(Mathf.FloorToInt(curr.x), Mathf.FloorToInt(curr.y)) == new Vector2Int(Mathf.FloorToInt(playerLoc.x), Mathf.FloorToInt(playerLoc.y)))
                    {
                        break;
                    }
                    List<(Vector2, int)> neighbors = GetWalkableNeighbors(curr);
                    foreach (var tup in neighbors)
                    {
                        neighborsExplored++;
                        Vector2 node = tup.Item1;
                        if (EuclideanEstimate(curr, node) < AStarScore[curr])
                        {
                            int weight = tup.Item2;
                            int currScore = djikstraDist[curr] + weight;
                            if (!djikstraDist.ContainsKey(node))
                            {
                                djikstraDist[node] = int.MaxValue;
                            }
                            if (!AStarScore.ContainsKey(node))
                            {
                                AStarScore[node] = int.MaxValue;
                            }
                            if (currScore < djikstraDist[node])
                            {

                                nodeParents[node] = curr;
                                djikstraDist[node] = currScore;

                                int hScore = djikstraDist[node] + EuclideanEstimate(node, playerLoc);
                                AStarScore[node] = hScore;

                                if (!priorityQueue.Contains(node))
                                {
                                    priorityQueue.Enqueue(node, hScore);
                                }
                            }


                        }
                        if (neighborsExplored > 350)
                        {
                            return;
                        }
                    }
                }
            }
            Vector2 newLoc = new Vector2(Mathf.FloorToInt(playerLoc.x) + 0.5f, Mathf.FloorToInt(playerLoc.y) + 0.5f);
            if (nodeParents.ContainsKey(newLoc))
            {
                Vector2 curr1 = newLoc;
                Vector2 finalLoc = curLoc;//new Vector2(Mathf.FloorToInt(curLoc.x), Mathf.FloorToInt(curLoc.y));
                int pathLength = 0;
                while (EuclideanEstimate(curr1, finalLoc) > 0)
                {
                    pathLength++;
                    if (pathLength >= 300) break;
                    path.Add(curr1);
                    if (nodeParents.ContainsKey(curr1))
                    {
                        curr1 = nodeParents[curr1];
                    }
                    else
                    {
                        break;
                    }
                }
                pathIndex = path.Count - 1;
            }
            else
            {

            }
        }
    }
    void shufflePathOrder()
    {
        if (!updatingOrder)
        {
            updatingOrder = true;
            pathFindOrder = Random.Range(0, 24);
            updatingOrder = false;
        }
    }
    public void spawnCoin()
    {
        Instantiate(lootReward, transform.position, Quaternion.identity);
    }
    List<(Vector2, int)> GetWalkableNeighbors(Vector2 curr)
    {
        List<(Vector2,int)> neighbors = new List<(Vector2,int)>();
        List<Vector3Int> surroundingTiles = new List<Vector3Int>();
        //Left
        Vector3Int left = new Vector3Int(Mathf.FloorToInt(curr.x) - 1, Mathf.FloorToInt(curr.y), 0);
        //Right
        Vector3Int right =  new Vector3Int(Mathf.FloorToInt(curr.x) + 1, Mathf.FloorToInt(curr.y), 0);
        //Up
        Vector3Int up =  new Vector3Int(Mathf.FloorToInt(curr.x), Mathf.FloorToInt(curr.y) + 1, 0);
        //Down
        Vector3Int down = new Vector3Int(Mathf.FloorToInt(curr.x), Mathf.FloorToInt(curr.y) - 1, 0);

        surroundingTiles = randomInsert(surroundingTiles, left, right, up, down);
        
        foreach (Vector3Int tile in surroundingTiles)
        {
            RaycastHit2D hit = Physics2D.Raycast(curr, new Vector2(tile.x, tile.y), distance: 1f);

            if (collidable.GetTile(tile) == null && ground.GetTile(tile) != null && (!hit.collider || hit.collider.gameObject.transform.position==transform.position || !hit.collider.gameObject.CompareTag("Enemy")))
            {
                neighbors.Add((new Vector2(tile.x + 0.5f, tile.y + 0.5f),1));
            }
            else if(hit.collider && !hit.collider.gameObject.CompareTag("Enemy"))
            {
                int tilePreference = mod == null ? 15 : mod.GetBlockHealthAtTile(tile) * 5;
                neighbors.Add((new Vector2(tile.x + 0.5f, tile.y + 0.5f), tilePreference));
            }
        }
        return neighbors;
    }

    List<Vector3Int> randomInsert(List<Vector3Int> insertList,Vector3Int elem1,Vector3Int elem2,Vector3Int elem3,Vector3Int elem4)
    {
        int rand = pathFindOrder;
        //Insert the 4 elements in a random order
        switch(rand)
        {
            case 0:
                insertList.Add(elem1);
                insertList.Add(elem2);
                insertList.Add(elem3);
                insertList.Add(elem4);
                break;
            case 1:
                insertList.Add(elem1);
                insertList.Add(elem2);
                insertList.Add(elem4);
                insertList.Add(elem3);
                break;
            case 2:
                insertList.Add(elem1);
                insertList.Add(elem3);
                insertList.Add(elem2);
                insertList.Add(elem4);
                break;
            case 3:
                insertList.Add(elem1);
                insertList.Add(elem3);
                insertList.Add(elem4);
                insertList.Add(elem2);
                break;
            case 4:
                insertList.Add(elem1);
                insertList.Add(elem4);
                insertList.Add(elem2);
                insertList.Add(elem3);
                break;
            case 5:
                insertList.Add(elem2);
                insertList.Add(elem1);
                insertList.Add(elem3);
                insertList.Add(elem4);
                break;
            case 6:
                insertList.Add(elem2);
                insertList.Add(elem3);
                insertList.Add(elem4);
                insertList.Add(elem1);
                break;
            case 7:
                insertList.Add(elem3);
                insertList.Add(elem1);
                insertList.Add(elem2);
                insertList.Add(elem4);
                break;
            case 8:
                insertList.Add(elem4);
                insertList.Add(elem1);
                insertList.Add(elem2);
                insertList.Add(elem3);
                break;
            case 9:
                insertList.Add(elem1);
                insertList.Add(elem4);
                insertList.Add(elem3);
                insertList.Add(elem2);
                break;
            case 10:
                insertList.Add(elem2);
                insertList.Add(elem1);
                insertList.Add(elem4);
                insertList.Add(elem3);
                break;
            case 11:
                insertList.Add(elem2);
                insertList.Add(elem4);
                insertList.Add(elem3);
                insertList.Add(elem1);
                break;
            case 12:
                insertList.Add(elem3);
                insertList.Add(elem1);
                insertList.Add(elem4);
                insertList.Add(elem2);
                break;
            case 13:
                insertList.Add(elem4);
                insertList.Add(elem1);
                insertList.Add(elem3);
                insertList.Add(elem2);
                break;
            case 14:
                insertList.Add(elem2);
                insertList.Add(elem3);
                insertList.Add(elem1);
                insertList.Add(elem4);
                break;
            case 15:
                insertList.Add(elem3);
                insertList.Add(elem4);
                insertList.Add(elem1);
                insertList.Add(elem2);
                break;
            case 16:
                insertList.Add(elem2);
                insertList.Add(elem4);
                insertList.Add(elem1);
                insertList.Add(elem3);
                break;
            case 17:
                insertList.Add(elem3);
                insertList.Add(elem2);
                insertList.Add(elem1);
                insertList.Add(elem4);
                break;
            case 18:
                insertList.Add(elem3);
                insertList.Add(elem2);
                insertList.Add(elem4);
                insertList.Add(elem1);
                break;
            case 19:
                insertList.Add(elem3);
                insertList.Add(elem4);
                insertList.Add(elem2);
                insertList.Add(elem1);
                break;
            case 20:
                insertList.Add(elem4);
                insertList.Add(elem2);
                insertList.Add(elem1);
                insertList.Add(elem3);
                break;
            case 21:
                insertList.Add(elem4);
                insertList.Add(elem2);
                insertList.Add(elem3);
                insertList.Add(elem1);
                break;
            case 22:
                insertList.Add(elem4);
                insertList.Add(elem3);
                insertList.Add(elem1);
                insertList.Add(elem2);
                break;
            case 23:
                insertList.Add(elem4);
                insertList.Add(elem3);
                insertList.Add(elem2);
                insertList.Add(elem1);
                break;
        }

        return insertList;
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        colliding = false;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        colliding = true;
        //Debug.Log("collision with " + collision.gameObject.name + " while " + colliding);
        if (!destroyCooldownActive) 
        {
            if (collision.gameObject.name.Equals("Collidable"))
            {
                //Debug.Log("Contact at " + collision.contacts[0].point);
                Vector3Int curPos = new Vector3Int(Mathf.FloorToInt(collision.contacts[0].point.x), Mathf.FloorToInt(collision.contacts[0].point.y), 0);
                mod.AttemptTileDestroy(curPos);
            }
            else if (collision.gameObject.CompareTag("Defense"))
            {
                Defense def = collision.gameObject.GetComponent<Defense>();
                def.doDamage(damageLevel);
                takeDamage(def.inherentDamage);
            }
            cooldown();
        }
        if(collision.gameObject.CompareTag("Enemy"))
        {
            if (!destroyCooldownActive)
            {
                shouldIMove = false;
                StartCoroutine("enableMove");
            }
        }
    }
    void takeDamage(int amount)
    {
        if((health-=amount)<=0)
        {
            spawnCoin();
            Destroy(gameObject);
        }
    }
    void cooldown()
    {
        destroyCooldownActive = true;
        StartCoroutine("enableDestroy");

    }
    IEnumerator enableDestroy()
    {
        yield return new WaitForSeconds(0.5f);
        destroyCooldownActive = false;

    }
    IEnumerator enableMove()
    {
        yield return new WaitForSeconds(1.0f);
        shouldIMove = true;
    }
    int EuclideanEstimate(Vector2 node, Vector2 goal)
    {
        return (int)Mathf.Sqrt(Mathf.Pow(node.x - goal.x, 2) +
            Mathf.Pow(node.y - goal.y, 2));
    }
    void followPath()
    {
        if (path.Count > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, path[pathIndex], baseSpeed * Time.deltaTime);
            if (!colliding && pathIndex > 0 && transform.position == (Vector3)path[pathIndex] && !collidable.GetTile(new Vector3Int(Mathf.FloorToInt(path[pathIndex].x), Mathf.FloorToInt(path[pathIndex].y), 0)))
            {
               /* if(collidable.GetTile(new Vector3Int(Mathf.FloorToInt(path[pathIndex].x), Mathf.FloorToInt(path[pathIndex].y), 0)))
                {
                    Debug.Log("I'm dumb");
                }*/
                lastTarget = path[pathIndex];
                pathIndex--;
            }
        }
    }
    void retracePath()
    {
        if(path.Count > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, path[pathIndex], 1f * Time.deltaTime);
            if (pathIndex < path.Count - 2)
            {
                pathIndex++;
            }
        }
    }

}
