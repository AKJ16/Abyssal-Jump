using TMPro; 
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Attributes

    [SerializeField] private string firstSceneName = "Stage0";
    [SerializeField] private TextMeshProUGUI bestTime;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private GameObject firstSelect;
    private GameObject lastSelected;

    // Methods

    private void Awake()
    {
        Time.timeScale = 1f;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UnmuffleBGM(0f);
        }

#if UNITY_WEBGL
        if (quitButton != null)
        {
            quitButton.SetActive(false);
        }
#endif
    }
    private void Start()
    {
        LoadMenuHighscore();

        if (firstSelect != null)
        {
            MenuButton button = firstSelect.GetComponent<MenuButton>();
            if (button != null)
            {
                button.setFirstSelect();
            }
        }

        if (EventSystem.current != null && firstSelect != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelect);
            lastSelected = firstSelect;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeInAmbience(0.5f);
            AudioManager.Instance.FadeInBGM(0.5f);
        }
    }

    private void Update()
    {
        if (EventSystem.current != null)
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

    private void LoadMenuHighscore()
    {
        if (bestTime != null)
        {
            float bestTime = SaveSystem.LoadBestTime() / 1000f; 

            if (bestTime == 0 || bestTime == float.MaxValue)
            {
                this.bestTime.text = "BEST TIME [--:--:--:---]"; 
            }
            else
            {
                this.bestTime.text = "BEST TIME [" + TimerManager.Instance.GetFormattedTime(bestTime) + "]"; 
            }
        }
    }

    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame(firstSceneName);
        }
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
