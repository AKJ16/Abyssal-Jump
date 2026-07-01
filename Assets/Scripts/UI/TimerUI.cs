using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    // Attributes

    private TextMeshProUGUI timerText;

    // Methods

    private void Awake()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (TimerManager.Instance != null)
        {
            timerText.text = TimerManager.Instance.ShowFormattedTime();
        }
    }
}
