using UnityEngine;

public class EndDoor : MonoBehaviour
{
    // Attributes

    [SerializeField] private Transform targetAnchor;
    private bool isTriggered;

    // Methods

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            isTriggered = true;

            if (EndManager.Instance != null && targetAnchor != null)
            {
                EndManager.Instance.TriggerEndSequence(targetAnchor.position);
            }
        }
    }
}
