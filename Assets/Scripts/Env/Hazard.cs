using UnityEngine;

public class Hazard : MonoBehaviour
{
    // Attributes

    [SerializeField] private AudioClip hazardImpactClip;

    // Methods

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (AudioManager.Instance != null && hazardImpactClip != null)
            {
                AudioManager.Instance.PlaySFX(hazardImpactClip);
            }

            GameManager.Instance.Respawn();
        }
    }
}
