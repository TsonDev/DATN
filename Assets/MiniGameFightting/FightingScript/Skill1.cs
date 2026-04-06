using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill1 : MonoBehaviour
{
    public int damage = 15;
    public Vector2 moveSpeed = new Vector2(3f, 0);
	public Vector2 knockback = new Vector2 (0, 0);

	[Header("Skill Settings")]
	public float lifeTime = 2f; // Thời gian tồn tại của skill

	Rigidbody2D rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	// Start is called before the first frame update
	void Start()
    {
        rb.velocity = new Vector2(moveSpeed.x * transform.localScale.x, moveSpeed.y);

		Destroy(gameObject, lifeTime);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Damageable damageable = collision.GetComponent<Damageable>();

		if (damageable != null)
		{
			Vector2 deliveredKnockback = transform.localScale.x > 0 ? knockback : new Vector2(-knockback.x, knockback.y);

			bool gotHit = damageable.Hit(damage, knockback);

			if (gotHit)
			{
				Debug.Log(collision.name + "hit for" + damage);
				Destroy(gameObject);
			}
		}
	}
}
