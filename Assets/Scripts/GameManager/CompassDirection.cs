using UnityEngine;

public class CompassDirection : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform targetIsland;
    public RectTransform compassUI;

    void Update()
    {
        if (player == null || targetIsland == null || compassUI == null)
        {
            compassUI.gameObject.SetActive(false);
            return;
        }

        if (!compassUI.gameObject.activeSelf)
            compassUI.gameObject.SetActive(true);

        
        Vector3 directionToTarget = targetIsland.position - player.position;
        directionToTarget.y = 0;

        
        float angle = Vector3.SignedAngle(player.forward, directionToTarget, Vector3.up);

        
        compassUI.localEulerAngles = new Vector3(0, 0, -angle);
    }

    public void SetTarget(Transform newTarget)
    {
        targetIsland = newTarget;
    }
}