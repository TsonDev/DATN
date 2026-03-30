using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;

    [Header("Positioning")]
    public float reach = 0.5f;                     // distance from hand
    public Vector2 localOffset = Vector2.zero;     // offset in hand local space (before rotation)
    public float rotationOffset = 90f;             // adjust if sprite needs +90 deg

    [Header("Timing")]
    public float maxLifetime = 1.0f;               // safety destroy if animation event not called
    public bool useAnimationEvents = true;         // if true, enable collider from animation event

    [Header("Components")]
    public Collider2D hitCollider;                 // assign in prefab (BoxCollider2D, CircleCollider2D...), should be isTrigger=true

    // runtime
    HashSet<int> hitTargets = new HashSet<int>();
    float lifeTimer;

    private void Awake()
    {
        if (hitCollider == null)
            hitCollider = GetComponent<Collider2D>();

        if (hitCollider != null)
            hitCollider.enabled = !useAnimationEvents; // if using anim events, start disabled
    }

    private void Start()
    {
        lifeTimer = maxLifetime;
        if (!useAnimationEvents)
        {
            // If not using animation events, enable immediately and auto-disable after a short window
            EnableHitbox();
            Invoke(nameof(DisableHitbox), maxLifetime * 0.5f);
            Invoke(nameof(EndSwing), maxLifetime);
        }
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
            EndSwing();
    }

    // Call from PlayerController immediately after Instantiate.
    // ownerHand: recommended to pass player's hand transform (a child Transform on player).
    public void Setup(Vector2 direction, Transform ownerHand = null)
    {
        // parent to hand if provided so it follows player movement/rotation
        if (ownerHand != null)
            transform.SetParent(ownerHand, false);

        // compute rotated local offset so melee aligns with direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle + rotationOffset);

        // rotate localOffset (Vector2) using quaternion -> use Vector3 to avoid conversion error
        Vector3 rotatedOffset = rot * new Vector3(localOffset.x, localOffset.y, 0f);

        // forward direction in local space (as Vector3)
        Vector3 forward = Quaternion.Euler(0f, 0f, angle) * Vector3.right;

        // set localPosition (works whether parented to hand or not)
        transform.localPosition = rotatedOffset + forward * reach;

        // set world rotation for sprite/orientation
        transform.rotation = rot;
    }

    // Called from Animation Event (frame where hit should register)
    public void EnableHitbox()
    {
        hitTargets.Clear();
        if (hitCollider != null) hitCollider.enabled = true;
    }

    // Optional: call from Animation Event after hit frame to stop registering hits
    public void DisableHitbox()
    {
        if (hitCollider != null) hitCollider.enabled = false;
    }

    // End swing (usually called by animation at end or as fallback)
    public void EndSwing()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hitCollider == null || !hitCollider.enabled) return;

        if (!collision.CompareTag("Enemy")) return;

        int id = collision.gameObject.GetInstanceID();
        if (hitTargets.Contains(id)) return; // already hit this target in this swing

        hitTargets.Add(id);

        var enemy = collision.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.ChangeHealth(-damage, DameType.TypeDamage.Monster);
        }
    }
}
