using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bobber : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource audioSource;
    public bool isInWater = false;
    private float waterBaseY = 0f;

    [Header("Buoyancy Settings")]
    public float floatOffset = 0.05f;     // ระยะชดเชย ให้ทุ่นลอยปริ่มๆ น้ำ
    public float buoyancyForce = 35f;     // แรงดันน้ำ
    public float waterDrag = 3f;          // ความหนืดของน้ำ
    public float waterAngularDrag = 2f;   // ความหนืดในการหมุน

    [Header("VFX & SFX")]
    public GameObject splashParticlePrefab;
    public AudioClip splashSound;

    [HideInInspector] public FishingRod myRod;

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
        if (isInWater)
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
        waterBaseY = hitY; // จำระดับความสูงของน้ำไว้ใช้คำนวณการลอยตัว

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
        if (!collision.gameObject.CompareTag("Water") && !isInWater)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            Debug.Log($"ทุ่นตกบนบก! ชนกับ: {collision.gameObject.name}");
        }
    }

    public void OnFishBite(FishController fish)
    {
        Debug.Log("เบ็ดกระตุก! ปลากินเหยื่อแล้ว เตรียมตัวดึง!");

        if (rb != null)
        {
            rb.AddForce(Vector3.down * 15f, ForceMode.Impulse);
        }

        if (splashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(splashSound);
        }

        if (FishingMiniGame.Instance != null)
        {
            FishingMiniGame.Instance.StartMiniGame(fish, myRod);
        }
    }
}