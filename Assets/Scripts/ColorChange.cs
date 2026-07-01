using Unity.VisualScripting;
using UnityEngine;

public class ColorChange : MonoBehaviour
{
    // Attributes

    private SpriteRenderer sr;
    [SerializeField] private Color newColor;

    // Methods

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = newColor * 2f;
    }
}
