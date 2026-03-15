using UnityEngine;
using UnityEngine.InputSystem;

public class NPCBuyer : MonoBehaviour
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
            if (!BuyerManager.instance.dialogueUI.activeSelf)
            {
                BuyerManager.instance.OpenDialogue();

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
            if (interactPrompt != null && !BuyerManager.instance.dialogueUI.activeSelf)
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
            if (interactPrompt != null) interactPrompt.SetActive(false);

            BuyerManager.instance.CloseDialogue();
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