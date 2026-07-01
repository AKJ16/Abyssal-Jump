using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Attributes

    public static PlayerController Instance { get; private set; }

    private PlayerState state;
    private PlayerCollision collision;
    private PlayerMovement movement;
    private PlayerJump jump;
    private PlayerDash dash;

    [SerializeField] private float flashDuration = 1f;
    [SerializeField] private float flashInterval = 0.1f;

    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float footstepInterval = 0.32f;
    private float footstepTimer;

    [SerializeField] private AudioClip landingSFX;
    [SerializeField] private AudioClip respawnSFX;
    private bool wasGrounded;

    private Vector2 moveInput;
    public Rigidbody2D rb { get; private set; }
    private SpriteRenderer sr;
    private Coroutine flashCoroutine;

    // Methods
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Instance = this;
        state = GetComponent<PlayerState>();
        collision = GetComponent<PlayerCollision>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        movement = GetComponent<PlayerMovement>();
        jump = GetComponent<PlayerJump>();
        dash = GetComponent<PlayerDash>();
    }

    private void Update()
    {
        state.isGrounded = collision.CheckGrounded();
        dash.UpdateDashLogic(state.isGrounded);

        if (!wasGrounded && state.isGrounded)
        {
            if (AudioManager.Instance != null && landingSFX != null)
            {
                AudioManager.Instance.PlaySFX(landingSFX);
            }
        }
        wasGrounded = state.isGrounded;

        if (state.isDashing || state.hit) return;

        jump.UpdateJumpLogic(state.isGrounded);
        HandleFlip();
    }

    private void FixedUpdate()
    {
        if (state.isDashing || state.hit) return;
        movement.Move(moveInput.x);

        if (state.isWalking && state.isGrounded)
        {
            footstepTimer -= Time.fixedDeltaTime;
            if (footstepTimer <= 0f)
            {
                if (AudioManager.Instance != null && footstepClip != null)
                {
                    AudioManager.Instance.PlaySFX(footstepClip);
                }
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f; 
        }
    }

    // --- INPUT HANDLERS ---
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (Mathf.Abs(moveInput.x) > 0.01f)
            moveInput.x = Mathf.Sign(moveInput.x);
        else
            moveInput.x = 0f;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (state.hit) return;

        if (context.performed)
        {
            jump.HandleJumpPressed();
        }

        if (context.canceled)
        {
            jump.HandleJumpReleased();
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (state.hit) return;
         
        if (context.performed)
        {
            dash.AttemptDash(moveInput);
        }
    }

    // --- UTILITIES ---
    private void HandleFlip()
    {
        if (moveInput.x > 0 && !state.isLookingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && state.isLookingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        if (state.isDashing || state.hit) return;

        state.isLookingRight = !state.isLookingRight;
        transform.localScale = new Vector3(state.isLookingRight ? 1 : -1, 1, 1);
    }

    public void SetFacingDirection(bool faceRight)
    {
        state.isLookingRight = faceRight;
        transform.localScale = new Vector3(faceRight ? 1 : -1, 1, 1);
    }

    public void StartFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashSequence());
    }

    public void StopFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (sr != null)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        }
        state.invincible = false;
    }

    public IEnumerator FlashSequence()
    {
        state.invincible = true;
        float elapsed = 0f;
        Color originalColor = sr.color;

        while (elapsed < flashDuration)
        {
            float targetAlpha = (sr.color.a == 1f) ? 0.3f : 1f;
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, targetAlpha);

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        sr.color = originalColor;
        state.invincible = false;
        flashCoroutine = null;
    }

    public void TriggerRespawnSFX()
    {
        if (AudioManager.Instance != null && respawnSFX != null)
        {
            AudioManager.Instance.PlaySFX(respawnSFX);
        }
    }
}
