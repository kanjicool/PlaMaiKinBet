using UnityEngine;
using UnityEngine.InputSystem;

public class NPCShop : MonoBehaviour
{
    [Header("UI TalkButton")]
    public GameObject interactPrompt;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] talkSounds; // ใส่เสียงคุยหลายๆ เสียงตรงนี้

    private bool isPlayerNear = false;

    private void Start()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerNear && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!ShopManager.instance.shopUI.activeSelf && !ShopManager.instance.dialogueUI.activeSelf)
            {
                Debug.Log("กด E สำเร็จ! เปิดหน้าต่างสนทนากับ NPC");
                ShopManager.instance.OpenDialogue();

                PlayRandomTalkSound(); // สุ่มเล่นเสียงคุย

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

            if (interactPrompt != null) interactPrompt.SetActive(false);

            ShopManager.instance.CloseShop();
            ShopManager.instance.CloseDialogue();
        }
    }

    private void PlayRandomTalkSound()
    {
        if (audioSource != null && talkSounds != null && talkSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, talkSounds.Length);
            audioSource.PlayOneShot(talkSounds[randomIndex]);
        }
    }
}