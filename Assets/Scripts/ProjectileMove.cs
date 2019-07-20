using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProjectileMove : MonoBehaviour
{
    private WorldModifier t;
    //private Tilemap collidable;
    Rigidbody2D myRigidBody;
    public float lifeSpan = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeSpan);
    }
    public void setLifeSpan(int span)
    {
        lifeSpan = span;
    }
    public void SetTileBreaker(WorldModifier tileB)
    {
        t = tileB;
    }
    // Update is called once per frame
    void Update()
    {
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            BaddieAI enemy = collision.gameObject.GetComponent<BaddieAI>();
            enemy.spawnCoin();
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
        if (collision.gameObject.name.CompareTo("Collidable") == 0)
        {
            Destroy(gameObject);
        }

    }
}
