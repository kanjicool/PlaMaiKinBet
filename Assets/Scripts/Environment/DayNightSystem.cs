using UnityEngine;

public class DayNightSystem : MonoBehaviour
{
    [Header("Time Settings")]
    [Range(0, 24)] public float currentTime = 8f; // เริ่มต้นที่ 8 โมงเช้า
    public float dayDurationInMinutes = 10f; // 1 วันในเกมใช้เวลากี่นาทีจริง

    [Header("Lights & Environment")]
    public Light sunLight;
    public Gradient sunColor;      // ใช้กำหนดสีท้องฟ้าตามเวลา (ส้ม-ขาว-น้ำเงินเข้ม)
    public AnimationCurve intensityCurve; // ใช้คุมความสว่าง (มืดตอนกลางคืน สว่างตอนกลางวัน)

    private void Update()
    {
        UpdateTime();
        UpdateSunRotation();
    }

    private void UpdateTime()
    {
        // คำนวณความเร็วของเวลา: 24 ชม. / (นาทีจริง * 60 วินาที)
        float timeMultiplier = 24f / (dayDurationInMinutes * 60f);
        currentTime += Time.deltaTime * timeMultiplier;

        if (currentTime >= 24f) currentTime = 0f; // ครบวันแล้วเริ่มใหม่
    }

    private void UpdateSunRotation()
    {
        // คำนวณมุมหมุน: (เวลาปัจจุบัน / 24 ชม.) * 360 องศา - 90 (เพื่อให้เที่ยงตรงหัวพอดี)
        float sunRotation = (currentTime / 24f) * 360f - 90f;
        sunLight.transform.localRotation = Quaternion.Euler(sunRotation, 170f, 0f);

        // ปรับสีและความสว่างตาม Curve และ Gradient
        float timePercent = currentTime / 24f;
        sunLight.color = sunColor.Evaluate(timePercent);
        sunLight.intensity = intensityCurve.Evaluate(timePercent);

        // ปิดไฟพระอาทิตย์ถ้ามันมุดลงดิน (ประหยัด Performance)
        sunLight.enabled = sunLight.intensity > 0;
    }
}