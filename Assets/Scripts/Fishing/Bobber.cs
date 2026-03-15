using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bobber : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource audioSource;
    public bool isInWater = false;
    public bool isBitten = false;
    private float waterBaseY = 0f;

    [Header("Buoyancy Settings")]
    public float floatOffset = 0.05f;
    public float buoyancyForce = 35f;
    public float waterDrag = 3f;
    public float waterAngularDrag = 2f;

    [Header("VFX & SFX")]
    public GameObject splashParticlePrefab;
    public AudioClip splashSound;

    public event Action<FishController> OnFishBitten;

    // 🌟 ตัวแปรเก็บโมเดลเหยื่อที่ห้อยอยู่
    private GameObject visualBait;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    void FixedUpdate()
    {
        if (isInWater && !isBitten)
        {
            float depth = (waterBaseY + floatOffset) - transform.position.y;
            if (depth > 0)
            {
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
            Destroy(Instantiate(splashParticlePrefab, splashPos, Quaternion.identity), 2f);
        }

        if (splashSound != null) audioSource.PlayOneShot(splashSound);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water")) HitWater(other.bounds.max.y);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Water") && !isInWater)
        {
            rb.isKinematic = true;
        }
    }

    // 🌟 ฟังก์ชันสร้างเหยื่อมาห้อยใต้ทุ่น
    public void SetBaitVisual(GameObject baitPrefab)
    {
        if (baitPrefab != null)
        {
            // สร้างเหยื่อและตั้งให้เป็นลูกของ Bobber ทันที
            visualBait = Instantiate(baitPrefab, transform);

            // 🌟 1. ตั้งค่า Position ตามในภาพ
            visualBait.transform.localPosition = new Vector3(0.771f, 0.122f, -0.086f);

            // 🌟 2. ตั้งค่า Rotation ตามในภาพ (ใช้ Quaternion.Euler เพื่อแปลงองศา)
            visualBait.transform.localRotation = Quaternion.Euler(2.127f, 187.1f, -16.593f);

            // 🌟 3. ตั้งค่า Scale ตามในภาพ
            visualBait.transform.localScale = new Vector3(80f, 80f, 80f);

            // ปิดระบบฟิสิกส์ของเหยื่อ เพื่อไม่ให้ถ่วงน้ำหนักหรือชนกับทุ่น
            if (visualBait.TryGetComponent<Rigidbody>(out Rigidbody rbBait)) Destroy(rbBait);
            if (visualBait.TryGetComponent<Collider>(out Collider colBait)) colBait.enabled = false;
        }
    }

    public bool ReceiveFishBite(FishController fish)
    {
        if (isBitten) return false;

        isBitten = true;

        // 🌟 ปลากินปุ๊บ ทำลายโมเดลเหยื่อที่ห้อยอยู่ทิ้ง
        if (visualBait != null)
        {
            Destroy(visualBait);
        }

        if (rb != null) rb.AddForce(Vector3.down * 15f, ForceMode.Impulse);
        if (splashSound != null && audioSource != null) audioSource.PlayOneShot(splashSound);

        OnFishBitten?.Invoke(fish);

        return true;
    }
}