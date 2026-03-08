using UnityEngine;

public class WaterPhysicsFollower : MonoBehaviour
{
    [Header("References")]
    public Transform player; // ลากเรือหรือตัวละครผู้เล่นมาใส่

    void LateUpdate()
    {
        if (player != null)
        {
            // อัปเดตตำแหน่ง X และ Z ให้ตรงกับผู้เล่น แต่คงความสูง (Y) ของผิวน้ำไว้เท่าเดิม
            transform.position = new Vector3(
                player.position.x,
                transform.position.y,
                player.position.z
            );
        }
    }
}