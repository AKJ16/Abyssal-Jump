using UnityEngine;

public class CameraZone : MonoBehaviour
{
    // Attributes

    [SerializeField] private float customYOffset = 0f;

    // Methods

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.RegisterZone(this, customYOffset);
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
                camFollow.UnregisterZone(this);
            }
        }
    }

    public float GetCustomYOffset()
    {
        return customYOffset;
    }
}
