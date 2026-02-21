using UnityEngine;

public class SpawnRate : MonoBehaviour
{
    public FishData myData;

    private enum FishState { Swimming, Nibbling, Hooked, Caught }
    private FishState currentState = FishState.Swimming;

    private Animator fishAnimator;

    void Start ()
    {
        fishAnimator = GetComponent<Animator>();
    }

    public void Initialize(FishData data)
    {
        myData = data;
        Debug.Log($"Fish {myData.fishName} is spawn!");
    }

    public void BiteHook()
    {
        currentState = FishState.Hooked;
        Debug.Log($"Fish bit the hook! EscapePower = {myData.escapePower}");
        if (fishAnimator != null)
        {
            fishAnimator.SetTrigger("isHooked");
        }

    }

    public void Struggle()
    {
        if (currentState == FishState.Hooked)
        {
            Debug.Log("Struggle");
        }
    }
}
