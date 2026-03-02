using UnityEngine;
using UnityEngine.Rendering;

public class ScreenEffectManager : MonoBehaviour
{
    [Header("Underwater Effect")]
    public Volume underwaterVolume;
    public float filterBlendSpeed = 10f;
    public float cameraFilterFullDepth = 0.2f;

    [Header("Low Health Effect (For Future)")]
    public Volume lowHealthVolume;

    private bool isInWater;
    private float waterSurfaceY;

    private void Awake()
    {
        if (underwaterVolume != null) underwaterVolume.weight = 0f;
        if (lowHealthVolume != null) lowHealthVolume.weight = 0f;
    }


    void Update()
    {
        HandleUnderwaterEffect();
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

        underwaterVolume.weight = Mathf.Lerp(underwaterVolume.weight, targetWeight, Time.deltaTime * filterBlendSpeed); ;
    }

    public void SetLowHealthEffect(float intensity)
    {
        if (lowHealthVolume != null)
        {
            lowHealthVolume.weight = intensity;
        }
    }

}
