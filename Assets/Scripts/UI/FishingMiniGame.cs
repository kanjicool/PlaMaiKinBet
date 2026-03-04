using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Xml.Serialization;

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
    private FishController hookedFish;

    private float sweetSpotCenter = 50f;
    private float targetSweetSpotCenter = 50f;
    private float changeTargetTimer = 0f;

    private float SweetSpotMin => sweetSpotCenter - (sweetSpotWidth / 2f);
    private float SweetSpotMax => sweetSpotCenter + (sweetSpotWidth / 2f);


    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (miniGamePanel != null) miniGamePanel.SetActive(false);
    }

    public void StartMiniGame(FishController fish)
    {
        hookedFish = fish;
        currentTension = 50f;
        catchProgress = 0f;

        sweetSpotCenter = 50f;
        targetSweetSpotCenter = 50f;
        
        isPlaying = true;

        if (miniGamePanel != null) miniGamePanel.SetActive(true);
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
        {
            currentTension += playerPullForce * Time.deltaTime;
        }
        else
        {
            currentTension -= tensionDropRate * Time.deltaTime;
        }

        float fishForce = hookedFish != null ? hookedFish.myData.escapePower * 2f : 10f;
        currentTension -= fishForce * Time.deltaTime;

        currentTension = Mathf.Clamp(currentTension, 0f, 100f);

        if (currentTension >= 100f) EndGame(false, "ดึงแรงเกินไป สายเบ็ดขาด!");
        if (currentTension <= 0f) EndGame(false, "หย่อนเกินไป ปลาหลุดหนีไปได้!");
    }

    private void MoveSweetSpot()
    {
        changeTargetTimer -= Time.deltaTime;
        if (changeTargetTimer <= 0f || Mathf.Abs(sweetSpotCenter - targetSweetSpotCenter) < 1f)
        {
            float halfWidth = sweetSpotWidth / 2f;
            targetSweetSpotCenter = Random.Range(halfWidth, 100f - halfWidth);

            changeTargetTimer = Random.Range(0.5f, 2f);
        }

        float moveSpeed = baseMoveSpeed + (hookedFish != null ? hookedFish.myData.escapePower : 0);
        sweetSpotCenter = Mathf.MoveTowards(sweetSpotCenter, targetSweetSpotCenter, moveSpeed * Time.deltaTime);
    }


    private void CheckSweetSpot()
    {
        if (currentTension >= SweetSpotMin && currentTension <= SweetSpotMax)
        {
            catchProgress += catchSpeed * Time.deltaTime;

            if (catchProgress >= 100f)
            {
                string fishName = hookedFish != null ? hookedFish.myData.fishName : "ปลาปริศนา";
                EndGame(true, $"ตกปลาสำเร็จ! ได้ {fishName} มาแล้ว!");
            }
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

        // อัปเดตตำแหน่งและขนาดของโซนสีเขียวบน UI
        if (sweetSpotUI != null)
        {
            // แปลงค่า 0-100 ให้เป็นแกน 0-1 สำหรับ Anchor
            float minAnchor = SweetSpotMin / 100f;
            float maxAnchor = SweetSpotMax / 100f;

            // ปรับ Anchor X (สมมติว่าหลอดเป็นแนวนอน)
            sweetSpotUI.anchorMin = new Vector2(minAnchor, sweetSpotUI.anchorMin.y);
            sweetSpotUI.anchorMax = new Vector2(maxAnchor, sweetSpotUI.anchorMax.y);

            // เคลียร์ค่า Offset เพื่อให้ภาพขยายเต็มพื้นที่ Anchor พอดี
            sweetSpotUI.offsetMin = new Vector2(0, sweetSpotUI.offsetMin.y);
            sweetSpotUI.offsetMax = new Vector2(0, sweetSpotUI.offsetMax.y);
        }
    }

    private void EndGame(bool isWin, string message)
    {
        isPlaying = false;
        if (miniGamePanel != null) miniGamePanel.SetActive(false);
        Debug.Log(message);

        if (isWin)
        {
            // โลจิกเมื่อชนะ
        }
        else
        {
            // โลจิกเมื่อแพ้
        }
    }
}