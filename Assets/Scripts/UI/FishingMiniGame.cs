using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FishingMiniGame : MonoBehaviour
{
    public static FishingMiniGame Instance { get; private set; }

    [Header("UI References")]
    public GameObject miniGamePanel;
    public Slider tensionSlider;       // หลอดตึง/หย่อน (0-100)
    public Image catchProgressBar;     // หลอดความสำเร็จ (0-100%)

    [Header("Dynamic Sweet Spot UI")]
    public RectTransform sweetSpotUI;

    [Header("Game Settings")]
    public float playerPullForce = 40f; // แรงดึงเวลาเรากดคลิกค้าง
    public float tensionDropRate = 25f; // ความเร็วที่สายจะหย่อนลงเวลาปล่อยเมาส์

    [Header("Sweet Spot")]
    public float sweetSpotWidth = 30f;
    public float baseMoveSpeed = 15f;
    public float catchSpeed = 20f;      // ความเร็วหลอดจับปลาเพิ่มขึ้น
    public float loseSpeed = 10f;       // ความเร็วหลอดจับปลาลดลง (ถ้าหลุดโซน)

    private float currentTension = 50f;
    private float catchProgress = 0f;
    private bool isPlaying = false;

    private float sweetSpotCenter = 50f;
    private float targetSweetSpotCenter = 50f;
    private float changeTargetTimer = 0f;

    private float SweetSpotMin => sweetSpotCenter - (sweetSpotWidth / 2f);
    private float SweetSpotMax => sweetSpotCenter + (sweetSpotWidth / 2f);

    private float currentEscapePower;
    private Action onWinCallback;
    private Action onLoseCallback;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (miniGamePanel != null) miniGamePanel.SetActive(false);
    }

    public void StartMiniGame(float escapePower, Action onWin, Action onLose)
    {
        currentEscapePower = escapePower;
        onWinCallback = onWin;
        onLoseCallback = onLose;

        currentTension = 50f;
        catchProgress = 0f;
        sweetSpotCenter = 50f;
        targetSweetSpotCenter = 50f;
        
        isPlaying = true;
        if (miniGamePanel != null) miniGamePanel.SetActive(true);

        Debug.Log("3. ระบบมินิเกมเริ่มทำงาน กำลังเปิดหน้าต่าง UI!");
    }

    private void Update()
    {
        if (!isPlaying) return;

        HandleTension();
        MoveSweetSpot();
        CheckSweetSpot();
        UpdateUI();
    }

    private void HandleTension()
    {
        if (Mouse.current.rightButton.isPressed)
            currentTension += playerPullForce * Time.deltaTime;
        else
            currentTension -= tensionDropRate * Time.deltaTime;

        currentTension -= (currentEscapePower * 2f) * Time.deltaTime;
        currentTension = Mathf.Clamp(currentTension, 0f, 100f);

        if (currentTension >= 100f) EndGame(false);
        if (currentTension <= 0f) EndGame(false);
    }

    private void MoveSweetSpot()
    {
        changeTargetTimer -= Time.deltaTime;
        if (changeTargetTimer <= 0f || Mathf.Abs(sweetSpotCenter - targetSweetSpotCenter) < 1f)
        {
            float halfWidth = sweetSpotWidth / 2f;
            targetSweetSpotCenter = UnityEngine.Random.Range(halfWidth, 100f - halfWidth);
            changeTargetTimer = UnityEngine.Random.Range(0.5f, 2f);
        }

        float moveSpeed = baseMoveSpeed + currentEscapePower;
        sweetSpotCenter = Mathf.MoveTowards(sweetSpotCenter, targetSweetSpotCenter, moveSpeed * Time.deltaTime);
    }


    private void CheckSweetSpot()
    {
        if (currentTension >= SweetSpotMin && currentTension <= SweetSpotMax)
        {
            catchProgress += catchSpeed * Time.deltaTime;
            if (catchProgress >= 100f) EndGame(true);
        }
        else
        {
            catchProgress -= loseSpeed * Time.deltaTime;
            catchProgress = Mathf.Max(catchProgress, 0f);
        }
    }

    private void UpdateUI()
    {
        if (tensionSlider != null) tensionSlider.value = currentTension;
        if (catchProgressBar != null) catchProgressBar.fillAmount = catchProgress / 100f;

        if (sweetSpotUI != null)
        {
            sweetSpotUI.anchorMin = new Vector2(SweetSpotMin / 100f, sweetSpotUI.anchorMin.y);
            sweetSpotUI.anchorMax = new Vector2(SweetSpotMax / 100f, sweetSpotUI.anchorMax.y);
            sweetSpotUI.offsetMin = new Vector2(0, sweetSpotUI.offsetMin.y);
            sweetSpotUI.offsetMax = new Vector2(0, sweetSpotUI.offsetMax.y);
        }
    }

    private void EndGame(bool isWin)
    {
        isPlaying = false;
        if (miniGamePanel != null) miniGamePanel.SetActive(false);

        if (isWin) onWinCallback?.Invoke();
        else onLoseCallback?.Invoke();

    }
}