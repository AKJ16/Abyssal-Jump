using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EndManager : MonoBehaviour
{
    // Attributes

    public static EndManager Instance { get; private set; }

    [SerializeField] private CanvasGroup whiteFader;
    [SerializeField] private CanvasGroup blackFader;
    [SerializeField] private CanvasGroup textGroup;

    [SerializeField] private TextMeshProUGUI congratulationsText;
    [SerializeField] private TextMeshProUGUI extraTextOne;
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private TextMeshProUGUI bestTimeText;
    [SerializeField] private TextMeshProUGUI extraTextTwo;
    [SerializeField] private TextMeshProUGUI continueText;

    [SerializeField] private float endDuration = 0.8f;      
    [SerializeField] private float fadeDuration = 1.5f;       
    [SerializeField] private float textDuration = 1.0f;
    [SerializeField] private float exitDuration = 0.5f;

    [SerializeField] private AudioClip endingMusicClip;
    [SerializeField] private float endingMusicVolume = 0.8f;

    private bool isSequenceComplete;

    // Methods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (whiteFader != null)
        {
            whiteFader.alpha = 0f;
            whiteFader.blocksRaycasts = false;
            whiteFader.interactable = false;
        }

        if (blackFader != null)
        {
            blackFader.alpha = 0f;
            blackFader.blocksRaycasts = false;
            blackFader.interactable = false;
        }

        if (textGroup != null)
        {
            textGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        if (isSequenceComplete && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ReturnToMenu();
        }
    }

    public void TriggerEndSequence(Vector3 doorTargetPos)
    {
        StartCoroutine(EndSequence(doorTargetPos));
    }

    private IEnumerator EndSequence(Vector3 doorTargetPos)
    {
        PlayerController player = PlayerController.Instance;
        if (player == null) yield break;

        PlayerState state = player.GetComponent<PlayerState>();

        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StopTimer();
        }

        if (state != null) state.hit = true;
        player.rb.linearVelocity = Vector2.zero;
        player.rb.simulated = false;

        float elapsed = 0f;
        Vector3 startPos = player.transform.position;
        while (elapsed < endDuration)
        {
            elapsed += Time.deltaTime;
            player.transform.position = Vector3.Lerp(startPos, doorTargetPos, elapsed / endDuration);
            yield return null;
        }
        player.transform.position = doorTargetPos;
        state.isWalking = false;

        yield return new WaitForSeconds(1f);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllAudio(fadeDuration);
        }

        elapsed = 0f;
        if (whiteFader != null)
        {
            whiteFader.blocksRaycasts = true;
            whiteFader.interactable = true;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                whiteFader.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            whiteFader.alpha = 1f;
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        yield return new WaitForSeconds(1f);

        if (AudioManager.Instance != null && endingMusicClip != null)
        {
            AudioManager.Instance.PlayEndingMusic(endingMusicClip);
            AudioManager.Instance.FadeInEndingMusic(fadeDuration, endingMusicVolume);
        }

        int finalScore = TimerManager.Instance != null ? Mathf.RoundToInt(TimerManager.Instance.GetElapsedTime() * 1000f) : 0;
        int highscore = SaveSystem.LoadBestTime();

        if (highscore == 0)
        {
            highscore = finalScore;
            SaveSystem.SaveBestTime(finalScore);
        }
        else if (finalScore < highscore) 
        {
            highscore = finalScore;
            SaveSystem.SaveBestTime(finalScore);
        }

        float finalTimeSec = finalScore / 1000f;
        float bestTimeSec = highscore / 1000f;

        string finalTimeStr = TimerManager.Instance != null ? TimerManager.Instance.GetFormattedTime(finalTimeSec) : "00:00:00:000";
        string bestTimeStr = TimerManager.Instance != null ? TimerManager.Instance.GetFormattedTime(bestTimeSec) : "00:00:00:000";

        Debug.Log(finalTimeSec);
        Debug.Log(bestTimeSec);
        Debug.Log(finalTimeStr);
        Debug.Log(bestTimeStr);

        if (finalTimeText != null)
        {
            finalTimeText.text = "FINAL TIME [" + finalTimeStr + "]";
        }

        if (bestTimeText != null)
        {
            bestTimeText.text = "BEST TIME [" + bestTimeStr + "]";
        }

        DeactivateAllTexts();

        if (textGroup != null)
        {
            textGroup.alpha = 1f;
        }

        if (congratulationsText != null)
        {
            yield return StartCoroutine(FadeInText(congratulationsText, 0.4f));
        }
        yield return new WaitForSeconds(textDuration);

        if (extraTextOne != null)
        {
            yield return StartCoroutine(FadeInText(extraTextOne, 0.4f));
        }
        yield return new WaitForSeconds(textDuration);

        if (finalTimeText != null)
        {
            yield return StartCoroutine(FadeInText(finalTimeText, 0.4f));
        }
        yield return new WaitForSeconds(textDuration);

        if (bestTimeText != null)
        {
            yield return StartCoroutine(FadeInText(bestTimeText, 0.4f));
        }
        yield return new WaitForSeconds(textDuration);

        if (extraTextTwo != null)
        {
            yield return StartCoroutine(FadeInText(extraTextTwo, 0.4f));
        }
        yield return new WaitForSeconds(textDuration);

        if (continueText != null)
        {
            yield return StartCoroutine(FadeInText(continueText, 0.4f));
        }
        isSequenceComplete = true;
    }

    private void DeactivateAllTexts()
    {
        if (congratulationsText != null) congratulationsText.gameObject.SetActive(false);
        if (extraTextOne != null) extraTextOne.gameObject.SetActive(false);
        if (finalTimeText != null) finalTimeText.gameObject.SetActive(false);
        if (bestTimeText != null) bestTimeText.gameObject.SetActive(false);
        if (extraTextTwo != null) extraTextTwo.gameObject.SetActive(false);
        if (continueText != null) continueText.gameObject.SetActive(false);
    }

    private IEnumerator FadeInText(TextMeshProUGUI text, float duration)
    {
        text.gameObject.SetActive(true);
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
    }

    private void ReturnToMenu()
    {
        isSequenceComplete = false;
        StartCoroutine(ReturnToMenuSequence());
    }

    private IEnumerator ReturnToMenuSequence()
    {
        float elapsed = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutEndingMusic(exitDuration);
        }

        if (blackFader != null)
        {
            blackFader.blocksRaycasts = true;
            blackFader.interactable = true;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                blackFader.alpha = Mathf.Lerp(0f, 1f, elapsed / exitDuration);
                yield return null;
            }
            blackFader.alpha = 1f;
        }

        if (GameManager.Instance != null)
        {
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.ResetTimer();
                TimerManager.Instance.StopTimer();
            }

            if (PlayerController.Instance != null)
            {
                Destroy(PlayerController.Instance.gameObject);
            }

            SceneManager.LoadScene(GameManager.Instance.GetMainMenuScene());
        }
    }
}
