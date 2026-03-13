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

    public bool ReceiveFishBite(FishController fish)
    {
        if (isBitten) return false;
        
        isBitten = true;

        if (rb != null) rb.AddForce(Vector3.down * 15f, ForceMode.Impulse); 
        if (splashSound != null && audioSource != null) audioSource.PlayOneShot(splashSound);

        OnFishBitten?.Invoke(fish);
        
        return true;
    }

}