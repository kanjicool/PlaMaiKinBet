using UnityEngine;

public class AlignObjects : MonoBehaviour
{
    [Header("Grid Settings")]
    public int columns = 5;

    [Tooltip("axis X")]
    public float spacingX = 2f;

    [Tooltip("axis Z)")]
    public float spacingZ = 2f;

    [ContextMenu("Align in Grid")]
    public void AlignGrid()
    {
        if (columns <= 0)
        {
            columns = 1;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            int row = i / columns;
            int col = i % columns;

            Vector3 newPosition = new Vector3(col * spacingX, 0, row * spacingZ);
            
            transform.GetChild(i).localPosition = newPosition;
        }

        Debug.Log("Finished arrangement...");
    }

}
