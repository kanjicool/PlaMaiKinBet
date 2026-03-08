using UnityEngine;

public class IslandTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        // เช็คว่าคนที่มาชนคือเรือของผู้เล่น (ต้องตั้ง Tag ของเรือเป็น "Player" ด้วยนะครับ)
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            // สั่งให้ GameLoopManager จัดการดึงโลกกลับศูนย์กลาง
            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.OnReachNewIsland(this.gameObject);
            }
        }
    }
}