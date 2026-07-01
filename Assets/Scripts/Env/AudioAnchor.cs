using UnityEngine;

public class AudioAnchor : MonoBehaviour
{
    // Attributes

    [SerializeField] private float bgmFadeInDuration = 0.5f;
    [SerializeField] private float ambienceFadeInDuration = 0.5f;

    [SerializeField] private AudioSource localBgmSource;
    [SerializeField] private AudioSource localAmbienceSource;

    // Methods

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplyLocalAnchorSettings(localBgmSource, localAmbienceSource);

            if (localBgmSource != null && localBgmSource.clip != null)
            {
                AudioManager.Instance.PlayBGM(localBgmSource.clip, bgmFadeInDuration);
            }

            if (localAmbienceSource != null && localAmbienceSource.clip != null)
            {
                AudioManager.Instance.PlayAmbience(localAmbienceSource.clip, ambienceFadeInDuration);
            }
        }
    }
}