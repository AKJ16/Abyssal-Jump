using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    // Attributes

    [SerializeField] private float jumpForce = 45f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private int maxAirJumps = 1;
    [SerializeField][Range(0f, 1f)] private float variableJumpCutoff = 0.5f;

    [SerializeField] private AudioClip jumpClip;

    private Rigidbody2D rb;
    private PlayerState state;
    private Animator anim;

    private float jumpBufferCounter;
    private float coyoteTimeCounter;
    private int airJumpCounter;

    // Methods

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        state = GetComponent<PlayerState>();
        anim = GetComponent<Animator>();
    }

    public void UpdateJumpLogic(bool isGrounded)
    {
        if (!state.hasJump)
        {
            state.isFalling = rb.linearVelocity.y < -0.1f && !isGrounded;
            if (isGrounded)
            {
                state.isJumping = false;
            }
            return;
        }

        jumpBufferCounter -= Time.deltaTime;

        if (isGrounded)
        {
            ResetJumpCapabilities();
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            PerformJump();
        }
    }

    private void ResetJumpCapabilities()
    {

        state.isJumping = false;
        airJumpCounter = 0;
        coyoteTimeCounter = coyoteTime;
    }

    public void HandleJumpPressed()
    {
        if (!state.hasJump) return;
        jumpBufferCounter = jumpBufferTime;

        if (coyoteTimeCounter <= 0 && airJumpCounter < maxAirJumps)
        {
            if (!state.hasDoubleJump) return;
            PerformJump();
            airJumpCounter++;
        }
    }

    public void HandleJumpReleased()
    {
        if (rb.linearVelocity.y > 0f && !state.isDashing && !state.hit)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpCutoff);
        }
    }

    private void PerformJump()
    {
        if (AudioManager.Instance != null && jumpClip != null)
        {
            AudioManager.Instance.PlaySFX(jumpClip);
        }
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
        state.isJumping = true;
    }
}
