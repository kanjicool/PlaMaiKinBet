using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UniversalBuoyancy : MonoBehaviour
{
    public Transform[] floatPoints;

    public float floatForce = 20f;     
    public float waterDrag = 2f;       
    public float waterAngularDrag = 1f; 

    private Rigidbody rb;
    private bool isUnderwater;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;

        if (floatPoints == null || floatPoints.Length == 0)
        {
            GameObject centerPoint = new GameObject("CenterFloatPoint");
            centerPoint.transform.parent = transform;
            centerPoint.transform.localPosition = Vector3.zero;
            floatPoints = new Transform[] { centerPoint.transform };
        }
    }

    private void FixedUpdate()
    {
        isUnderwater = false;

        foreach (Transform point in floatPoints)
        {
            float waterHeight = WaveManager.Instance.GetWaterHeightAtPosition(point.position);

            if (point.position.y < waterHeight)
            {
                isUnderwater = true;
                float depth = waterHeight - point.position.y;

                Vector3 uplift = Vector3.up * floatForce * depth;
                rb.AddForceAtPosition(uplift, point.position, ForceMode.Acceleration);

                rb.AddForceAtPosition(-rb.linearVelocity * waterDrag, point.position, ForceMode.Acceleration);
                rb.AddTorque(-rb.angularVelocity * waterAngularDrag, ForceMode.Acceleration);
            }
        }
    }
}