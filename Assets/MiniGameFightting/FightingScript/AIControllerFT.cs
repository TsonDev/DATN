using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class AIControllerFT : MonoBehaviour
{
	[Header("Move Settings")]
	public float MoveSpeed = 5f;
	public float jumpImpulse = 10f;
	public float dashSpeed = 15f;
	public float airWalkSpeed = 3f;
	public float dashTime = 0.2f;
	public float dashCooldown = 1f;

	[Header("AI Settings")]
	private Transform target;
	public float attackRange = 1.5f;
	public float detectionRange = 10f;
	public float dashDistance = 5f;
	[SerializeField] private float blockChance = 0.3f;

	[Header("Skill Settings")]
	[SerializeField] private float skill1Cooldown = 2f;
	[SerializeField] private float skill2Cooldown = 4f;
	[SerializeField] private float skill3Cooldown = 6f;

	[SerializeField] private int skill1ManaCost = 15;
	[SerializeField] private int skill2ManaCost = 25;
	[SerializeField] private int skill3ManaCost = 50;

	private bool skill1OnCooldown = false;
	private bool skill2OnCooldown = false;
	private bool skill3OnCooldown = false;

	private Vector2 moveInput;
	private TouchingDirections touchingDirections;
	private Rigidbody2D rb;
	private Animator animator;
	private Damageable damageable;
	private ManaSystem manaSystem;

	private bool _isMoving = false;
	private bool isDashing = false;
	private bool canDash = true;
	private bool _isFacingRight = true;

	private float lastAttackTime;
	private int attackStep = 0;
	float comboResetTime = 0.8f;
	public float attackCooldown = 1.0f;

	// --- Properties ---
	public float CurrentMoveSpeed
	{
		get
		{
			if (CanMove && !IsBlocking)
			{
				if (IsMoving && !touchingDirections.IsOnWall)
				{
					if (touchingDirections.IsGrounded)
					{
						if (isDashing) return dashSpeed;
						else return MoveSpeed;
					}
					else return airWalkSpeed;
				}
				else return 0;
			}
			else return 0;
		}
	}

	private bool _isBlocking = false;
	public bool IsBlocking
	{
		get { return _isBlocking; }
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
	public bool CanMove => animator.GetBool(AnimationStrings.canMove);
	public bool IsAlive => animator.GetBool(AnimationStrings.isAlive);

	// --- Unity Methods ---
	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		touchingDirections = GetComponent<TouchingDirections>();
		damageable = GetComponent<Damageable>();
		manaSystem = GetComponent<ManaSystem>(); // thêm phần mana

		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
		if (playerObj != null)
		{
			target = playerObj.transform;
		}
		else
		{
			Debug.LogWarning("AIControllerFT: Không tìm thấy GameObject với tag 'Player'!");
		}
	}

	private void FixedUpdate()
	{
		if (!isDashing && !damageable.LockVelocity)
		{
			if (IsBlocking)
			{
				rb.velocity = new Vector2(0, rb.velocity.y);
			}
			else
			{
				rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);
			}
		}
		animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
	}

	private float footstepTimer = 0f;
	[SerializeField] private float footstepInterval = 0.35f; // thời gian giữa 2 tiếng bước chân
	[SerializeField] private float decisionCooldown = 0.4f; // tần suất quyết định hành động (giảm spam)
	private float decisionTimer = 0f;

	private void Update()
	{
		if (!IsAlive || target == null) return;

		// Khi đang block, AI đứng yên
		if (IsBlocking)
		{
			moveInput = Vector2.zero;
			IsMoving = false;
			return;
		}

		// Giảm thời gian chờ quyết định hành động
		decisionTimer -= Time.deltaTime;

		// Tính khoảng cách tới Player
		float distance = Vector2.Distance(transform.position, target.position);

		// Nếu Player ở quá xa thì dừng
		if (distance > detectionRange)
		{
			moveInput = Vector2.zero;
			IsMoving = false;
			return;
		}

		// === KHI TRONG TẦM PHÁT HIỆN ===
		// Nếu chưa tới gần (ngoài tầm tấn công)
		if (Mathf.Abs(target.position.x - transform.position.x) > attackRange)
		{
			// Di chuyển về hướng Player
			float dir = target.position.x > transform.position.x ? 1 : -1;
			moveInput = new Vector2(dir, 0);
			IsMoving = true;
			SetFacingDirection(moveInput);

			// 🔊 Phát tiếng bước chân (có cooldown)
			if (touchingDirections.IsGrounded && IsMoving)
			{
				footstepTimer -= Time.deltaTime;
				if (footstepTimer <= 0f)
				{
					if (AudioManager_Fight.instance != null)
						AudioManager_Fight.instance.PlayMove();

					footstepTimer = footstepInterval;
				}
			}

			// Ngẫu nhiên dash về phía player
			if (canDash && Random.value < 0.01f)
				StartCoroutine(DashRandom());
		}
		else
		{
			// Đã tới gần → ngừng di chuyển
			moveInput = Vector2.zero;
			IsMoving = false;

			// Chỉ quyết định hành động mỗi vài phần giây để tránh spam
			if (decisionTimer <= 0f)
			{
				decisionTimer = decisionCooldown;

				float roll = Random.value;

				if (roll < 0.7f)
					TryAttack();       // 70% khả năng tấn công
				else
					TryUseSkill();     // 30% khả năng dùng skill
			}
		}
	}


	// --- Combat Actions ---
	private void TryUseSkill()
	{
		if (IsBlocking || !IsAlive) return;

		float roll = Random.value;

		if (roll < 0.4f)
		{
			int skillIndex = Random.Range(1, 4);
			switch (skillIndex)
			{
				case 1:
					if (!skill1OnCooldown && manaSystem.UseMana(skill1ManaCost))
					{
						Debug.Log("AI dùng Skill 1 (tầm xa)");
						animator.SetTrigger(AnimationStrings.Skill1);
						if (AudioManager_Fight.instance != null)
							AudioManager_Fight.instance.PlaySkill1();
						StartCoroutine(Skill1CooldownRoutine());
					}
					break;

				case 2:
					if (!skill2OnCooldown && manaSystem.UseMana(skill2ManaCost))
					{
						Debug.Log("AI dùng Skill 2 (tầm gần)");
						animator.SetTrigger(AnimationStrings.Skill2);
						if (AudioManager_Fight.instance != null)
							AudioManager_Fight.instance.PlaySkill2();
						StartCoroutine(Skill2CooldownRoutine());
					}
					break;

				case 3:
					if (!skill3OnCooldown && manaSystem.UseMana(skill3ManaCost))
					{
						Debug.Log("AI dùng Skill 3 (tất sát)");
						animator.SetTrigger(AnimationStrings.Skill3);
						if (AudioManager_Fight.instance != null)
							AudioManager_Fight.instance.PlaySkill2();
						StartCoroutine(Skill3CooldownRoutine());
					}
					break;
			}
		}
		else
		{
			TryAttack();
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

	// --- Movement & Combat Logic ---
	private void TryAttack()
	{
		if (IsBlocking) return;
		if (Time.time < lastAttackTime + attackCooldown)
			return;

		if (Time.time > lastAttackTime + comboResetTime)
			attackStep = 0;

		attackStep++;
		if (attackStep > 3) attackStep = 1;

		animator.SetInteger("attackStep", attackStep);
		animator.SetTrigger("attack");

		lastAttackTime = Time.time;
	}

	private void SetFacingDirection(Vector2 moveInput)
	{
		if (moveInput.x > 0 && !IsFacingRight)
		{
			IsFacingRight = true;
		}
		else if (moveInput.x < 0 && IsFacingRight)
		{
			IsFacingRight = false;
		}
	}

	private IEnumerator DashRandom()
	{
		if (IsBlocking) yield break;
		if (target == null) yield break;

		canDash = false;
		isDashing = true;
		animator.SetBool(AnimationStrings.isDashing, true);

		float originalGravity = rb.gravityScale;
		rb.gravityScale = 0;

		// 👉 Dash theo hướng của player
		int dir = target.position.x > transform.position.x ? 1 : -1;
		IsFacingRight = dir > 0;
		if (AudioManager_Fight.instance != null)
			AudioManager_Fight.instance.PlayDash();
		// Giữ hướng dash theo player
		rb.velocity = new Vector2(dir * dashSpeed, 0);

		yield return new WaitForSeconds(dashTime);

		rb.gravityScale = originalGravity;
		isDashing = false;
		animator.SetBool(AnimationStrings.isDashing, false);

		yield return new WaitForSeconds(dashCooldown);
		canDash = true;
	}


	public void TryBlock()
	{
		if (!IsBlocking && Random.value < blockChance)
		{
			StartCoroutine(BlockRoutine());
		}
	}

	private IEnumerator BlockRoutine()
	{
		IsBlocking = true;
		Debug.Log("AI START BLOCK");
		if (AudioManager_Fight.instance != null)
			AudioManager_Fight.instance.PlayHurt(); // 🛡 âm bắt đầu đỡ

		yield return new WaitForSeconds(Random.Range(0.5f, 1f));

		IsBlocking = false;
		Debug.Log("AI END BLOCK");
	}

	// --- Hit Reaction ---
	public void OnHit(int damage, Vector2 knockback)
	{
		if (AudioManager_Fight.instance != null)
			AudioManager_Fight.instance.PlayHurt(); // bị đánh trúng

		rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);

		// hồi mana khi bị đánh
		manaSystem.GainManaFromDamage(damage);
	}

	// --- Player Jump Response ---
	private void OnEnable()
	{
		PlayerControllerFT.OnPlayerJump += HandlePlayerJump;
	}

	private void OnDisable()
	{
		PlayerControllerFT.OnPlayerJump -= HandlePlayerJump;
	}

	private void HandlePlayerJump()
	{
		if (touchingDirections.IsGrounded && target != null)
		{
			float distance = Vector2.Distance(transform.position, target.position);
			if (distance <= detectionRange)
			{
				Jump();
			}
		}
	}

	private void Jump()
	{
		if (touchingDirections.IsGrounded && CanMove && !IsBlocking)
		{
			animator.SetTrigger(AnimationStrings.jump);
			rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
			if (AudioManager_Fight.instance != null)
				AudioManager_Fight.instance.PlayJump();

		}
	}
	public void PlayAttackSFX()
	{
		if (AudioManager_Fight.instance != null)
			AudioManager_Fight.instance.PlayAttack();
	}
}
