using UnityEngine;
using Bitgem.VFX.StylisedWater;

[RequireComponent(typeof(Rigidbody))]
public class BoatBuoyancy : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    public Transform[] floatPoints;
    public float buoyancyForce = 15f;

    // เพิ่มตัวแปรนี้เข้ามาเพื่อให้คุณปรับความสูง-ต่ำของการลอยน้ำได้อิสระ
    [Tooltip("ปรับค่าบวกเพื่อเรือลอยสูงขึ้น ปรับลบเพื่อเรือจมลง")]
    public float surfaceOffset = 0.5f;

    public float waterLinearDamping = 2f;
    public float waterAngularDamping = 2f;

    private Rigidbody rb;
    private float originalLinearDamping;
    private float originalAngularDamping;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalLinearDamping = rb.linearDamping;
        originalAngularDamping = rb.angularDamping;
    }

    void FixedUpdate()
    {
        var waterHelper = WaterVolumeHelper.Instance;
        if (waterHelper == null) return;

        int pointsUnderwater = 0;

        foreach (Transform point in floatPoints)
        {
            float? waterHeight = null;
            try
            {
                waterHeight = waterHelper.GetHeight(point.position);
            }
            catch (System.Exception)
            {
                continue;
            }

            if (waterHeight.HasValue)
            {
                // เอาความสูงน้ำ มาบวกกับ Offset ของเรา
                float adjustedWaterHeight = waterHeight.Value + surfaceOffset;

                // ถ้าจุดนี้อยู่ต่ำกว่าระดับน้ำที่ปรับปรุงแล้ว
                if (point.position.y < adjustedWaterHeight)
                {
                    // ยิ่งจมลึก แรงดันยิ่งมาก
                    float depth = adjustedWaterHeight - point.position.y;

                    rb.AddForceAtPosition(Vector3.up * buoyancyForce * depth, point.position, ForceMode.Force);
                    pointsUnderwater++;
                }
            }
        }

        if (pointsUnderwater > 0)
        {
            rb.linearDamping = waterLinearDamping;
            rb.angularDamping = waterAngularDamping;
        }
        else
        {
            rb.linearDamping = originalLinearDamping;
            rb.angularDamping = originalAngularDamping;
        }
    }
}