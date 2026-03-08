using UnityEngine;

public class IslandTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.OnReachNewIsland(this.gameObject);
            }
        }
    }
}