// PLAYER MOVEMENT
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private int extraJumpsAllowed = 1; // Number of double jumps
    private int extraJumpsLeft;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;
    private bool isGrounded;

    [Header("References")]
    [SerializeField] private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        extraJumpsLeft = extraJumpsAllowed;
    }

    void Update()
    {
        float moveX = 0f;

        // 1. Handle Left/Right Input (New Input System)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                moveX = -1f;
            }
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                moveX = 1f;
            }
        }

        // 2. Flip Sprite Direction based on movement
        if (moveX != 0f)
        {
            spriteRenderer.flipX = moveX < 0f;
        }

        // 3. Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            extraJumpsLeft = extraJumpyReset(); // Reset double jumps when grounded
        }

        // 4. Jump & Double Jump Input
        bool jumpPressed = Keyboard.current != null && 
            (Keyboard.current.spaceKey.wasPressedThisFrame || 
             Keyboard.current.wKey.wasPressedThisFrame || 
             Keyboard.current.upArrowKey.wasPressedThisFrame);

        if (jumpPressed)
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (extraJumpsLeft > 0)
            {
                Jump();
                extraJumpsLeft--;
                animator.SetTrigger("DoubleJump"); // Fires the mid-air double jump animation trigger
            }
        }

        // 5. Apply Horizontal Movement (Unity 6 linearVelocity)
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);

        // 6. Push Parameters to Animator
        animator.SetFloat("Speed", Mathf.Abs(moveX));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private int extraJumpyReset()
    {
        return extraJumpsAllowed;
    }

    // Visualize the ground check in the Scene view
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}