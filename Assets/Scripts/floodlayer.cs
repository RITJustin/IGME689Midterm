using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;

public class FloodMeshDraped : MonoBehaviour
{
    [Header("ArcGIS Settings")]
    public ArcGISMapComponent mapComponent;
    public string featureServiceUrl = "https://services2.arcgis.com/RQcpPaCpMAXzUI5g/arcgis/rest/services/polygene75recl3D/FeatureServer/0/query?f=geojson&where=1=1&outfields=*";

    [Header("Mesh Settings")]
    public float extrusionScale = 0.33f; // divide CorrectedFlood by 3
    public Material floodMaterial;
    public bool makeColliderConvex = true; // for CharacterController interaction

    private GameObject floodParent;
    private List<JToken> features;

    IEnumerator Start()
    {
        if (mapComponent == null)
            mapComponent = FindFirstObjectByType<ArcGISMapComponent>();

        floodParent = new GameObject("FloodLayerParent");

        // Wait a short time for the map to initialize
        yield return new WaitForSeconds(2f);

        // Then fetch features and build mesh
        yield return StartCoroutine(GetFeaturesAndCreateMesh());
    }

    IEnumerator GetFeaturesAndCreateMesh()
    {
        UnityWebRequest request = UnityWebRequest.Get(featureServiceUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        var jObject = JObject.Parse(request.downloadHandler.text);
        features = jObject.SelectToken("features").ToObject<List<JToken>>();

        CreateFloodMesh();
    }

    void CreateFloodMesh()
    {
        foreach (var feature in features)
        {
            var geometry = feature.SelectToken("geometry");
            if (geometry == null) continue;

            var coordinates = geometry["coordinates"];
            if (coordinates == null) continue;

            double correctedFlood = 0;
            var attr = feature.SelectToken("properties");
            if (attr != null && attr["CorrectedFlood"] != null)
                double.TryParse(attr["CorrectedFlood"].ToString(), out correctedFlood);

            float extrusionHeight = (float)(correctedFlood * extrusionScale);

            foreach (var ring in coordinates)
            {
                List<ArcGISPoint> points = new List<ArcGISPoint>();
                foreach (var point in ring)
                {
                    double lon = point[0].Value<double>();
                    double lat = point[1].Value<double>();
                    points.Add(new ArcGISPoint(lon, lat, 0, new ArcGISSpatialReference(4326)));
                }

                // Build and place mesh
                GameObject polygonObj = new GameObject("FloodPolygon");
                polygonObj.transform.parent = floodParent.transform;

                Mesh mesh = ExtrudePolygonOnMap(points, extrusionHeight);
                MeshFilter mf = polygonObj.AddComponent<MeshFilter>();
                MeshRenderer mr = polygonObj.AddComponent<MeshRenderer>();
                mf.mesh = mesh;
                mr.material = floodMaterial != null ? floodMaterial : new Material(Shader.Find("Standard")) { color = new Color(0, 0, 1, 0.5f) };

                // Add collider
                MeshCollider mc = polygonObj.AddComponent<MeshCollider>();
                mc.sharedMesh = mesh;
                mc.convex = makeColliderConvex;  // Convex required for CharacterController
                mc.isTrigger = false;            // Do not use trigger for swimming detection
            }
        }
    }

    Mesh ExtrudePolygonOnMap(List<ArcGISPoint> points, float height)
    {
        List<Vector3> baseVertices = new List<Vector3>();
        foreach (var pt in points)
        {
            Vector3 v = mapComponent.GeographicToEngine(pt);
            // Vertical offset removed for collider alignment
            baseVertices.Add(v);
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int baseStart = 0;
        vertices.AddRange(baseVertices);

        int topStart = vertices.Count;
        foreach (var v in baseVertices)
            vertices.Add(new Vector3(v.x, v.y + height, v.z));

        // Triangulate bottom and top
        for (int i = 1; i < baseVertices.Count - 1; i++)
        {
            // Bottom
            triangles.Add(baseStart);
            triangles.Add(baseStart + i + 1);
            triangles.Add(baseStart + i);

            // Top
            triangles.Add(topStart);
            triangles.Add(topStart + i);
            triangles.Add(topStart + i + 1);
        }

        // Build sides
        for (int i = 0; i < baseVertices.Count; i++)
        {
            int next = (i + 1) % baseVertices.Count;
            int b0 = baseStart + i;
            int b1 = baseStart + next;
            int t0 = topStart + i;
            int t1 = topStart + next;

            triangles.Add(b0); triangles.Add(t0); triangles.Add(t1);
            triangles.Add(b0); triangles.Add(t1); triangles.Add(b1);
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        return mesh;
    }
}
