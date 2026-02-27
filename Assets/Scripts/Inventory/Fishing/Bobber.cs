using UnityEngine;
using Bitgem.VFX.StylisedWater;

[RequireComponent(typeof(Rigidbody))]
public class Bobber : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool isInWater = false;
    private float waterBaseY = 0f;

    [Header("Buoyancy Settings")]
    public float floatOffset = 0.05f;     // ระยะชดเชย ให้ทุ่นลอยปริ่มๆ น้ำ
    public float buoyancyForce = 35f;     // แรงดันน้ำ (ยิ่งเยอะยิ่งเด้งสู้รวดเร็ว)
    public float waterDrag = 3f;          // ความหนืดของน้ำ (ป้องกันไม่ให้ทุ่นเด้งดึ๋งไม่หยุด)
    public float waterAngularDrag = 2f;   // ความหนืดในการหมุน

    [Header("VFX & SFX")]
    public GameObject splashParticlePrefab;
    public AudioClip splashSound;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    void FixedUpdate()
    {
        // ถ้าตกน้ำแล้ว ให้คำนวณแรงลอยตัวตลอดเวลา
        if (isInWater)
        {
            // 1. หาความสูงของน้ำ ณ ปัจจุบัน (ถ้ามีคลื่น ค่านี้จะขยับขึ้นลง)
            float targetY = waterBaseY;
            if (WaterVolumeHelper.Instance != null)
            {
                float? waterHeight = WaterVolumeHelper.Instance.GetHeight(transform.position);
                if (waterHeight.HasValue)
                {
                    targetY = waterHeight.Value;
                }
            }

            // 2. คำนวณความลึก (เป้าหมายผิวน้ำ + ระยะชดเชย - ตำแหน่งปัจจุบัน)
            float depth = (targetY + floatOffset) - transform.position.y;

            // 3. ถ้าทุ่นอยู่ต่ำกว่าระดับที่ควรจะเป็น (คือจมน้ำอยู่)
            if (depth > 0)
            {
                // ใช้การดันขึ้น ยิ่งจมลึก(depth มาก) ยิ่งดันแรง 
                // ทำให้เวลาใกล้ผิวน้ำแรงดันจะแผ่วลง ดูนุ่มนวลเป็นธรรมชาติ
                rb.AddForce(Vector3.up * (depth * buoyancyForce), ForceMode.Acceleration);
            }
        }
    }

    private void HitWater(float hitY)
    {
        if (isInWater) return;

        isInWater = true;
        waterBaseY = hitY;

        rb.linearDamping = waterDrag;
        rb.angularDamping = waterAngularDrag;

        if (splashParticlePrefab != null)
        {
            Vector3 splashPos = new Vector3(transform.position.x, hitY, transform.position.z);
            GameObject splash = Instantiate(splashParticlePrefab, splashPos, Quaternion.identity);

            Destroy(splash, 2f);
        }

        if (splashSound != null)
        {
            audioSource.PlayOneShot(splashSound);
        }

        Debug.Log("ทุ่นตกน้ำ! ระบบฟิสิกส์ลอยตัวเริ่มทำงาน");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            HitWater(other.bounds.max.y);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Water")) return;

        if (!isInWater)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            Debug.Log($"ทุ่นตกบนบก! ชนกับ: {collision.gameObject.name}");
        }
    }
}