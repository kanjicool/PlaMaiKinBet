using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [Header("Hotbar Slots")]
    public GameObject[] itemSlots = new GameObject[6];

    private InputSystem_Actions inputActions;
    private int currentItemIndex = -1;


    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Player.Slot1.performed += ctx => EquipItem(0);
        inputActions.Player.Slot2.performed += ctx => EquipItem(1);
        inputActions.Player.Slot3.performed += ctx => EquipItem(2);
        inputActions.Player.Slot4.performed += ctx => EquipItem(3);
        inputActions.Player.Slot5.performed += ctx => EquipItem(4);
        inputActions.Player.Slot6.performed += ctx => EquipItem(5);
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void EquipItem(int index)
    {
        if (index >= itemSlots.Length || itemSlots[index] == null) return;

        if (currentItemIndex == index)
        {
            itemSlots[index].SetActive(false);
            currentItemIndex = -1;
            return;
        }

        if (currentItemIndex != -1 && itemSlots[currentItemIndex] != null)
        {
            itemSlots[currentItemIndex].SetActive(false);
        }

        itemSlots[index].SetActive(true);
        currentItemIndex = index;
    }
}
