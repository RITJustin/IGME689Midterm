using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;

public class ArcGISFeatureLayerComponent : MonoBehaviour
{
    [System.Serializable]
    public class QueryLink
    {
        public string Link;
        public string[] RequestHeaders;
    }

    [SerializeField] private ArcGISMapComponent mapComponent;
    public QueryLink WebLink;

    [SerializeField] private float extrusionScale = 1f;   // Scale gridcode for extrusion
    [SerializeField] private float terrainOffset = 0.1f;   // Small lift to prevent z-fighting

    private GameObject floodLayerParent;
    private List<JToken> jFeatures;

    private void Start()
    {
        mapComponent = FindFirstObjectByType<ArcGISMapComponent>();

        floodLayerParent = new GameObject("FloodLayerParent");
        floodLayerParent.transform.position = Vector3.zero;

        StartCoroutine(GetAndRenderFloodFeatures());
    }

    private IEnumerator GetAndRenderFloodFeatures()
    {
        UnityWebRequest request = UnityWebRequest.Get(WebLink.Link);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        var jObject = JObject.Parse(request.downloadHandler.text);
        jFeatures = jObject.SelectToken("features").ToObject<List<JToken>>();

        CreateFeatures();
        yield return null;
    }

    private void CreateFeatures()
    {
        foreach (var feature in jFeatures)
        {
            var geometry = feature.SelectToken("geometry");
            if (geometry == null) continue;

            var coordinates = geometry["coordinates"];
            if (coordinates == null) continue;

            // Get "gridcode" for extrusion height
            var attributes = feature.SelectToken("properties");
            double gridcode = 0;
            if (attributes != null && attributes["gridcode"] != null)
                double.TryParse(attributes["gridcode"].ToString(), out gridcode);

            float extrusionHeight = (float)(gridcode * extrusionScale);

            foreach (var ring in coordinates)
            {
                List<ArcGISPoint> arcGISPoints = new List<ArcGISPoint>();
                foreach (var point in ring)
                {
                    double lon = point[0].Value<double>();
                    double lat = point[1].Value<double>();
                    ArcGISPoint arcPoint = new ArcGISPoint(lon, lat, 0, new ArcGISSpatialReference(4326));
                    arcGISPoints.Add(arcPoint);
                }

                // Create extruded polygon draped on map
                GameObject polygonObj = new GameObject("DrapedFloodPolygon");
                polygonObj.transform.parent = floodLayerParent.transform;

                Mesh mesh = ExtrudePolygonOnMap(arcGISPoints, extrusionHeight);
                var mf = polygonObj.AddComponent<MeshFilter>();
                var mr = polygonObj.AddComponent<MeshRenderer>();
                mf.mesh = mesh;
                mr.material = new Material(Shader.Find("Standard")) { color = Color.blue * 0.5f };
            }
        }
    }

    private Mesh ExtrudePolygonOnMap(List<ArcGISPoint> points, float height)
    {
        List<Vector3> baseVertices = new List<Vector3>();
        foreach (var pt in points)
        {
            Vector3 v = mapComponent.GeographicToEngine(pt);
            v.y += terrainOffset; // only small lift to avoid z-fighting
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
