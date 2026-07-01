using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum AbilityType
{
    Jump,
    DoubleJump,
    Dash,
    OmniDash
}

public class AbilityPlant : MonoBehaviour
{
    // Attributes

    [SerializeField] private AbilityType abilityToUnlock;
    [SerializeField] private Transform targetPosition;
    [SerializeField] private float dragDuration = 0.7f;

    [SerializeField] private AudioClip unlockSFX;

    private Animator anim;
    private Collider2D col;
    private bool isCollected;
    private string uniqueID;

    // Methods

    private void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        uniqueID = $"PL_{sceneName}_{transform.position.x}_{transform.position.y}_{transform.position.z}";

        if (PlayerPrefs.GetInt(uniqueID, 0) == 1)
        {
            isCollected = true;
            if (anim != null)
            {
                anim.SetTrigger("Collected");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;
            col.enabled = false;

            PlayerPrefs.SetInt(uniqueID, 1);
            PlayerPrefs.Save();

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                StartCoroutine(CollectSequence(player));
            }
        }
    }

    private IEnumerator CollectSequence(PlayerController player)
    {
        if (AudioManager.Instance != null && unlockSFX != null)
        {
            AudioManager.Instance.PlaySFX(unlockSFX);
        }

        PlayerState state = player.GetComponent<PlayerState>();
        if (state != null)
        {
            state.hit = true;
            state.collect = true;
        }

        player.rb.linearVelocity = Vector2.zero;
        player.rb.simulated = false;

        if (targetPosition != null)
        {
            float elapsed = 0f;
            Vector3 startPos = player.transform.position;

            while (elapsed < dragDuration)
            {
                elapsed += Time.deltaTime;
                player.transform.position = Vector3.Lerp(startPos, targetPosition.position, elapsed / dragDuration);
                yield return null;
            }
            player.transform.position = targetPosition.position;
        }

        if (anim != null)
        {
            anim.SetTrigger("Collected");
        }

        UnlockAbility(state);
    }

    private void UnlockAbility(PlayerState state)
    {
        switch (abilityToUnlock)
        {
            case AbilityType.Jump:
                state.hasJump = true;
                break;
            case AbilityType.DoubleJump:
                state.hasDoubleJump = true;
                break;
            case AbilityType.Dash:
                state.hasDash = true;
                break;
            case AbilityType.OmniDash:
                state.hasOmniDash = true;
                break;
        }

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowTutorial(abilityToUnlock);
        }
    }
}
