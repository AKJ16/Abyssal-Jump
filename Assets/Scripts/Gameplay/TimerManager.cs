using UnityEngine;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{
    // Attributes

    public static TimerManager Instance {  get; private set; }

    private float elapsedTime;
    private bool isRunning = false;

    // Methods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GameManager.Instance != null && scene.name != GameManager.Instance.GetMainMenuScene())
        {
            StartTimer();
        }
    }

    private void Update()
    {
        if (isRunning && GameManager.Instance != null && !GameManager.Instance.GetLoadingScene())
        {
            elapsedTime += Time.deltaTime;
        }
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
    }

    public string ShowFormattedTime()
    {

        int hours = (int)(elapsedTime / 3600f);
        int minutes = (int)((elapsedTime % 3600f) / 60f);
        int seconds = (int)(elapsedTime % 60f);
        int milliseconds = (int)((elapsedTime * 1000f) % 1000f);

        return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", hours, minutes, seconds, milliseconds);
    }

    public string GetFormattedTime(float rawTime)
    {
        if (rawTime == float.MaxValue) return "--:--:--:---";

        int hours = (int)(rawTime / 3600f);
        int minutes = (int)((rawTime % 3600f) / 60f);
        int seconds = (int)(rawTime % 60f);
        int milliseconds = (int)((rawTime * 1000f) % 1000f);

        return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", hours, minutes, seconds, milliseconds);
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}
