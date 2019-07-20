using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AutoProjectileSpawner : ProjectileSpawner
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        InvokeRepeating("ShootUp", cooldown, cooldown);
    }
    void ShootUp()
    {
        Vector3 shootDir = new Vector3(transform.position.x, transform.position.y + 5);
        Shoot(shootDir);
        lastTime = Time.time;
    }
    // Update is called once per frame
    void Update()
    {
        showCooldown();
    }

}
