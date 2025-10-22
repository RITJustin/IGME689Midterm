using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ChildWaterAligner : MonoBehaviour
{
    [Tooltip("Vertical offset above the parent flood mesh")]
    public float localHeightOffset = 0.2f;

    [Tooltip("Update continuously if the flood mesh moves or animates")]
    public bool updateContinuously = false;

    private Transform parentFloodTransform;

    void Start()
    {
        // Find the parent transform (assumes this water is a child)
        if (transform.parent != null)
            parentFloodTransform = transform.parent;
        else
            Debug.LogWarning("Water object has no parent flood layer. Local offset will be relative to world origin.");

        // Initial alignment
        AlignToParent();
    }

    void Update()
    {
        if (updateContinuously)
            AlignToParent();
    }

    void AlignToParent()
    {
        if (parentFloodTransform != null)
        {
            // Keep water directly above parent, applying small local Y offset
            transform.localPosition = new Vector3(0, localHeightOffset, 0);
            // Optional: reset rotation to match parent (usually not needed)
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            // Fallback: align to world origin Y + offset
            Vector3 pos = transform.position;
            pos.y = localHeightOffset;
            transform.position = pos;
        }
    }
}
