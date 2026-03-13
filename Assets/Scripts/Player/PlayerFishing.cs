using UnityEngine;
using UnityEngine.Rendering;

public class PlayerFishing : MonoBehaviour
{

    [Header("Fishing States")]
    public bool isEquipped;
    public bool isCharging;
    public bool isFishingIdle;

    public bool isReeling;

    private Animator anim;
    private PlayerController playerController;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (isFishingIdle || isCharging)
        {
            if (playerController != null && playerController.IsBusy)
            {
                CancelFishing();
            }
        }
    }

    public void SetEquippedState(bool equipped)
    {
        isEquipped = equipped;
    }

    public void StartCharging()
    {
        if (!isEquipped || (playerController != null && playerController.IsBusy)) return;

        isCharging = true;
        isFishingIdle = false;
        anim.SetBool("isChargingCast", true);
        anim.SetBool("isFishingIdle", false);
    }

    public void ExecuteCast()
    {
        if (!isCharging) return;

        isCharging = false;
        isFishingIdle = true;

        anim.SetBool("isChargingCast", false);
        //anim.SetTrigger("castRod");
        anim.SetBool("isFishingIdle", true);
    }

    public void CancelFishing()
    {
        isCharging = false;
        isFishingIdle = false;

        anim.SetBool("isChargingCast", false);
        anim.SetBool("isFishingIdle", false);
    }

    public void UpdateChargeAnimation(float chargePercentage)
    {
        if (isCharging && anim != null)
        {
            anim.SetFloat("chargeProgress", chargePercentage);
        }
    }
}
