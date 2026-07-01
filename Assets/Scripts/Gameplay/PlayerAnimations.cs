using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    // Attributes

    private Animator anim;
    private PlayerState state;
    private Rigidbody2D rb;

    private bool wasDashing;

    // Methods

    private void Awake()
    {
        anim = GetComponent<Animator>();
        state = GetComponent<PlayerState>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        anim.SetBool("Walking", state.isWalking);
        anim.SetBool("Grounded", state.isGrounded);

        anim.SetBool("Jumping", rb.linearVelocity.y > 0.1f && !state.isGrounded);
        anim.SetBool("Falling", state.collect || rb.linearVelocity.y < -0.1f && !state.isGrounded);

        if (state.isDashing && !wasDashing)
        {
            anim.SetTrigger("Dashing");
        }
        wasDashing = state.isDashing;
    }
}
