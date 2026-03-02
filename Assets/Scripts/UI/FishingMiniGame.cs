using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FishingMiniGame : MonoBehaviour
{
    // ทำ Singleton เพื่อให้ระบบอื่นเรียกใช้ได้ง่ายๆ
    public static FishingMiniGame Instance { get; private set; }

    [Header("UI References")]
    public GameObject miniGamePanel;
    public Slider tensionSlider;       // หลอดตึง/หย่อน (0-100)
    public Image catchProgressBar;     // หลอดความสำเร็จ (0-100%)

    [Header("Game Settings")]
    public float playerPullForce = 40f; // แรงดึงเวลาเรากดคลิกค้าง
    public float tensionDropRate = 25f; // ความเร็วที่สายจะหย่อนลงเวลาปล่อยเมาส์

    [Header("Sweet Spot (โซนสีเขียว)")]
    public float sweetSpotMin = 30f;
    public float sweetSpotMax = 70f;
    public float catchSpeed = 20f;      // ความเร็วหลอดจับปลาเพิ่มขึ้น
    public float loseSpeed = 10f;       // ความเร็วหลอดจับปลาลดลง (ถ้าหลุดโซน)

    private float currentTension = 50f;
    private float catchProgress = 0f;
    private bool isPlaying = false;
    private FishController hookedFish;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (miniGamePanel != null) miniGamePanel.SetActive(false);
    }

    // เริ่มมินิเกม (เรียกโดยทุ่นตอนปลากินเบ็ด)
    public void StartMiniGame(FishController fish)
    {
        hookedFish = fish;
        currentTension = 50f; // เริ่มที่ตรงกลาง
        catchProgress = 0f;
        isPlaying = true;

        if (miniGamePanel != null) miniGamePanel.SetActive(true);
    }

    private void Update()
    {
        if (!isPlaying) return;

        HandleTension();
        CheckSweetSpot();
        UpdateUI();
    }

    private void HandleTension()
    {
        // 1. จำลองแรงของผู้เล่น (ผมใส่เป็นคลิกขวา Mouse1 ไว้ก่อนนะครับ คุณเปลี่ยนเป็น Action ได้)
        if (Mouse.current.rightButton.isPressed)
        {
            currentTension += playerPullForce * Time.deltaTime;
        }
        else
        {
            currentTension -= tensionDropRate * Time.deltaTime;
        }

        // 2. จำลองแรงปลาดึงสู้ (ดึงลง)
        // ดึง escapePower จาก FishData มาใช้ ยิ่งปลาโหด ยิ่งดึงแรง
        float fishForce = hookedFish.myData.escapePower * 2f;
        currentTension -= fishForce * Time.deltaTime;

        // ล็อคไม่ให้ค่าเกิน 0-100
        currentTension = Mathf.Clamp(currentTension, 0f, 100f);

        // 3. ตรวจสอบเงื่อนไขแพ้
        if (currentTension >= 100f) EndGame(false, "ดึงแรงเกินไป สายเบ็ดขาด!");
        if (currentTension <= 0f) EndGame(false, "หย่อนเกินไป ปลาหลุดหนีไปได้!");
    }

    private void CheckSweetSpot()
    {
        // ถ้าอยู่ในโซนสีเขียว
        if (currentTension >= sweetSpotMin && currentTension <= sweetSpotMax)
        {
            catchProgress += catchSpeed * Time.deltaTime;

            // ถ้าหลอดจับปลาเต็ม 100% = ชนะ!
            if (catchProgress >= 100f)
            {
                EndGame(true, $"ตกปลาสำเร็จ! ได้ {hookedFish.myData.fishName} มาแล้ว!");
            }
        }
        else
        {
            // ถ้าหลุดโซนสีเขียว หลอดจับปลาจะค่อยๆ ลดลง
            catchProgress -= loseSpeed * Time.deltaTime;
            catchProgress = Mathf.Max(catchProgress, 0f);
        }
    }

    private void UpdateUI()
    {
        if (tensionSlider != null) tensionSlider.value = currentTension;
        if (catchProgressBar != null) catchProgressBar.fillAmount = catchProgress / 100f;
    }

    private void EndGame(bool isWin, string message)
    {
        isPlaying = false;
        if (miniGamePanel != null) miniGamePanel.SetActive(false);
        Debug.Log(message);

        if (isWin)
        {
            // ถ้าชนะ ดึงปลาเข้าหาตัวละคร
            // (เดี๋ยวเราค่อยไปอัปเดต FishController เพื่อทำให้มันลอยเข้ากระเป๋า)
        }
        else
        {
            // ถ้าแพ้ ปลาว่ายหนี
        }
    }
}