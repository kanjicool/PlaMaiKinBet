using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class BoatSummonStation : MonoBehaviour
{
    [Header("Summon Settings")]
    [Tooltip("จุดที่จะให้เรือโผล่มา (ควรเป็น Empty Object เหนือน้ำใกล้ๆ แท่น)")]
    public Transform summonPoint;

    [Header("UI Settings")]
    public GameObject interactUI; // UI แจ้งเตือนเช่น "กด F เพื่อเรียกเรือ"

    private bool isPlayerNear = false;
    private BoatInteract playerBoat;

    private void Start()
    {
        // บังคับให้ Collider เป็น Trigger เสมอ
        GetComponent<Collider>().isTrigger = true;

        if (interactUI != null) interactUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (interactUI != null) interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactUI != null) interactUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isPlayerNear) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SummonBoat();
        }
    }

    private void SummonBoat()
    {
        // 1. ค้นหาเรือในฉาก (ถอดย้ายไปเกาะไหนก็หาเจอ)
        if (playerBoat == null)
        {
            playerBoat = FindFirstObjectByType<BoatInteract>();
        }

        if (playerBoat != null && summonPoint != null)
        {
            // ป้องกันบัค: ถ้าผู้เล่นยังนั่งอยู่ในเรือ ไม่ควรให้เรียกเรือได้
            if (playerBoat.boatController.isPlayerDriving)
            {
                Debug.LogWarning("ไม่สามารถเรียกเรือได้ เพราะมีคนขับอยู่!");
                return;
            }

            // 2. สั่งเรียกเรือมายังจุดที่ตั้งไว้!
            playerBoat.TeleportBoatTo(summonPoint);
        }
        else
        {
            Debug.LogError("<color=red>หาเรือไม่เจอ หรือยังไม่ได้ตั้งค่า Summon Point ใน Inspector!</color>");
        }
    }
}