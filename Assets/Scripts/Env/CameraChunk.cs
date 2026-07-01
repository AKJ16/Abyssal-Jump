using UnityEngine;

public class CameraChunk : MonoBehaviour
{
    // Attributes

    private Collider2D chunkCollider;

    // Methods

    private void Awake()
    {
        chunkCollider = GetComponent<Collider2D>();
        chunkCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetBoundary(chunkCollider);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.ClearBoundary(chunkCollider);
            }
        }
    }
}
