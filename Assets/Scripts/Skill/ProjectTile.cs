using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectTile : MonoBehaviour
{
    
    private float TimeDestroy;
    public float TimeAlive;
    public int damage;
    Rigidbody2D rig2d;
    Animator animator;
    Vector2 moveInput;

    private void Awake()
    {
        rig2d = GetComponent<Rigidbody2D>();
       animator = GetComponent<Animator>();
        TimeDestroy = TimeAlive;
    }
    private void Update()
    {
        TimeDestroy -= Time.deltaTime;
        if (TimeDestroy < 0)
        {
            Destroy(gameObject);
            TimeDestroy = TimeAlive;
        }
    }

    public void LunchProTile(Vector2 direction, float force)
    {
        rig2d.AddForce(direction * force);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyBase enemyBase = collision.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.ChangeHealth(-damage,DameType.TypeDamage.Projectile);
              
            }
            Destroy(gameObject);
        }
    }
}
