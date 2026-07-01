using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    // Attributes

    public static TutorialManager Instance { get; private set; }

    [SerializeField] private CanvasGroup overlayCanvas;
    [SerializeField] private CanvasGroup tutorialCanvas;
    [SerializeField] private GameObject[] tutorialPanels;
    [SerializeField] private float screenFadeInDuration = 0.4f;
    [SerializeField] private float tutorialFadeInDuration = 0.6f;
    [SerializeField] private float fadeOutDuration = 0.4f;
    [Range(0f, 1f)][SerializeField] private float targetBackgroundAlpha = 0.85f;

    [SerializeField] private AudioClip unlockSFX;

    private bool isTutorialActive;
    private bool isFullyComplete;
    private Coroutine fadeCoroutine;

    // Methods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (overlayCanvas != null)
        {
            overlayCanvas.alpha = 0f;
            overlayCanvas.blocksRaycasts = false;
            overlayCanvas.interactable = false;
        }

        if (tutorialCanvas != null)
        {
            tutorialCanvas.alpha = 0f;
            tutorialCanvas.blocksRaycasts = false;
            tutorialCanvas.interactable = false;
        }

        foreach (var panel in tutorialPanels)
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    private void Update()
    {
        if (isTutorialActive && isFullyComplete && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CloseTutorial();
        }
    }

    public void ShowTutorial(AbilityType abilityType)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(ShowSequence(abilityType));
    }

    private IEnumerator ShowSequence(AbilityType abilityToUnlock)
    {
        isActiveState(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutBGM(0.7f);
        }

        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StopTimer();
        }

        if (overlayCanvas != null)
        {
            float elapsed = 0f;
            while (elapsed < screenFadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                overlayCanvas.alpha = Mathf.Lerp(0f, targetBackgroundAlpha, elapsed / screenFadeInDuration);
                yield return null;
            }
            overlayCanvas.alpha = targetBackgroundAlpha;
            overlayCanvas.blocksRaycasts = true;
            overlayCanvas.interactable = true;
        }
        else
        {
            yield return new WaitForSecondsRealtime(screenFadeInDuration);
        }

        if (AudioManager.Instance != null && unlockSFX != null)
        {
            AudioManager.Instance.PlaySFX(unlockSFX);
        }


        yield return new WaitForSecondsRealtime(0.5f);

        int index = (int)abilityToUnlock;
        for (int i = 0; i < tutorialPanels.Length; i++)
        {
            if (tutorialPanels[i] != null)
            {
                tutorialPanels[i].SetActive(i == index);
            }
        }

        if (tutorialCanvas != null)
        {
            float elapsed = 0f;
            while (elapsed < tutorialFadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                tutorialCanvas.alpha = Mathf.Lerp(0f, 1f, elapsed / tutorialFadeInDuration);
                yield return null;
            }
            tutorialCanvas.alpha = 1f;
            tutorialCanvas.blocksRaycasts = true;
            tutorialCanvas.interactable = true;
        }

        isFullyComplete = true;
    }

    public void CloseTutorial()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(CloseSequence());
    }

    private IEnumerator CloseSequence()
    {
        float startOverlayAlpha = overlayCanvas != null ? overlayCanvas.alpha : 0f;
        float startTutorialAlpha = tutorialCanvas != null ? tutorialCanvas.alpha : 0f;
        
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeOutDuration;

            if (tutorialCanvas != null)
            {
                tutorialCanvas.alpha = Mathf.Lerp(startTutorialAlpha, 0f, t);
            }

            yield return null;
        }

        if (tutorialCanvas != null)
        {
            tutorialCanvas.alpha = 0f;
            tutorialCanvas.blocksRaycasts = false;
            tutorialCanvas.interactable = false;
        }

        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeOutDuration;

            if (overlayCanvas != null)
            {
                overlayCanvas.alpha = Mathf.Lerp(startOverlayAlpha, 0f, t);
            }

            yield return null;
        }

        if (overlayCanvas != null)
        {
            overlayCanvas.alpha = 0f;
            overlayCanvas.blocksRaycasts = false;
            overlayCanvas.interactable = false;
        }

        foreach (var panel in tutorialPanels)
        {
            if (panel != null) panel.SetActive(false);
        }

        if (PlayerController.Instance != null)
        {
            PlayerState playerState = PlayerController.Instance.GetComponent<PlayerState>();
            if (playerState != null)
            {
                playerState.hit = false;
                playerState.collect = false;
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeInBGM(0.5f);
        }

        isActiveState(false);
    }

    private void isActiveState(bool active)
    {
        isTutorialActive = active;

        if (active)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.StartTimer();
            }

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.rb.simulated = true;
                PlayerState playerState = PlayerController.Instance.GetComponent<PlayerState>();
                if (playerState != null)
                {
                    playerState.hit = false;
                }
            }
        }
    }
}
