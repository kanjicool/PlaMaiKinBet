using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Wave Settings (ต้องตรงกับ Shader)")]
    public float waveHeight = 0.5f;  // ความสูงคลื่น
    public float waveSpeed = 1.0f;   // ความเร็ว
    public float waveLength = 1.0f;  // ความกว้างคลื่น (Frequency)
    public float globalWaterLevel = 0.0f; // ระดับน้ำปกติ (Y)

    private void Awake()
    {
        // ทำเป็น Singleton ให้เรียกใช้ง่ายๆ จากทุกที่
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ฟังก์ชันพระเอก: ใครอยากรู้น้ำสูงเท่าไหร่ ให้ส่งตำแหน่งตัวเองมาถาม
    public float GetWaterHeightAtPosition(Vector3 position)
    {
        // สูตร Sine Wave พื้นฐาน (ปรับให้ตรงกับ Shader ของคุณได้)
        // Offset += Sin(x * length + time * speed)
        float offset = Mathf.Sin(position.x * waveLength + Time.time * waveSpeed);
        offset += Mathf.Sin(position.z * waveLength + Time.time * waveSpeed * 0.5f); // เพิ่มมิติ Z เพื่อความพริ้ว

        return globalWaterLevel + (offset * waveHeight);
    }
}