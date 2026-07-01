using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaCover : MonoBehaviour
{
    // Attributes

    [SerializeField] private GameObject coverParent;
    [SerializeField] private float fadeSpeed = 3f;

    private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    private Coroutine fadeCoroutine;

    private string uniqueID;
    private bool isRevealed;

    // Methods

    private void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (coverParent != null)
        {
            spriteRenderers.AddRange(coverParent.GetComponentsInChildren<SpriteRenderer>());
        }

        string sceneName = SceneManager.GetActiveScene().name;
        uniqueID = $"SA_{sceneName}_{transform.position.x}_{transform.position.y}_{transform.position.z}";

        if (PlayerPrefs.GetInt(uniqueID, 0) == 1)
        {
            isRevealed = true;
            foreach (var sr in spriteRenderers)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 0f;
                    sr.color = c;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isRevealed) return;

        if (other.CompareTag("Player"))
        {
            isRevealed = true;

            PlayerPrefs.SetInt(uniqueID, 1);
            PlayerPrefs.Save();

            TriggerFade(0f);
        }
    }

    private void TriggerFade(float targetAlpha)
    {
        if (spriteRenderers.Count == 0) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeSequence(targetAlpha));
    }

    private IEnumerator FadeSequence(float targetAlpha)
    {
        float currentAlpha = spriteRenderers[0].color.a;

        while (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

            foreach (var sr in spriteRenderers)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = currentAlpha;
                    sr.color = c;
                }
            }

            yield return null;
        }

        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = targetAlpha;
                sr.color = c;
            }
        }
    }
}
