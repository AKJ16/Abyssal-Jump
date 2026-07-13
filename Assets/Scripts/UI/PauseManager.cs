using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // Attributes

    public static PauseManager Instance { get; private set; }

    [SerializeField] private CanvasGroup overlayCanvas; 
    [SerializeField] private CanvasGroup buttonsGroup;    
    [SerializeField] private CanvasGroup exitFader;    
    [SerializeField] private GameObject firstSelect; 

    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float buttonDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float exitDuration = 0.5f;
    [Range(0f, 1f)][SerializeField] private float targetAlpha = 0.9f;

    private bool isPaused;
    private bool isTransitioning;
    private Coroutine transitionCoroutine;
    private GameObject lastSelected; 

    // Methods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SetGroupState(overlayCanvas, 0f, false);
        SetGroupState(buttonsGroup, 0f, false);
        SetGroupState(exitFader, 0f, false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isTransitioning)
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        if (isPaused && EventSystem.current != null)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(lastSelected);
            }
            else
            {
                lastSelected = EventSystem.current.currentSelectedGameObject;
            }
        }
    }

    public void PauseGame()
    {
        MenuButton button = firstSelect.GetComponent<MenuButton>();
        if (button != null)
        {
            button.setFirstSelect();
        }

        isPaused = true;
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(PauseSequence());
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(ResumeSequence());
    }

    public void ReturnToMenu()
    {
        isPaused = false;
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        StartCoroutine(ReturnToMenuSequence());
    }

    private IEnumerator PauseSequence()
    {
        isTransitioning = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.MuffleBGM(0.3f);
        }

        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StopTimer();
        }

        if (PlayerController.Instance != null)
        {
            PlayerState playerState = PlayerController.Instance.GetComponent<PlayerState>();
            if (playerState != null)
            {
                playerState.hit = true;
            }
        }

        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime; 
                SetGroupState(overlayCanvas, Mathf.Lerp(0f, targetAlpha, elapsed / fadeInDuration), true);
                yield return null;
            }
            SetGroupState(overlayCanvas, targetAlpha, true);
        }
        else
        {
            yield return new WaitForSecondsRealtime(fadeInDuration);
        }

        if (buttonsGroup != null)
        {
            buttonsGroup.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < buttonDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetGroupState(buttonsGroup, Mathf.Lerp(0f, 1f, elapsed / buttonDuration), true);
                yield return null;
            }
            SetGroupState(buttonsGroup, 1f, true);
        }

        if (EventSystem.current != null && firstSelect != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelect);
            lastSelected = firstSelect;
        }

        isTransitioning = false;
        Time.timeScale = 0f;
    }

    private IEnumerator ResumeSequence()
    {
        isTransitioning = true;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        float startButtonsAlpha = buttonsGroup != null ? buttonsGroup.alpha : 0f;
        float startOverlayAlpha = overlayCanvas != null ? overlayCanvas.alpha : 0f;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeOutDuration;

            if (buttonsGroup != null)
            {
                SetGroupState(buttonsGroup, Mathf.Lerp(startButtonsAlpha, 0f, t), false);
            }

            if (overlayCanvas != null)
            {
                SetGroupState(overlayCanvas, Mathf.Lerp(startOverlayAlpha, 0f, t), false);
            }

            yield return null;
        }

        SetGroupState(buttonsGroup, 0f, false);
        SetGroupState(overlayCanvas, 0f, false);

        if (AudioManager.Instance != null)
    {
        AudioManager.Instance.UnmuffleBGM(0.3f);
    }

        Time.timeScale = 1f;

        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StartTimer();
        }

        if (PlayerController.Instance != null)
        {
            PlayerState playerState = PlayerController.Instance.GetComponent<PlayerState>();
            if (playerState != null)
            {
                playerState.hit = false;
            }
        }

        isTransitioning = false;
    }

    private IEnumerator ReturnToMenuSequence()
    {
        float elapsed = 0f;
        if (exitFader != null)
        {
            exitFader.blocksRaycasts = true;
            exitFader.interactable = true;

            while (elapsed < exitDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                exitFader.alpha = Mathf.Lerp(0f, 1f, elapsed / exitDuration);
                yield return null;
            }
            exitFader.alpha = 1f;
        }

        Time.timeScale = 1f;
        SetGroupState(buttonsGroup, 0f, false);
        SetGroupState(overlayCanvas, 0f, false);

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

    private void SetGroupState(CanvasGroup group, float alpha, bool interactive)
    {
        if (group == null) return;

        group.alpha = alpha;
        group.blocksRaycasts = interactive;
        group.interactable = interactive;
    }
}