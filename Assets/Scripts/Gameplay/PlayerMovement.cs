using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Attributes

    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float airControlMultiplier = 2f;
    [SerializeField] private float excessMomentum = 0.25f;

    private Rigidbody2D rb;
    private PlayerState state;
    private float momentumCounter;

    // Methods

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        state = GetComponent<PlayerState>();
    }

    public void Move(float inputX)
    {
        float currentX = rb.linearVelocity.x;
        float targetSpeed = inputX * walkSpeed;
        bool hasInput = Mathf.Abs(inputX) > 0.01f;

        bool preservingMomentum =!state.isGrounded && hasInput && Mathf.Abs(currentX) > walkSpeed && Mathf.Sign(currentX) == Mathf.Sign(inputX);

        if (preservingMomentum)
        {
            float momentumTarget = Mathf.Sign(currentX) * walkSpeed;
            float decayedX = Mathf.MoveTowards(currentX, momentumTarget, excessMomentum * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(decayedX, rb.linearVelocity.y);
            return;
        }

        float accelRate;

        if (state.isGrounded)
        {
            accelRate = Mathf.Abs(inputX) > 0.01f ? acceleration : deceleration;
        }
        else
        {
            accelRate = Mathf.Abs(inputX) > 0.01f ? acceleration * airControlMultiplier : deceleration * airControlMultiplier;
        }

        if (!state.isGrounded && Mathf.Abs(inputX) > 0.01f && Mathf.Abs(rb.linearVelocity.x) > walkSpeed && Mathf.Sign(rb.linearVelocity.x) == Mathf.Sign(inputX))
        {
            return;
        }

        float finalX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(finalX, rb.linearVelocity.y);

        state.isWalking = Mathf.Abs(inputX) > 0.01f && state.isGrounded;
    }

    public void PreserveMomentum()
    {
        momentumCounter = excessMomentum;
    }
}
