using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
   
    // Start is called before the first frame update
    [SerializeField] private float moveSpeed;
    public float boostSpeed
    {
        get { return moveSpeed; }
        set {  moveSpeed = value; }
    }
    PlayerController controller;
    private Rigidbody2D rb;
    public Vector2 moveInput { get; set; }
    public Vector2 lastDir { get; private set; } = Vector2.down;
    private Animator animator;

    [Header("Sound")]
    [SerializeField] SoundData floorStep;
    [SerializeField] SoundData snowStep;
    [SerializeField] SoundData sandStep;
    [SerializeField] SoundData earthStep;
    [SerializeField] float stepInterval = 0.4f;
    EnumSerfaceArea currentSurface = EnumSerfaceArea.Floor;
    float stepTimer = 0f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    public float dashEnergyCost = 15f;

    [Header("Dash Effects")]
    public GameObject dashTrailPrefab;

    private bool isDashing;
    private float dashCooldownTimer;
    private Vector2 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
    }
    private void Update()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // Hỗ trợ phím Space để Dash nếu chưa map Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryDash();
        }

        if (moveInput.sqrMagnitude > 0.01f && !isDashing)
        {
            PlayStepSound();
        }
    }
    private void FixedUpdate()
    {

        PlayerController playerController = GetComponent<PlayerController>();
        if(playerController != null && playerController.IsKnockback())
        {
            return; // Bỏ qua việc di chuyển nếu đang bị knockback
        }

        if (isDashing)
        {
            rb.velocity = dashDirection * dashSpeed;
            return;
        }

        rb.velocity = moveSpeed * moveInput;
    }
    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        // KIỂM TRA: Nếu người dùng đang nhấn phím (độ dài vector > 0)
        if (moveInput.sqrMagnitude > 0.001f)
        {
            // Lưu lại hướng hiện tại vào biến 'lastDir'
            lastDir = moveInput.normalized;

            animator.SetBool("isWalking", true);

            // Cập nhật hướng cho Blend Tree lúc đang đi
            if (!isDashing)
            {
                animator.SetFloat("MoveX", moveInput.x);
                animator.SetFloat("MoveY", moveInput.y);
            }
         
        }
        if (moveInput == Vector2.zero)
        {
            animator.SetBool("isWalking",false);
            if (!isDashing)
            {
                animator.SetFloat("lastInputX",lastDir.x);
                animator.SetFloat("lastInputY",lastDir.y);
            }
            stepTimer = 0f;
        }
        
    }

    public void TryDash()
    {
        if (!isDashing && dashCooldownTimer <= 0f)
        {
            // Kiểm tra và trừ năng lượng trước khi cho lướt
            if (controller != null && !controller.TryConsumeEnergy(dashEnergyCost))
            {
                return; // KO lướt nếu rỗng năng lượng
            }
            StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        dashDirection = (moveInput.sqrMagnitude > 0.01f) ? moveInput.normalized : lastDir;
        
        // Thêm trạng thái vô địch tạm thời cho người chơi khi đang lướt
        if (controller != null)
        {
            controller.ActivateInvincibleForDash(dashDuration);
        }

        // Tạo bóng mờ gắt lại phía sau 0.4 đơn vị
        if (dashTrailPrefab != null)
        {
            Vector3 trailPos = transform.position - (Vector3)(dashDirection * 0.4f);
            GameObject trail = Instantiate(dashTrailPrefab, trailPos, transform.rotation);
            Destroy(trail, 1.2f); // tự làm mờ/hủy sau một phút ngắn (cần script mờ dần nếu muốn đẹp)
        }

        yield return new WaitForSeconds(dashDuration);
        
        isDashing = false;
        // Cập nhật lại hướng sau khi lướt
        if (moveInput == Vector2.zero)
        {
            animator.SetFloat("lastInputX", lastDir.x);
            animator.SetFloat("lastInputY", lastDir.y);
        }
        else
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        SerfaceArea area = col.GetComponent<SerfaceArea>();
        if (area != null)
        {
            currentSurface = area.serfaceType;
        }
    }
    void PlayStepSound()
    {
        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            SoundData sound = floorStep;

            switch (currentSurface)
            {
                case EnumSerfaceArea.Sand:
                    sound = sandStep;
                    break;
                case EnumSerfaceArea.snow:
                    sound = snowStep;
                    break;
                case EnumSerfaceArea.Earth:
                    sound = earthStep;
                    break;
            }

            if (SoundManager.Instance != null && sound != null)
                SoundManager.Instance.PlaySound(sound);

            stepTimer = stepInterval;
        }
    }
}
