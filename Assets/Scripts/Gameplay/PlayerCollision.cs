using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    // Attributes

    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.2f);
    [SerializeField] private LayerMask whatIsGround;

    // Methods

    public bool CheckGrounded()
    {
        return Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, whatIsGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (groundCheckPoint)
        {
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
    }
}
