using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Defense : MonoBehaviour
{
    public int cost;
    public int inherentDamage;
    public int health;
    public void doDamage(int amount)
    {
        if((health-=amount) <= 0)
        {
            Destroy(gameObject);
        }
    }
}
