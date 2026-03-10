using UnityEngine;

public class IslandTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // ทำงานแค่ครั้งเดียวตอนขับเรือมาถึงเกาะเป้าหมาย
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            Debug.Log("ถึงเกาะเป้าหมายแล้ว! หาจุดตกปลาได้เลย");

            // TODO: ในอนาคตคุณสามารถสั่งโชว์ UI บนหน้าจอตรงนี้ได้
            // เช่น UIManager.Instance.ShowMessage("ถึงเกาะเป้าหมายแล้ว! เริ่มตกปลาได้เลย");
        }
    }
}