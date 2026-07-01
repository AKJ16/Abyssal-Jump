using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuButton : MonoBehaviour, 
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler,
    ISelectHandler,
    IDeselectHandler,
    ISubmitHandler
{
    // Attributes

    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private Color hoverColor = Color.white;
    [SerializeField] private float transitionSpeed = 12f;
    [SerializeField] private UnityEngine.Events.UnityEvent onClick;

    [SerializeField] private AudioClip selectSFX;
    [SerializeField] private AudioClip clickSFX;

    private TextMeshProUGUI text;
    private Color originalColor;
    private Vector3 originalScale;

    private Color targetColor;
    private Vector3 targetScale;
    private bool isSelected;
    private bool firstSelect;

    // Methods

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        originalColor = text.color;
        originalScale = transform.localScale;

        targetColor = originalColor;
        targetScale = originalScale;
    }

    private void Update()
    {
        text.color = Color.Lerp(text.color, targetColor, transitionSpeed * Time.unscaledDeltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, transitionSpeed * Time.unscaledDeltaTime);

        if (isSelected && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ExecuteClick();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetColor = originalColor;
        targetScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ExecuteClick();
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        targetColor = hoverColor;
        targetScale = originalScale * hoverScale;

        if (firstSelect)
        {
            firstSelect = false;
        }
        else
        {
            if (AudioManager.Instance != null && selectSFX != null)
            {
                AudioManager.Instance.PlaySFX(selectSFX);
            }
        }

        
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        targetColor = originalColor;
        targetScale = originalScale;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        ExecuteClick();
    }

    private void ExecuteClick()
    {
        if (AudioManager.Instance != null && clickSFX != null)
        {
            AudioManager.Instance.PlaySFX(clickSFX);
        }

        onClick?.Invoke();
    }

    public void setFirstSelect()
    {
        firstSelect = true;
    }
}
