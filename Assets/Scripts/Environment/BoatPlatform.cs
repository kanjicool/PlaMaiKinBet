using UnityEngine;

public class BoatPlatform : MonoBehaviour
{
    // ตรวจสอบเมื่อมีอะไรมาชน (หรือเหยียบ)
    private void OnCollisionEnter(Collision collision)
    {
        // เช็คว่าเป็น Player ไหม (อย่าลืมตั้ง Tag ที่ตัว Player ว่า "Player")
        if (collision.gameObject.CompareTag("Player"))
        {
            // ให้ Player มาเป็นลูกของเรือ (จะขยับตามเรืออัตโนมัติ)
            collision.transform.SetParent(transform);
        }
    }

    // ตรวจสอบเมื่อ Player เดินออกจากเรือ
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // ปลดความเป็นลูกออก (กลับไปเป็นอิสระ)
            collision.transform.SetParent(null);
        }
    }
}