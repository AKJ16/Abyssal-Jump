using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerSnapshot unmuffledSnapshot;
    [SerializeField] private AudioMixerSnapshot muffledSnapshot;

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource endingMusicSource;

    private float maxBGMVolume;
    private float maxAmbienceVolume;
    private Coroutine bgmFadeCoroutine;
    private Coroutine ambienceFadeCoroutine;
    private Coroutine endingFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 

        maxBGMVolume = bgmSource.volume;
        maxAmbienceVolume = ambienceSource.volume;

        if (bgmSource != null) bgmSource.volume = 0f;
        if (ambienceSource != null) ambienceSource.volume = 0f;
    }

    public void ApplyLocalAnchorSettings(AudioSource localBGMTemplate, AudioSource localAmbienceTemplate)
    {
        if (bgmSource != null && localBGMTemplate != null)
        {
            bgmSource.transform.position = localBGMTemplate.transform.position;
            bgmSource.minDistance = localBGMTemplate.minDistance;
            bgmSource.maxDistance = localBGMTemplate.maxDistance;
            bgmSource.rolloffMode = localBGMTemplate.rolloffMode;
        }

        if (ambienceSource != null && localAmbienceTemplate != null)
        {
            ambienceSource.transform.position = localAmbienceTemplate.transform.position;
            ambienceSource.minDistance = localAmbienceTemplate.minDistance;
            ambienceSource.maxDistance = localAmbienceTemplate.maxDistance;
            ambienceSource.rolloffMode = localAmbienceTemplate.rolloffMode;
        }
    }

    public void MuffleBGM(float duration)
    {
        if (muffledSnapshot != null)
        {
            muffledSnapshot.TransitionTo(duration);
        }
    }

    public void UnmuffleBGM(float duration)
    {
        if (unmuffledSnapshot != null)
        {
            unmuffledSnapshot.TransitionTo(duration);
        }
    }

    public void PlayBGM(AudioClip clip, float fadeInDuration)
    {
        if (clip == null || bgmSource == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            FadeInBGM(fadeInDuration);
            return;
        }

        StartCoroutine(CrossfadeSource(bgmSource, clip, maxBGMVolume, fadeInDuration));
    }

    public void PlayAmbience(AudioClip clip, float fadeInDuration)
    {
        if (clip == null || ambienceSource == null) return;

        if (ambienceSource.clip == clip && ambienceSource.isPlaying)
        {
            FadeInAmbience(fadeInDuration);
            return;
        }

        StartCoroutine(CrossfadeSource(ambienceSource, clip, maxAmbienceVolume, fadeInDuration));
    }

    private IEnumerator CrossfadeSource(AudioSource source, AudioClip newClip, float targetMaxVolume, float duration)
    {
        if (source.isPlaying && source.clip != null)
        {
            float startVol = source.volume;
            float elapsedOut = 0f;
            float fadeOutTime = duration * 0.5f; 

            while (elapsedOut < fadeOutTime)
            {
                elapsedOut += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVol, 0f, elapsedOut / fadeOutTime);
                yield return null;
            }
            source.Stop();
        }

        source.clip = newClip;
        source.volume = 0f;
        source.Play();

        float elapsedIn = 0f;
        float fadeInTime = duration * 0.5f; 

        while (elapsedIn < fadeInTime)
        {
            elapsedIn += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, targetMaxVolume, elapsedIn / fadeInTime);
            yield return null;
        }
        source.volume = targetMaxVolume;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip);
    }

    public void StopSFX( )
    {
        if (sfxSource == null) return;

        sfxSource.Stop();
    }

    public void StopAllAudio(float fadeDuration)
    {
        FadeOutBGM(fadeDuration);
        FadeOutAmbience(fadeDuration);
    }

    public void FadeInBGM(float duration)
    {
        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
        }
        bgmFadeCoroutine = StartCoroutine(FadeSource(bgmSource, maxBGMVolume, duration));
    }

    public void FadeOutBGM(float duration)
    {
        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
        }
        bgmFadeCoroutine = StartCoroutine(FadeSource(bgmSource, 0f, duration, true));
    }

    public void FadeInAmbience(float duration)
    {
        if (ambienceFadeCoroutine != null)
        {
            StopCoroutine(ambienceFadeCoroutine);
        }
        ambienceFadeCoroutine = StartCoroutine(FadeSource(ambienceSource, maxAmbienceVolume, duration));
    }

    public void FadeOutAmbience(float duration)
    {
        if (ambienceFadeCoroutine != null)
        {
            StopCoroutine(ambienceFadeCoroutine);
        }
        ambienceFadeCoroutine = StartCoroutine(FadeSource(ambienceSource, 0f, duration, true));
    }

    private IEnumerator FadeSource(AudioSource source, float targetVolume, float duration, bool stopOnComplete = false)
    {
        if (source == null) yield break;

        if (targetVolume > 0.01f && !source.isPlaying)
        {
            source.Play();
        }

        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;

        if (stopOnComplete && targetVolume < 0.01f)
        {
            source.Stop();
        }
    }

    public void PlayEndingMusic(AudioClip clip)
    {
        if (clip == null || endingMusicSource == null) return;

        endingMusicSource.clip = clip;
        endingMusicSource.volume = 0f;
        endingMusicSource.Play();
    }

    public void FadeInEndingMusic(float duration, float targetVolume)
    {
        if (endingFadeCoroutine != null)
        {
            StopCoroutine(endingFadeCoroutine);
        }
        endingFadeCoroutine = StartCoroutine(FadeSource(endingMusicSource, targetVolume, duration));
    }

    public void FadeOutEndingMusic(float duration)
    {
        if (endingFadeCoroutine != null)
        {
            StopCoroutine(endingFadeCoroutine);
        }
        endingFadeCoroutine = StartCoroutine(FadeSource(endingMusicSource, 0f, duration, true));
    }
}