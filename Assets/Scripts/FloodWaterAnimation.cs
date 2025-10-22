using UnityEngine;

public class FloodWaterAnimation : MonoBehaviour
{
    [Header("Material Settings")]
    public Material floodMaterial;           // assign your HDRP water material
    public Vector2 scrollSpeed = new Vector2(0.05f, 0.05f); // speed of normal map movement
    public Vector2 normalMapTiling = new Vector2(10f, 10f); // increase tiling for large meshes

    private Vector2 initialOffset;

    void Start()
    {
        if (floodMaterial == null)
        {
            Debug.LogError("FloodWaterAnimation: No material assigned!");
            enabled = false;
            return;
        }

        // Set the tiling once
        floodMaterial.SetTextureScale("_NormalMap", normalMapTiling);

        // Store the initial offset to scroll from
        initialOffset = floodMaterial.GetTextureOffset("_NormalMap");
    }

    void Update()
    {
        if (floodMaterial != null)
        {
            Vector2 offset = initialOffset + new Vector2(Time.time * scrollSpeed.x, Time.time * scrollSpeed.y);
            floodMaterial.SetTextureOffset("_NormalMap", offset);
        }
    }
}
