using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
	public UnityEvent<int, Vector2> damagableHit;
	public UnityEvent<int, int> healthChanged;

	Animator animator;
	private PlayerControllerFT playerController;
	private AIControllerFT aiController;

	[SerializeField]
	private int _maxHealth = 100;

	public int MaxHealth
	{
		get { return _maxHealth; }
		set { _maxHealth = value; }
	}

	[SerializeField]
	private int _health = 100;

	public int Health
	{
		get { return _health; }
		set
		{
			_health = value;
			healthChanged?.Invoke(_health, MaxHealth);

			if (_health <= 0)
			{
				IsAlive = false;
			}
		}
	}

	public bool LockVelocity
	{
		get
		{
			return animator.GetBool(AnimationStrings.lockVelocity);
		}
		set
		{
			animator.SetBool(AnimationStrings.lockVelocity, value);
		}
	}
	[SerializeField]
	private bool _isAlive = false;

	[SerializeField]
	private bool isInvincible = false;

	
	private float timeSinceHit = 0;
	public float invincibilityTime = 0.25f;

	public bool IsAlive
	{
		get { return _isAlive; }
		set
		{
			_isAlive = value;
			animator.SetBool(AnimationStrings.isAlive, value);
		}
	}
	
	private void Update()
	{
		if (isInvincible)
		{
			if(timeSinceHit > invincibilityTime)
			{
				isInvincible = false;
				timeSinceHit = 0;
			}

			timeSinceHit += Time.deltaTime;
		}
	}

	private void Awake()
	{
		animator = GetComponent<Animator>();
		playerController = GetComponent<PlayerControllerFT>();
		aiController = GetComponent<AIControllerFT>();
	}
	private IEnumerator UnlockVelocityAfterAnim(float delay)
	{
		yield return new WaitForSeconds(delay);
		LockVelocity = false;
	}
	public bool Hit(int damage, Vector2 knockback)
	{
		if (IsAlive && !isInvincible)
		{
			int finalDamage = damage;
			bool isBlocking = false;

			// --- Kiểm tra Player block ---
			if (playerController != null && playerController.IsBlocking)
			{
				isBlocking = true;
				finalDamage = Mathf.RoundToInt(damage * playerController.blockDamageReduction);
			}

			// --- Kiểm tra AI block ---
			if (aiController != null && aiController.IsBlocking)
			{
				isBlocking = true;
				finalDamage = Mathf.RoundToInt(damage * 0.3f); // AI block giảm còn 20% damage
			}
			else if (aiController != null)
			{
				// Nếu chưa block thì thử block cho các đòn tiếp theo
				aiController.TryBlock();
			}

			if (isBlocking)
			{
				Debug.Log($"[BLOCK] Damage reduced from {damage} → {finalDamage}");
				Health -= finalDamage;
				isInvincible = true;

				LockVelocity = true;
				animator.SetTrigger(AnimationStrings.blockHit);
				if (AudioManager_Fight.instance != null)
					AudioManager_Fight.instance.PlayBlock();
				StartCoroutine(UnlockVelocityAfterAnim(0.2f));
			}
			else
			{
				// Không block
				Health -= finalDamage;
				isInvincible = true;

				LockVelocity = true;
				animator.SetTrigger(AnimationStrings.hitTrigger);
			}

			damagableHit?.Invoke(finalDamage, knockback);

			return true;
		}
		return false;
	}
}