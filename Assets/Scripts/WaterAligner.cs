using UnityEngine;

public class WaterAligner : MonoBehaviour
{
    [Header("Flood Reference")]
    public Transform floodMesh;       // Assign your FloodMeshDraped parent
    public Vector3 localOffset;       // Optional: small offset above flood surface

    void LateUpdate()
    {
        if (floodMesh != null)
        {
            // Keep water mesh at the flood mesh position + offset
            transform.position = floodMesh.position + localOffset;

            // Optional: match rotation if flood mesh rotates
            transform.rotation = floodMesh.rotation;

            // Optional: match scale if needed
            // transform.localScale = floodMesh.localScale;
        }
    }
}
