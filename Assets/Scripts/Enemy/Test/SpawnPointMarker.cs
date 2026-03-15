using UnityEngine;

public class SpawnPointMarker : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color markerColor = Color.red; // สีของจุดเกิด
    public float markerSize = 0.5f;       // ขนาดของสัญลักษณ์
    public bool showForwardArrow = true;  // แสดงลูกศรบอกทิศทางไหม

    // OnDrawGizmos จะทำงานตลอดเวลาในหน้า Scene (แม้ไม่ได้คลิกที่วัตถุ)
    private void OnDrawGizmos()
    {
        // 1. ตั้งค่าเมทริกซ์การวาดให้ตรงกับตำแหน่งและการหมุนของวัตถุ
        // วิธีนี้จะทำให้สัญลักษณ์หมุนตามที่เรา Rotate วัตถุใน Unity เป๊ะๆ
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.matrix = rotationMatrix;

        // 2. วาดกล่องสี่เหลี่ยมโปร่งแสง
        Gizmos.color = new Color(markerColor.r, markerColor.g, markerColor.b, 0.3f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one * markerSize);

        // 3. วาดเส้นขอบกล่องให้ชัดเจน
        Gizmos.color = markerColor;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one * markerSize);

        // 4. วาดลูกศรชี้ไปทิศหน้า (Z-Forward)
        if (showForwardArrow)
        {
            Gizmos.color = Color.yellow;
            Vector3 arrowTip = Vector3.forward * (markerSize * 1.5f);

            // วาดเส้นแกนลูกศร
            Gizmos.DrawLine(Vector3.zero, arrowTip);

            // วาดหัวลูกศร (ด้านซ้ายและขวา)
            Gizmos.DrawLine(arrowTip, arrowTip + (Vector3.back + Vector3.left) * (markerSize * 0.3f));
            Gizmos.DrawLine(arrowTip, arrowTip + (Vector3.back + Vector3.right) * (markerSize * 0.3f));
        }
    }
}