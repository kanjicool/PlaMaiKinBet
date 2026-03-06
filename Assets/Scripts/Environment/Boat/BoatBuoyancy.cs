using UnityEngine;
using Bitgem.VFX.StylisedWater;

[RequireComponent(typeof(Rigidbody))]
public class BoatBuoyancy : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    public Transform[] floatPoints; // สร้าง GameObject เปล่า 4 ตัววางไว้ที่มุมเรือทั้ง 4 ด้านแล้วลากมาใส่
    public float buoyancyForce = 15f; // แรงลอยตัว
    public float waterLinearDamping = 2f; // แรงต้านน้ำ (Unity 6 ใช้ linearDamping แทน drag)
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

            // ใช้ try-catch ดักไว้เผื่อระบบน้ำยัง initialize ไม่เสร็จ
            try
            {
                waterHeight = waterHelper.GetHeight(point.position);
            }
            catch (System.Exception)
            {
                // ถ้าระบบน้ำยังไม่พร้อม ให้ข้ามจุดนี้ไปก่อน
                continue;
            }

            // ถ้าจุดนี้อยู่ต่ำกว่าระดับน้ำ
            if (waterHeight.HasValue && point.position.y < waterHeight.Value)
            {
                // คำนวณความลึก ยิ่งลึกยิ่งแรงผลักมาก
                float depth = waterHeight.Value - point.position.y;

                // ออกแรงดันขึ้นที่จุดนั้นๆ
                rb.AddForceAtPosition(Vector3.up * buoyancyForce * depth, point.position, ForceMode.Force);
                pointsUnderwater++;
            }
        }

        // ถ้ามีส่วนใดส่วนหนึ่งสัมผัสน้ำ ให้เพิ่มแรงต้านน้ำ
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