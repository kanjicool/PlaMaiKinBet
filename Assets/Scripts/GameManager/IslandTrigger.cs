using UnityEngine;

public enum IslandType { QuestIsland, HubIsland }

public class IslandTrigger : MonoBehaviour
{
    public IslandType islandType = IslandType.QuestIsland;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && islandType == IslandType.QuestIsland) return; // Quest เกาะทริกเกอร์ครั้งเดียว

        if (other.CompareTag("Player"))
        {
            if (GameLoopManager.Instance != null)
            {
                if (islandType == IslandType.QuestIsland)
                {
                    hasTriggered = true;
                    GameLoopManager.Instance.OnReachQuestIsland();
                }
                else if (islandType == IslandType.HubIsland)
                {
                    // ถ้าถึงเกาะ Hub ให้ลองส่งอาหารบอส
                    GameLoopManager.Instance.FeedBoss();
                }
            }
        }
    }
}