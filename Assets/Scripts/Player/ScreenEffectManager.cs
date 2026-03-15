using UnityEngine;
using UnityEngine.Rendering;
using System.Collections; // ต้องเพิ่มอันนี้เพื่อใช้ Coroutine

public class ScreenEffectManager : MonoBehaviour
{
    public static ScreenEffectManager Instance { get; private set; }

    [Header("Underwater Effect")]
    public Volume underwaterVolume;
    public float filterBlendSpeed = 10f;
    public float cameraFilterFullDepth = 0.2f;

    [Header("Low Health Effect")]
    public Volume lowHealthVolume;

    [Header("Boss Pressure Effect")]
    public Volume bossDangerVolume;
    public float dangerBlendSpeed = 2f;
    private float targetDangerWeight = 0f;

    [Header("Boss Audio")]
    public AudioSource bgmAudioSource;
    public AudioSource alertAudioSource;
    public AudioClip heartbeatClip;
    public AudioClip alertClip;
    public AudioClip huntClip;

    [Header("Audio Fade Settings")]
    public float audioFadeDuration = 1.5f; // ระยะเวลาในการเฟดเสียง (วินาที)
    public float maxBgmVolume = 1f;        // ความดังสูงสุดของเสียง BGM

    private bool isInWater;
    private float waterSurfaceY;

    // ตัวแปรเก็บ Coroutine เพื่อป้องกันการเฟดชนกัน
    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (underwaterVolume != null) underwaterVolume.weight = 0f;
        if (lowHealthVolume != null) lowHealthVolume.weight = 0f;
        if (bossDangerVolume != null) bossDangerVolume.weight = 0f;

        if (bgmAudioSource != null) bgmAudioSource.volume = 0f;
    }

    void Update()
    {
        HandleUnderwaterEffect();
        HandleBossDangerEffect();
    }

    public void SetWaterState(bool inWater, float surfaceY = 0f)
    {
        isInWater = inWater;
        waterSurfaceY = surfaceY;
    }

    private void HandleUnderwaterEffect()
    {
        if (underwaterVolume == null) return;

        float targetWeight = 0f;

        if (isInWater && Camera.main != null)
        {
            float cameraY = Camera.main.transform.position.y;
            float cameraDepth = waterSurfaceY - cameraY;

            if (cameraDepth > 0)
            {
                targetWeight = Mathf.Clamp01(cameraDepth / cameraFilterFullDepth);
            }
        }

        underwaterVolume.weight = Mathf.Lerp(underwaterVolume.weight, targetWeight, Time.deltaTime * filterBlendSpeed);

        if (targetWeight == 0f && underwaterVolume.weight < 0.001f)
        {
            underwaterVolume.weight = 0f;
            if (underwaterVolume.enabled) underwaterVolume.enabled = false;
        }
        else
        {
            if (!underwaterVolume.enabled) underwaterVolume.enabled = true;
        }
    }

    public void SetLowHealthEffect(float intensity)
    {
        if (lowHealthVolume != null)
        {
            lowHealthVolume.weight = intensity;
            lowHealthVolume.enabled = (intensity > 0.001f);
        }
    }

    #region Boss Pressure System

    private void HandleBossDangerEffect()
    {
        if (bossDangerVolume == null) return;

        bossDangerVolume.weight = Mathf.Lerp(bossDangerVolume.weight, targetDangerWeight, Time.deltaTime * dangerBlendSpeed);
        bossDangerVolume.enabled = (bossDangerVolume.weight > 0.001f);
    }

    public void TriggerBossAngry()
    {
        targetDangerWeight = 0.5f;

        if (bgmAudioSource != null && heartbeatClip != null)
        {
            if (bgmAudioSource.clip != heartbeatClip || !bgmAudioSource.isPlaying)
            {
                CrossfadeBGM(heartbeatClip);
            }
        }
    }

    public void TriggerBossRampage()
    {
        targetDangerWeight = 1f;

        if (alertAudioSource != null && alertClip != null)
        {
            alertAudioSource.PlayOneShot(alertClip);
        }

        if (bgmAudioSource != null && huntClip != null)
        {
            if (bgmAudioSource.clip != huntClip || !bgmAudioSource.isPlaying)
            {
                CrossfadeBGM(huntClip);
            }
        }
    }

    public void ClearBossPressure()
    {
        targetDangerWeight = 0f;

        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            // สั่งเฟดเสียงให้เงียบสนิท แล้วค่อย Stop
            if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = StartCoroutine(FadeOutAndStop(bgmAudioSource, audioFadeDuration));
        }
    }

    #endregion

    #region Audio Fading System

    private void CrossfadeBGM(AudioClip newClip)
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        currentFadeCoroutine = StartCoroutine(CrossfadeCoroutine(bgmAudioSource, newClip, audioFadeDuration));
    }

    private IEnumerator CrossfadeCoroutine(AudioSource audioSource, AudioClip newClip, float fadeTime)
    {
        // 1. ถ้ามีเพลงเล่นอยู่ ให้ค่อยๆ ลดเสียงเดิมลงจนเหลือ 0
        if (audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;
            while (audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * Time.deltaTime / (fadeTime / 2f);
                yield return null;
            }
            audioSource.Stop();
        }

        // 2. เปลี่ยนไฟล์เสียงเป็นเพลงใหม่
        audioSource.clip = newClip;
        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.Play();

        // 3. ค่อยๆ เร่งเสียงใหม่ขึ้นไปจนถึงค่าสูงสุดที่ตั้งไว้
        while (audioSource.volume < maxBgmVolume)
        {
            audioSource.volume += maxBgmVolume * Time.deltaTime / (fadeTime / 2f);
            yield return null;
        }
        audioSource.volume = maxBgmVolume; // บังคับให้เป็นค่าเป๊ะๆ ตอนจบ
    }

    private IEnumerator FadeOutAndStop(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume; // รีเซ็ตเสียงกลับมารอไว้สำหรับการเล่นครั้งต่อไป
    }

    #endregion
}