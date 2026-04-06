using System.Collections;
using UnityEngine;

public class EnemyProjecttile : MonoBehaviour
{
    [Header("Lifetime")]
    public float timeAlive = 4f;
    float lifeTimer;

    [Header("Damage")]
    public int damage = 1;
    public bool pierceEnemies = false; // if true, will not destroy on hitting player (rare)

    [Header("Physics")]
    public Rigidbody2D rb;
    public float defaultSpeed = 10f;
    [Tooltip("If sprite faces up (Y+) by default set 0. If faces right set -90, etc.")]
    public float rotationOffset = 0f;

    [Header("Animation")]
    public Animator animator;
    public string destroyBoolName = "isDestroy";

    bool isDestroying;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        lifeTimer = timeAlive;
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f && !isDestroying)
        {
            StartCoroutine(PlayDestroyAndRemove());
        }
    }

    // Launch with normalized direction and set velocity (deterministic)
    public void LaunchProjectile(Vector2 direction, float speed)
    {
        if (rb == null) return;
        // set orientation so sprite faces travel direction (adjust with rotationOffset)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        rb.velocity = direction.normalized * (speed > 0f ? speed : defaultSpeed);
    }

    IEnumerator PlayDestroyAndRemove()
    {
        isDestroying = true;
        if (animator != null && !string.IsNullOrEmpty(destroyBoolName))
            animator.SetBool(destroyBoolName, true);

        float wait = 0.12f;
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            var clips = animator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                var nameLower = clip.name.ToLower();
                if (nameLower.Contains("destroy") || nameLower.Contains("hit") || nameLower.Contains("explode") || nameLower.Contains("break"))
                {
                    wait = Mathf.Max(wait, clip.length);
                    break;
                }
            }
        }

        yield return null;
        yield return new WaitForSeconds(wait);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroying) return;

        // Damage the player
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ChangeHealth(-damage, DameType.TypeDamage.Projectile);
            }

            if (!pierceEnemies)
            {
                StartCoroutine(PlayDestroyAndRemove());
            }
            return;
        }

        // Optionally destroy on hitting environment/walls (non-trigger)
        if (!other.isTrigger)
        {
            StartCoroutine(PlayDestroyAndRemove());
        }
    }
}

