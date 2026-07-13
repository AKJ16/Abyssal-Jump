using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class GameManager : MonoBehaviour
{
    // Attributes

    public static GameManager Instance { get; private set; }

    [SerializeField] private Vector3 defaultSpawnPoint;
    private Vector3 activeSpawnPoint;
    private string activeSpawnScene;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [SerializeField] private float sceneTransitionSpeed;
    [SerializeField] private Vector2 jumpEntranceSpeed;
    private bool isSceneTransition = false;
    private bool isLoadingScene = false;
    private bool keepFaderBlack;

    [SerializeField] private float fadeDuration = 0.4f;
    private CanvasGroup screenFader;

    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.2f;

    private CheckPoint activeCheckPoint;

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

        activeSpawnPoint = defaultSpawnPoint;
    }

    private void Start()
    {
        activeSpawnScene = SceneManager.GetActiveScene().name;
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
        FindFaderInScene();

        if (scene.name != mainMenuSceneName)
        {
            InitializeGameplayScene();
        }
        else
        {
            StartCoroutine(MainMenuFade());
        }
    }

    private void FindFaderInScene()
    {
        GameObject faderObj = GameObject.Find("Screen Transition");
        if (faderObj != null)
        {
            screenFader = faderObj.GetComponent<CanvasGroup>();
            if (screenFader == null)
            {
                screenFader = faderObj.AddComponent<CanvasGroup>();
            }

            SetFaderAlpha(isSceneTransition ? 1f : 0f);
        }
    }

    private void SetFaderAlpha(float alpha)
    {
        if (screenFader == null)
        {
            return;
        }

        screenFader.alpha = alpha;

        bool shouldBlock = alpha > 0.01f;
        screenFader.blocksRaycasts = shouldBlock;
        screenFader.interactable = shouldBlock;
    }

    private IEnumerator MainMenuFade()
    {
        if (screenFader != null)
        {
            SetFaderAlpha(1f);
            yield return new WaitForSeconds(0.2f);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                screenFader.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            SetFaderAlpha(0f);
        }
    }

    private void InitializeGameplayScene()
    {
        if (PlayerController.Instance == null && playerPrefab != null)
        {
            SceneEntrance startEntrance = FindDefaultEntrance();
            Vector3 spawnPos = defaultSpawnPoint;
            Collider2D startChunk = null;

            if (startEntrance != null)
            {
                spawnPos = startEntrance.transform.position;
                startChunk = startEntrance.GetStartingChunk();
            }

            GameObject playerObj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

            activeSpawnPoint = spawnPos;
            activeSpawnScene = SceneManager.GetActiveScene().name;

            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.InstantSnap(startChunk);
            }
        }
    }

    private SceneEntrance FindDefaultEntrance()
    {
        SceneEntrance[] entrances = FindObjectsByType<SceneEntrance>(FindObjectsSortMode.None);

        if (entrances.Length > 0)
        {
            foreach (var ent in entrances)
            {
                if (ent.GetEntranceID() == "Start")
                {
                    return ent;
                }
            }
            return entrances[0];
        }
        return null;
    }

    public void UpdateSpawnPoint(Vector3 newPoint, CheckPoint newCheckPoint)
    {
        if (activeCheckPoint != null && activeCheckPoint != newCheckPoint)
        {
            activeCheckPoint.Deactivate();
        }

        activeSpawnPoint = newPoint;
        activeSpawnScene = SceneManager.GetActiveScene().name;
        activeCheckPoint = newCheckPoint;
    }

    public void Respawn()
    {
        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        PlayerState state = player.GetComponent<PlayerState>();
        if (state != null && state.hit) return;

        StartCoroutine(RespawnSequence(player, state));
    }

    private IEnumerator RespawnSequence(PlayerController player, PlayerState state)
    {
        isSceneTransition = true;

        if (state != null)
        {
            state.hit = true;
        }
        player.rb.linearVelocity = Vector2.zero;
        player.rb.simulated = false;

        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.Shake(shakeDuration, shakeMagnitude);
        }

        yield return new WaitForSeconds(0.4f);

        if (screenFader != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                screenFader.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            screenFader.alpha = 1f;
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        if (SceneManager.GetActiveScene().name != activeSpawnScene)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(activeSpawnScene);
            while (!loadOperation.isDone)
            {
                yield return null;
            }
        }

        player.transform.position = activeSpawnPoint;

        camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.InstantSnap();
        }

        yield return new WaitForSeconds(0.2f);

        player.StartFlash();

        player.rb.simulated = true;
        if (state != null)
        {
            state.hit = false;
        }

        player.TriggerRespawnSFX();

        if (screenFader != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                screenFader.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            screenFader.alpha = 0f;
        }

        isSceneTransition = false;
    }

    public void StartGame(string sceneName)
    {
        StartCoroutine(StartGameSequence(sceneName));
    }

    private IEnumerator StartGameSequence(string sceneName)
    {
        keepFaderBlack = true;
        isSceneTransition = true;
        PlayerPrefs.DeleteAll();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllAudio(fadeDuration);
        }

        if (screenFader != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                screenFader.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            screenFader.alpha = 1f;
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        yield return new WaitForSeconds(0.2f);

        isLoadingScene = true;
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!loadOperation.isDone)
        {
            yield return null;
        }
        isLoadingScene = false;

        if (screenFader != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                screenFader.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            screenFader.alpha = 0f;
        }

        keepFaderBlack = false;
        isSceneTransition = false;
    }


    public void TransitionToScene(string sceneName, string entranceID, Vector2 exitDir, float exitSpeed, float duration)
    {
        StartCoroutine(TransitionSequence(sceneName, entranceID, exitDir, exitSpeed, duration));
    }

    private IEnumerator TransitionSequence(string sceneName, string entranceID, Vector2 exitDir, float exitSpeed, float duration)
    {
        isSceneTransition = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutBGM(duration);
            AudioManager.Instance.FadeOutAmbience(duration);
        }

        PlayerController player = PlayerController.Instance;
        if (player == null)
        {
            yield break;
        }

        PlayerState state = player.GetComponent<PlayerState>();

        if (state != null)
        {
            if (state.hit)
            {
                yield break;
            }
            else
            {
                state.hit = true;
            }
        }

        float originalGravity = player.rb.gravityScale;
        player.rb.gravityScale = 0f;
        player.rb.linearVelocity = exitDir * exitSpeed;

        yield return new WaitForSeconds(duration);

        if (screenFader != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                screenFader.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            screenFader.alpha = 1f;
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        player.rb.linearVelocity = Vector2.zero;

        
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        SceneEntrance entrance = FindEntrance(entranceID);
        Collider2D startingChunk = null;

        if (entrance != null)
        {
            player.transform.position = entrance.transform.position;
            startingChunk = entrance.GetStartingChunk();
        }

        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.InstantSnap(startingChunk);
        }

        yield return new WaitForSeconds(0.2f);

        if (screenFader != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                screenFader.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            screenFader.alpha = 0f;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeInBGM(fadeDuration);
            AudioManager.Instance.FadeInAmbience(fadeDuration);
        }

        player.rb.simulated = true;
        player.rb.gravityScale = originalGravity;

        if (exitDir == Vector2.up)
        {
            float totalTime = sceneTransitionSpeed;
            float elapsed = 0f;

            float horizontalDirectionSign = state != null && state.isLookingRight ? 1f : -1f;
            float targetX = horizontalDirectionSign * jumpEntranceSpeed.x;

            player.rb.linearVelocity = new Vector2(0f, jumpEntranceSpeed.y);

            while (elapsed < totalTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / totalTime);

                float delayedT = Mathf.Clamp01((t - 0.25f) / 0.75f);
                float horizontalBlend = Mathf.SmoothStep(0f, 1f, delayedT);
                float currentX = Mathf.Lerp(0f, targetX, horizontalBlend);

                player.rb.linearVelocity = new Vector2(currentX, player.rb.linearVelocity.y);

                yield return null;
            }
        }
        else if (exitDir == Vector2.left || exitDir == Vector2.right)
        {
            float elapsed = 0f;
            player.rb.gravityScale = 0f; 

            while (elapsed < sceneTransitionSpeed)
            {
                elapsed += Time.deltaTime;
                player.rb.linearVelocity = exitDir * 4.5f;
                yield return null;
            }
            player.rb.gravityScale = originalGravity;

            yield return new WaitForSeconds(0.1f);
        }

        if (state != null)
        {
            state.hit = false;
        }

        isSceneTransition = false;
    }

    private SceneEntrance FindEntrance(string id)
    {
        SceneEntrance[] entrances = FindObjectsByType<SceneEntrance>(FindObjectsSortMode.None);
        foreach (var ent in entrances)
        {
            if (ent.GetEntranceID() == id)
            {
                return ent;
            }
        }
        return null;
    }

    public string GetMainMenuScene()
    {
        return mainMenuSceneName;
    }

    public bool GetLoadingScene()
    {
        return isLoadingScene;
    }

    public CanvasGroup GetScreenFader()
    {
        return screenFader;
    }
}
