using UnityEngine;

public enum TransitionType
{
    Horizontal,
    Vertical
}

public class SceneTransition : MonoBehaviour
{
    // Attributes

    [SerializeField] private string sceneToLoad;
    [SerializeField] private string targetEntranceID;
    [SerializeField] private float exitSpeed = 5f;
    [SerializeField] private float exitDuration = 0.6f;

    private bool isTransitioning;
    [SerializeField] private TransitionType transitionType = TransitionType.Horizontal;

    // Methods

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning) return;

        if (other.CompareTag("Player"))
        {
            PlayerState playerState = other.GetComponent<PlayerState>();
            if (playerState != null && playerState.hit) return;

            isTransitioning = true;
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            Vector2 calculatedDirection = Vector2.right;

            if (playerRb != null)
            {
                float velX = playerRb.linearVelocity.x;
                float velY = playerRb.linearVelocity.y;

                if (transitionType == TransitionType.Vertical)
                {
                    calculatedDirection = velY >= 0f ? Vector2.up : Vector2.down;
                }
                else
                {
                    if (Mathf.Abs(velX) > 0.1f)
                    {
                        calculatedDirection = velX > 0f ? Vector2.right : Vector2.left;
                    }
                    else if (playerState != null)
                    {
                        calculatedDirection = playerState.isLookingRight ? Vector2.right : Vector2.left;
                    }
                }
            }

            GameManager.Instance.TransitionToScene(sceneToLoad, targetEntranceID, calculatedDirection, exitSpeed, exitDuration);
        }
    }
}
