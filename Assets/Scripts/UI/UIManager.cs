using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Casting Bar UI")]
    public GameObject castBarContainer;
    public Image castBarFill;

    public Gradient castBarGradient;

    [Header("Wave UI")]
    public TextMeshProUGUI waveText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        HideCastBar();
    }

    // =============================
    //       Casting Bar System
    // =============================
    public void ShowCastBar()
    {
        if (castBarContainer != null) castBarContainer.SetActive(true);
        UpdateCastBar(0f, 1f);
    }

    public void HideCastBar()
    {
        if (castBarContainer != null) castBarContainer.SetActive(false);
    }

    public void UpdateCastBar(float currentValue, float maxValue)
    {
        if (castBarFill == null) return;

        float fillPercentage = currentValue / maxValue;

        castBarFill.fillAmount = fillPercentage;

        castBarFill.color = castBarGradient.Evaluate(fillPercentage);
    }

    // =============================
    //         Wave System
    // =============================
    public void UpdateWaveText(int currentWave)
    {
        if (waveText != null)
        {
            waveText.text = $"WAVE {currentWave}";
        }
    }
}
