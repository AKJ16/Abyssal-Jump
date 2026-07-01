using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPoint : MonoBehaviour
{
    // Attributes

    [SerializeField] private AudioClip activeSFX;

    private Animator anim;
    private bool isActive;
    private string uniqueID;

    // Methods

    private void Awake()
    {
        anim = GetComponent<Animator>();
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        uniqueID = $"CP_{sceneName}_{transform.position.x}_{transform.position.y}_{transform.position.z}";

        if (PlayerPrefs.GetInt(uniqueID, 0) == 1)
        {
            isActive = true;
            if (anim != null)
            {
                anim.Play("Active");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActive) return;

        if (other.CompareTag("Player"))
        {
            isActive = true;

            if (anim != null)
            {
                anim.SetTrigger("Active"); 
            }

            PlayerPrefs.SetInt(uniqueID, 1);
            PlayerPrefs.Save();

            if (AudioManager.Instance != null && activeSFX != null)
            {
                AudioManager.Instance.PlaySFX(activeSFX);
            }

            GameManager.Instance.UpdateSpawnPoint(transform.position, this);
        }
    }

    public void Deactivate()
    {
        isActive = false;
        PlayerPrefs.SetInt(uniqueID, 0);
        PlayerPrefs.Save();

        if (anim != null)
        {
            anim.SetTrigger("Deactivate");
        }
    }
}
