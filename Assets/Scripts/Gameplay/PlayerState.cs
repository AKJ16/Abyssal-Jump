using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [Header("Directions")]
    public bool isLookingRight = true;

    [Header("Condition")]
    public bool isGrounded;
    public bool invincible;
    public bool hit;
    public bool collect = false;

    [Header("Movement")]
    public bool isWalking;
    public bool isJumping;
    public bool isFalling;
    public bool isWallJumping;
    public bool isDashing;

    [Header("Unlocked Abilities")]
    public bool hasJump = false;       
    public bool hasDoubleJump = false; 
    public bool hasDash = false;
    public bool hasOmniDash = false;
}
