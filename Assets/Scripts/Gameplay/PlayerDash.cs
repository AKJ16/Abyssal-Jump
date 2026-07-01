using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    // Attributes

    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float verticalDashEndMultiplier = 0.85f;
    [SerializeField] private float groundedDownDashHop = 6f;
    [SerializeField] private float groundedDownDashDuration = 0.1f;

    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioClip dashEffectClip;

    private Rigidbody2D rb;
    private PlayerState state;
    private Animator anim;
    private SpriteRenderer sr;

    private float originalGravity;
    private bool canDash = true;
    private bool airDash = true;
    private bool wasGrounded;
    private bool ignoreCooldownReset;
    private bool groundedDuringDash = false;
    private Coroutine cooldownCoroutine;

    // Methods

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        state = GetComponent<PlayerState>();
        originalGravity = rb.gravityScale;
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void UpdateDashLogic(bool isGrounded)
    {
        if (!wasGrounded && isGrounded)
        {
            airDash = true;

            if (state.isDashing)
            {
                groundedDuringDash = true;
            }
            else if (!state.isDashing && !ignoreCooldownReset && !canDash)
            {
                if (cooldownCoroutine != null)
                {
                    if (PlayerController.Instance != null)
                    {
                        StartCoroutine(DashPulseSequence());
                    }
                    StopCoroutine(cooldownCoroutine);
                    cooldownCoroutine = null;
                }
            }
        }

        if (!state.isDashing && groundedDuringDash)
        {
            if (cooldownCoroutine != null)
            {
                if (PlayerController.Instance != null)
                {
                    StartCoroutine(DashPulseSequence());
                }
                StopCoroutine(cooldownCoroutine);
                cooldownCoroutine = null;
            }
        }

        wasGrounded = isGrounded;
    }

    public void AttemptDash(Vector2 inputDir)
    {
        if (!state.hasDash && !state.hasOmniDash) return;
        if (!canDash || !airDash) return;
        canDash = false;

        if (!state.isGrounded)
        {
            airDash = false;
        }

        StartCoroutine(ExecuteDash(inputDir));
    }

    private IEnumerator ExecuteDash(Vector2 inputDir)
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.StopFlash();
        }

        if (AudioManager.Instance != null && dashClip != null)
        {
            AudioManager.Instance.PlaySFX(dashClip);
            AudioManager.Instance.PlaySFX(dashEffectClip);
        }

        Color originalColor = sr.color;
        originalColor.a = 1f;
        Vector2 dashDir = inputDir;

        if (!state.hasOmniDash)
        {
            dashDir.y = 0f;

            if (Mathf.Abs(dashDir.x) < 0.1f)
            {
                dashDir.x = state.isLookingRight ? 1f : -1f;
            }
        }
        else
        {
            if (dashDir == Vector2.zero)
            {
                dashDir = new Vector2(state.isLookingRight ? 1 : -1, 0);
            }
            else if (state.isGrounded && dashDir.y < -0.5f)
            {
                ignoreCooldownReset = true;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, groundedDownDashHop);
                anim.SetBool("Jumping", true);
                yield return new WaitForSeconds(groundedDownDashDuration);
            }
        }

        dashDir.Normalize();
        state.isDashing = true;
        sr.color = new Color(1.06f, 1.62f, 1.96f, 1f);

        int playerLayer = gameObject.layer;
        int barrierLayer = LayerMask.NameToLayer("Barrier");

        if (barrierLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, barrierLayer, true);
        }

        rb.gravityScale = 0;
        rb.linearVelocity = dashDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * verticalDashEndMultiplier);
        state.isDashing = false;

        yield return StartCoroutine(ReturnColor(sr.color, originalColor, dashDuration));

        if (barrierLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, barrierLayer, false);
        }

        cooldownCoroutine = StartCoroutine(DashCooldown());
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        ignoreCooldownReset = false;
        cooldownCoroutine = null;

        if (PlayerController.Instance != null)
        {
            StartCoroutine(DashPulseSequence());
        }
    }

    private IEnumerator ReturnColor(Color start, Color end, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = t / duration;

            sr.color = Color.Lerp(start, end, normalized);

            yield return null;
        }

        sr.color = end;
    }

    private IEnumerator DashPulseSequence()
    {
        Color originalColor = sr.color;
        originalColor.a = 1f;

        Color pulseColor = new Color(1.4f, 1.4f, 1.4f, 1f);
        sr.color = pulseColor;
        yield return new WaitForSeconds(0.08f);

        float elapsed = 0f;
        float fadeDuration = 0.15f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            sr.color = Color.Lerp(pulseColor, originalColor, elapsed / fadeDuration);
            yield return null;
        }

        sr.color = originalColor;
        canDash = true;
    }
}
