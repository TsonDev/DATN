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
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
    }
    private void Update()
    {
        if (moveInput.sqrMagnitude > 0.01f)
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
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
         
        }
        if (moveInput == Vector2.zero)
        {
            animator.SetBool("isWalking",false);
            animator.SetFloat("lastInputX",lastDir.x);
            animator.SetFloat("lastInputY",lastDir.y);
            stepTimer = 0f;
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

            SoundManager.Instance.PlaySound(sound);

            stepTimer = stepInterval;
        }
    }
}
