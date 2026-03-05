using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AdvancedBuoyancy : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    public Transform[] floaters;          // ใส่จุดลอยตัว (Empty Object 4 มุมของเรือ)
    public float floatingPower = 15f;     // แรงดันน้ำ ยิ่งเยอะยิ่งลอยสูง
    public float waterHeight = 0f;        // ระดับความสูงของผิวน้ำ (แกน Y)

    [Header("Water Drag (ความหนืดผิวน้ำ)")]
    public float underWaterDrag = 3f;
    public float underWaterAngularDrag = 2f;
    public float airDrag = 0.05f;
    public float airAngularDrag = 0.05f;

    private Rigidbody rb;
    private bool isUnderwater;
    private int floatersUnderwater;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        floatersUnderwater = 0;

        // วนลูปเช็กจุดลอยตัวแต่ละจุดว่าจมน้ำไหม
        for (int i = 0; i < floaters.Length; i++)
        {
            float difference = floaters[i].position.y - waterHeight;

            if (difference < 0) // ถ้าจุดนี้อยู่ต่ำกว่าผิวน้ำ
            {
                // ดันจุดนี้ขึ้นด้วยแรงแปรผันตามความลึก (ยิ่งลึกยิ่งดันแรง)
                rb.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(difference), floaters[i].position, ForceMode.Force);
                floatersUnderwater++;

                if (!isUnderwater)
                {
                    isUnderwater = true;
                    SwitchState(true);
                }
            }
        }

        // ถ้าไม่มีจุดไหนจมน้ำเลย ให้เปลี่ยนกลับเป็นสถานะลอยกลางอากาศ
        if (isUnderwater && floatersUnderwater == 0)
        {
            isUnderwater = false;
            SwitchState(false);
        }
    }

    private void SwitchState(bool isUnder)
    {
        if (isUnder)
        {
            rb.linearDamping = underWaterDrag;
            rb.angularDamping = underWaterAngularDrag;
        }
        else
        {
            rb.linearDamping = airDrag;
            rb.angularDamping = airAngularDrag;
        }
    }
}