using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class PlayerControllerFT : MonoBehaviour
{
    public float MoveSpeed = 5f;
	public float jumpImpulse = 10f;
	public float dashSpeed = 15f;
	public float airWalkSpeed = 3f;
	public float dashTime = 0.2f;
	public float dashCooldown = 0f;

	public static event System.Action OnPlayerJump;

	Vector2 moveInput;
	TouchingDirections touchingDirections;
	Damageable damageable;

	// --- Skill Cooldowns ---
	[SerializeField] private float skill1Cooldown = 1f; // 3s hồi chiêu
	[SerializeField] private float skill2Cooldown = 3f; // 3s hồi chiêu
	[SerializeField] private float skill3Cooldown = 5f; // 3s hồi chiêu
	private bool skill1OnCooldown = false;
	private bool skill2OnCooldown = false;
	private bool skill3OnCooldown = false;

	private ManaSystem manaSystem;

	private bool _isMoving = false;
	private bool isDashing = false;
	private bool canDash = true;
	private int attackStep = 0;
	private float lastAttackTime;
	[SerializeField] private float comboResetTime = 0.8f;

	// --- Blocking ---
	[Range(0f, 1f)] public float blockDamageReduction = 0.3f; // 30% sát thương khi block
	Rigidbody2D rb;
	Animator animator;

	public float CurrentMoveSpeed
	{
		get
		{
			if (CanMove)
			{
				if (IsMoving && !touchingDirections.IsOnWall)
				{
					if (touchingDirections.IsGrounded)
					{
						if (isDashing)
						{
							return dashSpeed;
						}
						else
						{
							return MoveSpeed;
						}
					}
					else
					{
						return airWalkSpeed;
					}
				}
				else
				{
					return 0;
				}
			}
			else
			{
				return 0;
			}
		}
	}
	private bool _isBlocking = false;
	public bool IsBlocking
	{
		get
		{
			return _isBlocking;
		}
		private set
		{
			_isBlocking = value;
			animator.SetBool(AnimationStrings.isBlocking, value);
		}
	}

	public bool IsMoving
	{
		get { return _isMoving; }
		private set
		{
			_isMoving = value;
			animator.SetBool(AnimationStrings.isMoving, value);
		}
	}
	private bool _isFacingRight = true;

	public bool IsFacingRight
	{
		get { return _isFacingRight; }
		private set
		{
			if (_isFacingRight != value)
			{
				transform.localScale *= new Vector2(-1, 1);
			}
			_isFacingRight = value;
		}
	}
	public bool CanMove
	{
		get
		{
			return animator.GetBool(AnimationStrings.canMove);

		}
	}
	public bool IsAlive
	{
		get
		{
			return animator.GetBool(AnimationStrings.isAlive);
		}
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		touchingDirections = GetComponent<TouchingDirections>();
		damageable = GetComponent<Damageable>();
		manaSystem = GetComponent<ManaSystem>();
	}

	private void FixedUpdate()
	{
		if (IsAlive && !isDashing && !damageable.LockVelocity)
		{
			if (IsBlocking)
			{
				// khi block thì đứng yên (khóa trơn trượt)
				rb.velocity = new Vector2(0, rb.velocity.y);
			}
			else
			{
				rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);
			}
		}
		else if (!IsAlive)
		{
			rb.velocity = new Vector2(0, rb.velocity.y); // khi chết thì ngừng di chuyển ngang
		}

		animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
	}


	public void OnMove(InputAction.CallbackContext context)
	{
		if (IsBlocking) // khi block thì không đọc input
		{
			moveInput = Vector2.zero;
			IsMoving = false;
			return;
		}

		moveInput = context.ReadValue<Vector2>();

		if (IsAlive)
		{
			IsMoving = moveInput != Vector2.zero;
			AudioManager_Fight.instance.PlayMove();
			SetFacingDirection(moveInput);
		}
		else
		{
			IsMoving = false;
		}
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if(context.started && touchingDirections.IsGrounded && CanMove)
		{
			animator.SetTrigger(AnimationStrings.jump);
			rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
			AudioManager_Fight.instance.PlayJump();
			OnPlayerJump?.Invoke();
		}
	}
	public void OnDash(InputAction.CallbackContext context)
	{
		if (IsBlocking) return; // không dash khi block
		if (context.performed && canDash)
		{
			StartCoroutine(Dash());
			AudioManager_Fight.instance.PlayDash();
		}
	}

	public void OnAttack(InputAction.CallbackContext context)
	{
		if (IsBlocking) return; // không tấn công khi block
		if (context.started)
		{
			TryAttack();
		}
	}

	public void OnSkill1(InputAction.CallbackContext context)
	{
		if (context.started && !skill1OnCooldown)
		{
			if (manaSystem.UseMana(15))
			{
			animator.SetTrigger(AnimationStrings.Skill1);
			AudioManager_Fight.instance.PlaySkill1();
			StartCoroutine(Skill1CooldownRoutine());
			}
			else
			{
				Debug.Log("Không đủ mana để dùng Skill1!");
			}
		}
		else if (context.started && skill1OnCooldown)
		{
			Debug.Log("Skill1 đang hồi chiêu...");
		}
	}
	public void OnSkill2(InputAction.CallbackContext context)
	{
		if (context.started && !skill2OnCooldown)
		{
			if (manaSystem.UseMana(25))
			{
			animator.SetTrigger(AnimationStrings.Skill2);
			AudioManager_Fight.instance.PlaySkill2();
			StartCoroutine(Skill2CooldownRoutine());
			}
			else
			{
				Debug.Log("Không đủ mana để dùng Skill2!");
			}
		}
		else if (context.started && skill2OnCooldown)
		{
			Debug.Log("Skill1 đang hồi chiêu...");
		}
	}
	public void OnSkill3(InputAction.CallbackContext context)
	{
		if (context.started && !skill3OnCooldown)
		{
			if (manaSystem.UseMana(50))
			{
				animator.SetTrigger(AnimationStrings.Skill3);
				AudioManager_Fight.instance.PlaySkill3();
				StartCoroutine(Skill3CooldownRoutine());
			}
			else
			{
				Debug.Log("Không đủ mana để dùng Skill2!");
			}
		}
		else if (context.started && skill3OnCooldown)
		{
			Debug.Log("Skill1 đang hồi chiêu...");
		}
	}
	private IEnumerator Skill1CooldownRoutine()
	{
		skill1OnCooldown = true;
		yield return new WaitForSeconds(skill1Cooldown);
		skill1OnCooldown = false;
	}

	private IEnumerator Skill2CooldownRoutine()
	{
		skill2OnCooldown = true;
		yield return new WaitForSeconds(skill2Cooldown);
		skill2OnCooldown = false;
	}

	private IEnumerator Skill3CooldownRoutine()
	{
		skill3OnCooldown = true;
		yield return new WaitForSeconds(skill3Cooldown);
		skill3OnCooldown = false;
	}
	private void TryAttack()
	{
		if (Time.time > lastAttackTime + comboResetTime)
			attackStep = 0; // reset combo nếu lâu quá

		attackStep++;
		if (attackStep > 3) attackStep = 1; // loop 1 → 3

		animator.SetInteger(AnimationStrings.attackStep, attackStep);
		animator.SetTrigger(AnimationStrings.attack);
		lastAttackTime = Time.time;

	}
	public void OnHit(int damage, Vector2 knockback)
	{
		if (AudioManager_Fight.instance != null)
			AudioManager_Fight.instance.PlayHurt();
		rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);
		if (manaSystem != null)
		{
			manaSystem.GainManaFromDamage(damage);
		}
	}

	public void OnBlock(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			IsBlocking = true;
			AudioManager_Fight.instance.PlayHurt();
			Debug.Log(">>> BLOCK START");
		}
		else if (context.canceled)
		{
			IsBlocking = false;
			Debug.Log(">>> BLOCK END");
		}
	}
	private IEnumerator Dash()
	{
		canDash = false;
		isDashing = true;
		animator.SetBool(AnimationStrings.isDashing, true);

		float originalGravity = rb.gravityScale;
		rb.gravityScale = 0;
		rb.velocity = new Vector2(IsFacingRight ? dashSpeed : -dashSpeed, 0);

		yield return new WaitForSeconds(dashTime);

		rb.gravityScale = originalGravity;
		isDashing = false;
		animator.SetBool(AnimationStrings.isDashing, false);

		yield return new WaitForSeconds(dashCooldown);
		canDash = true;
	}

	private void SetFacingDirection(Vector2 moveInput)
	{
		if(moveInput.x > 0 && !IsFacingRight)
		{
			IsFacingRight = true;
		}
		else if (moveInput.x < 0 && IsFacingRight)
		{
			IsFacingRight = false;
		}
	}
	public void PlayAttackSFX()
	{
		if (AudioManager_Fight.instance != null)
			AudioManager_Fight.instance.PlayAttack(); 
	}
}
