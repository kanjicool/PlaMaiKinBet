using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestSlotUI : MonoBehaviour
{
    public Image fishIcon;
    public TextMeshProUGUI fishText;

    public void SetupSlot(Sprite icon, string nameAndAmount)
    {
        if (fishIcon != null)
        {
            fishIcon.sprite = icon;
            fishIcon.enabled = (icon != null); // ป้องกันบั๊กกรณีลืมใส่รูปใน FishData
        }

        if (fishText != null)
        {
            fishText.text = nameAndAmount;
        }
    }
}