using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class ProjectileSpawner : Defense
{
    Rigidbody2D myRigidBody;
    [SerializeField] ProjectileMove projectile;
    //[SerializeField] protected Tilemap colliderMap;
    [SerializeField] float bulletSpeed = 10.0f;
    [SerializeField] protected float cooldown;
    [SerializeField] GameObject cooldownBar;
    [SerializeField] GameObject cooldownContainer;
    protected float lastTime;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        lastTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        showCooldown();
        if (Time.time > lastTime + cooldown)
        {

            if (Input.GetButtonDown("Fire1") && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Shoot(clickPos);
                lastTime = Time.time;
            }

        }
    }
    protected void showCooldown()
    {
        float remainingCooldown = (Time.time - lastTime) / cooldown;
        if (remainingCooldown < 1)
        {
            cooldownContainer.SetActive(true);
            cooldownBar.gameObject.transform.localScale = new Vector3(remainingCooldown > 1 ? 1 : remainingCooldown, 1f);
        }
        else
        {
            cooldownContainer.SetActive(false);
        }
    }
    protected void Shoot(Vector3 clickPos)
    {
        Vector3 difference = clickPos - transform.position;
        float distance = difference.magnitude;
        Vector2 direction = difference / distance;
        direction.Normalize();
        ProjectileMove instance = Instantiate(projectile, transform.position, Quaternion.identity);
        instance.SetTileBreaker(GetComponent<WorldModifier>());
        //instance.SetColliderMap(colliderMap);
        instance.GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;
    }
}
