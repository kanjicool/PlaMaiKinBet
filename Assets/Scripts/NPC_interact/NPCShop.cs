using UnityEngine;
using UnityEngine.InputSystem;

public class NPCShop : MonoBehaviour
{
    [Header("UI TalkButton")]
    public GameObject interactPrompt; // ลาก InteractPrompt_Canvas มาใส่ช่องนี้

    private bool isPlayerNear = false;

    private void Start()
    {
        // เริ่มเกมมาให้ซ่อนปุ่ม E ไว้ก่อน
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        // เช็คว่าผู้เล่นอยู่ใกล้ และกดปุ่ม E
        if (isPlayerNear && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // เช็คว่าถ้าหน้าคุยและหน้าร้านยังไม่เปิด ถึงจะเปิดหน้าคุยได้
            if (!ShopManager.instance.shopUI.activeSelf && !ShopManager.instance.dialogueUI.activeSelf)
            {
                Debug.Log("กด E สำเร็จ! เปิดหน้าต่างสนทนากับ NPC");
                ShopManager.instance.OpenDialogue();

                // ซ่อนปุ่ม E บนหัว NPC ออกไปตอนที่กำลังคุยอยู่
                if (interactPrompt != null) interactPrompt.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("ผู้เล่นอยู่ในระยะ - โชว์ปุ่ม E");

            // โชว์ปุ่ม E บนหัวเมื่อเดินเข้าใกล้ (และต้องไม่ได้เปิดหน้าคุยอยู่)
            if (interactPrompt != null && !ShopManager.instance.dialogueUI.activeSelf)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            Debug.Log("ผู้เล่นเดินออกนอกระยะ");

            // ซ่อนปุ่ม E บนหัวเมื่อเดินออก
            if (interactPrompt != null) interactPrompt.SetActive(false);

            // บังคับปิดหน้าต่างคุยและร้านค้าเมื่อผู้เล่นเดินหนี
            ShopManager.instance.CloseShop();
            ShopManager.instance.CloseDialogue();
        }
    }
}