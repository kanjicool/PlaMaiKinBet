using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Casting Bar UI")]
    public GameObject castBarContainer;
    public Image castBarFill;

    public Gradient castBarGradient;

    [Header("Wave UI")]
    public TextMeshProUGUI waveText;

    [Header("Death Screen UI")]
    public GameObject deathPanel;
    public TextMeshProUGUI titleText;           
    public TextMeshProUGUI waveSurvivedText;    
    public TextMeshProUGUI livesRemainingText;  

    public GameObject respawnButton;            
    public GameObject restartButton;            

    [Header("Game Over Audio")]
    public AudioSource uiAudioSource;           
    public AudioClip sadGameOverMusic;          

    public AudioClip[] bossTauntClips;

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

    // =============================
    //       Game Over System
    // =============================
    public void ShowDeathScreen(int waveReached, int livesRemaining)
    {
        if (ScreenEffectManager.Instance != null)
        {
            ScreenEffectManager.Instance.ForceStopBossPressure();
        }

        if (deathPanel != null) deathPanel.SetActive(true);

        // คำนวณ Wave ที่รอด
        int survived = Mathf.Max(0, waveReached - 1);
        if (waveSurvivedText != null) waveSurvivedText.text = $"SURVIVED: {survived} WAVES";

        // แสดงจำนวนชีวิต
        if (livesRemainingText != null)
        {
            if (livesRemaining > 0)
                livesRemainingText.text = $"LIVES REMAINING: {livesRemaining}";
            else
                livesRemainingText.text = "<color=red>NO LIVES REMAINING</color>";
        }

        // เปลี่ยนหัวข้อตามสถานะ
        if (titleText != null)
        {
            titleText.text = livesRemaining > 0 ? "YOU DIED!" : "<color=red>GAME OVER</color>";
        }

        // เปิด/ปิด ปุ่มให้ถูกต้อง
        if (respawnButton != null) respawnButton.SetActive(livesRemaining > 0);
        if (restartButton != null) restartButton.SetActive(livesRemaining <= 0);

        // ปลดเมาส์และหยุดเวลา
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;


        if (bossTauntClips != null && bossTauntClips.Length > 0)
        {
            // สุ่มตัวเลขตั้งแต่ 0 ถึงจำนวนเสียงที่มี
            int randomIndex = Random.Range(0, bossTauntClips.Length);
            AudioClip tauntClip = bossTauntClips[randomIndex];

            if (tauntClip != null && uiAudioSource != null)
            {
                // ใช้ PlayOneShot เพื่อให้เสียงพูดเล่นซ้อนกับเพลงเศร้าได้
                uiAudioSource.PlayOneShot(tauntClip);
            }
        }

        // เล่นเพลงเศร้า ถ้า Game Over (ชีวิตหมด)
        if (livesRemaining <= 0 && uiAudioSource != null && sadGameOverMusic != null)
        {
            uiAudioSource.PlayOneShot(sadGameOverMusic);
        }
    }

    public void RespawnPlayer()
    {
        Time.timeScale = 1f; // คืนเวลาให้เกมเดินต่อ
        if (deathPanel != null) deathPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // สั่งให้ PlayerCombat ทำการวาร์ปตัวละครเกิดใหม่
        PlayerCombat player = FindFirstObjectByType<PlayerCombat>();
        if (player != null) player.ExecuteRespawn();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
