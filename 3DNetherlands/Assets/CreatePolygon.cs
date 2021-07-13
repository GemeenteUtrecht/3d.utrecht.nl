using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;

public class CreatePolygon : MonoBehaviour
{
    public Material BebouwingMaterial;
    public Material PerceelMaterial;

    public Transform Bebouwing;
    public Transform Perceel;

    void Start()
    {
        //# Amsterdam
        //# bbox_min_x = 109000
        //# bbox_min_y = 474000
        //# bbox_max_x = 141000
        //# bbox_max_y = 501000

        //# Utrecht
        //bbox_min_x = 123000
        //bbox_min_y = 443000
        //bbox_max_x = 146000
        //bbox_max_y = 464000

        //string bbox = "150000,460000,15800,467000";
        //string bbox = "109000,474000,141000,501000";
        string bbox = "123000,486000,124000,487000";
        string url = $"https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=kadastralekaartv4:perceel&STARTINDEX=0&COUNT=1000&SRSNAME=urn:ogc:def:crs:EPSG::28992&BBOX={bbox},urn:ogc:def:crs:EPSG::28992&outputFormat=json";
        string url_bebouwing = $"https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=kadastralekaartv4:bebouwing&STARTINDEX=0&COUNT=1000&SRSNAME=urn:ogc:def:crs:EPSG::28992&BBOX={bbox},urn:ogc:def:crs:EPSG::28992&outputFormat=json";

        var testpolygon = GetTestPolygon();
        var testpolygon2 = GetTestPolygon2();
 //       var testpolygonSquareClockwise = GetTestPolygonSquareClockwise();
 //       var testpolygonSquareCounterClockwise = GetTestPolygonSquareCounterClockwise();
        var polygonsPerceel = GetPerceelPolygon(url);
        var polygonsBebouwing = GetBebouwingPolygons(url_bebouwing);

        //var center = GetCenter(testpolygonSquareClockwise);
        //var center = GetCenter(testpolygonSquareCounterClockwise);
        //var center = GetCenter(testpolygonSquare);
        //var center = GetCenter(testpolygon2);
        //var center = GetCenter(testpolygon);
        var center = GetCenter(polygonsPerceel);
        //var center = GetCenter(polygonsBebouwing);

        //RenderPolygons(testpolygon2, center, PerceelMaterial, Perceel);
        //RenderPolygons(testpolygon, center, PerceelMaterial, Perceel);
        //RenderPolygons(polygonsPerceel, center, PerceelMaterial, Perceel);
        RenderPolygons(polygonsPerceel, center, PerceelMaterial, Perceel);


        //RenderConvexPolygons(polygonsBebouwing.Take(1).ToList(), center, BebouwingMaterial, Bebouwing);
        //RenderConvexPolygons(polygonsBebouwing, center, BebouwingMaterial, Bebouwing);
        //RenderConvexPolygons(testpolygon, center, BebouwingMaterial, Bebouwing);
        //RenderConvexPolygons(testpolygon2, center, BebouwingMaterial, Bebouwing);
        //RenderConvexPolygons(testpolygonSquareClockwise, center, BebouwingMaterial, Bebouwing);
        //RenderConvexPolygons(testpolygonSquareCounterClockwise, center, BebouwingMaterial, Bebouwing);

        //TestTwoPolygons();

    }


    void RenderConvexPolygons(List<Vector2[]> polygons, Vector2 center, Material material, Transform parent)
    {
        foreach (var polygon in polygons)
        {            
            RenderPolygonConvex(polygon, material, center, parent);
        }
    }

    void TestTwoPolygons()
    {
        List<Vector2[]> polygons = new List<Vector2[]>();

        List<Vector2> pol1 = new List<Vector2>();
        pol1.Add(new Vector2(0, 0));
        pol1.Add(new Vector2(0, 4));
        pol1.Add(new Vector2(4, 4));
        pol1.Add(new Vector2(4, 0));
        pol1.Add(new Vector2(0, 0));

        List<Vector2> pol2 = new List<Vector2>();
        pol2.Add(new Vector2(10, 10));
        pol2.Add(new Vector2(10, 14));
        pol2.Add(new Vector2(14, 14));
        pol2.Add(new Vector2(14, 10));
        pol2.Add(new Vector2(10, 10));

        polygons.Add(pol1.ToArray());
        polygons.Add(pol2.ToArray());

        RenderPolygons(polygons, new Vector2(5, 7), BebouwingMaterial, transform);

    }


    void RenderPolygons(List<Vector2[]> polygons, Vector2 center, Material material, Transform parent)
    {
        List<Vector2> vertices = new List<Vector2>();
        List<int> indices = new List<int>();

        int count = 0;
        foreach (var list in polygons)
        {
            for (int i = 0; i < list.Length - 1; i++)
            {
                indices.Add(count + i);
                indices.Add(count + i + 1);
            }
            count += list.Length;
            vertices.AddRange(list);
        }

        GameObject newgameobject = new GameObject();
        newgameobject.transform.parent = parent;
        MeshFilter filter = newgameobject.AddComponent<MeshFilter>();
        newgameobject.AddComponent<MeshRenderer>().material = material;

        var mesh = new Mesh();
        mesh.vertices = vertices.Select(o => new Vector3(o.x -center.x ,0,o.y - center.y )).ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        filter.sharedMesh = mesh;       
    }


    Vector2 GetCenter(List<Vector2[]> polygons)
    {
        var minx = polygons.Min(l => l.Min(o => o.x));
        var maxx = polygons.Max(l => l.Max(o => o.x));
        var miny = polygons.Min(l => l.Min(o => o.y));
        var maxy = polygons.Max(l => l.Max(o => o.y));

        var centerx = minx + ((maxx - minx) / 2);
        var centery = miny + ((maxy - miny) / 2);

        Vector2 center = new Vector2(centerx, centery);
        return center;
    }

    List<Vector2[]> GetPerceelPolygon(string url)
    {
        List<Vector2[]> list = new List<Vector2[]>();

        using (WebClient client = new WebClient())
        using (Stream stream = client.OpenRead(url))
        using (StreamReader streamReader = new StreamReader(stream))
        using (JsonTextReader reader = new JsonTextReader(streamReader))
        {
            reader.SupportMultipleContent = true;
            var serializer = new JsonSerializer();
            JsonModels.WebFeatureService.WFSRootobject wfs = serializer.Deserialize<JsonModels.WebFeatureService.WFSRootobject>(reader);

            foreach(var feature in wfs.features)
            {
                List<Vector2> polygonList = new List<Vector2>();

                var coordinates = feature.geometry.coordinates;
                foreach(var points in coordinates)
                {
                    foreach(var point in points)
                    {
                        polygonList.Add(new Vector2(point[0], point[1]));
                    }
                }
                list.Add(polygonList.ToArray());
            }           
        }

        return list;
    }

    List<Vector2[]> GetBebouwingPolygons(string url)
    {
        List<Vector2[]> list = new List<Vector2[]>();

        using (WebClient client = new WebClient())
        using (Stream stream = client.OpenRead(url))
        using (StreamReader streamReader = new StreamReader(stream))
        using (JsonTextReader reader = new JsonTextReader(streamReader))
        {
            reader.SupportMultipleContent = true;
            var serializer = new JsonSerializer();
            JsonModels.WfsBebouwing.Rootobject wfs = serializer.Deserialize<JsonModels.WfsBebouwing.Rootobject>(reader);

            foreach (var feature in wfs.features)
            {
                List<Vector2> polygonList = new List<Vector2>();

                var coordinates = feature.geometry.coordinates;
                foreach (var points in coordinates)
                {
                    foreach (var point in points)
                    {                        
                        polygonList.Add(new Vector2(point[0], point[1]));
                    }
                }
                list.Add(polygonList.ToArray());
            }
        }


        return list;
    }

    List<Vector2[]> GetTestPolygon()
    {
        List<Vector2[]> polygons = new List<Vector2[]>();
        List<Vector2> polygon = new List<Vector2>();
        polygon.Add(new Vector2(0, 0));
        polygon.Add(new Vector2(0, 4));
        polygon.Add(new Vector2(2, 3));
        polygon.Add(new Vector2(10, 4));
        polygon.Add(new Vector2(4, 0));
        polygon.Add(new Vector2(2, 1));
        polygons.Add(polygon.ToArray());
        //polygon.Add(new Vector2(0, 0));
        return polygons;                
    }

    List<Vector2[]> GetTestPolygonSquareClockwise()
    {
        List<Vector2[]> polygons = new List<Vector2[]>();
        List<Vector2> polygon = new List<Vector2>();
        polygon.Add(new Vector2(0, 0));
        polygon.Add(new Vector2(0, 4));
        polygon.Add(new Vector2(4, 4));
        polygon.Add(new Vector2(4, 0));      
        polygons.Add(polygon.ToArray());        
        return polygons;
    }

    List<Vector2[]> GetTestPolygonSquareCounterClockwise()
    {
        List<Vector2[]> polygons = new List<Vector2[]>();
        List<Vector2> polygon = new List<Vector2>();
        polygon.Add(new Vector2(1, 0));
        polygon.Add(new Vector2(4, 0));
        polygon.Add(new Vector2(4, 4));
        polygon.Add(new Vector2(0, 4));
        polygons.Add(polygon.ToArray());
        return polygons;
    }

    List<Vector2[]> GetTestPolygon2()
    {
        List<Vector2[]> polygons = new List<Vector2[]>();
        List<Vector2> polygon = new List<Vector2>();

        polygon.Add(new Vector2(364.6016f, -521.6875f));
        polygon.Add(new Vector2(364.5938f,-521.7188f));
        polygon.Add(new Vector2(371.2031f,-523.875f));
        polygon.Add(new Vector2(371.8281f,-524.0625f));
        polygon.Add(new Vector2(374.4531f,-515.9375f));
        polygon.Add(new Vector2(373.4844f,-515.625f));
        polygon.Add(new Vector2(374.2188f,-513.4063f));
        polygon.Add(new Vector2(375.1797f,-513.75f));
        polygon.Add(new Vector2(378.3828f,-503.9688f));
        polygon.Add(new Vector2(376.4844f,-503.375f));
        polygon.Add(new Vector2(376.8516f,-502.25f));
        polygon.Add(new Vector2(373.7188f,-499.375f));
        polygon.Add(new Vector2(372.6094f,-499.8125f));
        polygon.Add(new Vector2(371.9141f,-498f));
        polygon.Add(new Vector2(307.1641f,-524.125f));
        polygon.Add(new Vector2(311.625f,-535.3438f));
        polygon.Add(new Vector2(357.2422f,-516.9375f));
        polygon.Add(new Vector2(355.5938f,-512.75f));
        polygon.Add(new Vector2(360.5938f,-510.7813f));
        polygon.Add(new Vector2(360.2656f,-509.9688f));
        polygon.Add(new Vector2(362.8906f,-508.9063f));
        polygon.Add(new Vector2(364.1563f,-508.3438f));
        polygon.Add(new Vector2(364.4766f,-508.75f));
        polygon.Add(new Vector2(364.1484f,-509.0313f));
        polygon.Add(new Vector2(364.7578f,-510.375f));
        polygon.Add(new Vector2(366.6016f,-512.125f));
        polygon.Add(new Vector2(367.9766f,-512.6563f));
        polygon.Add(new Vector2(368.2578f,-512.3438f));
        polygon.Add(new Vector2(368.6484f,-512.6563f));
        polygon.Add(new Vector2(368.2109f,-513.9688f));
        polygon.Add(new Vector2(367.2266f,-516.5938f));
        polygon.Add(new Vector2(366.375f,-516.3125f));
       // polygon.Add(new Vector2(364.6016f,-521.6875f));

        polygons.Add(polygon.ToArray());
        return polygons;
    }

 
    //http://wiki.unity3d.com/index.php?title=Triangulator
    void RenderPolygonConvex(Vector2[] polygonPoints, Material lineMaterial, Vector2 center, Transform parent)
    {

        if (polygonPoints.First().Equals( polygonPoints.Last()))
        {
            polygonPoints = polygonPoints.Take(polygonPoints.Length - 1).ToArray();            
        }


        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(polygonPoints);
        int[] indices = tr.Triangulate();

        Vector3[] vertices = new Vector3[polygonPoints.Length];
        for (int i = 0; i < polygonPoints.Length; i++)
        {
           vertices[i] = new Vector3(polygonPoints[i].x - center.x, 0, polygonPoints[i].y - center.y);

           //debug object to show the points
           var debugobject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
           debugobject.transform.position = vertices[i];
           debugobject.transform.localScale = Vector3.one * 0.4f;
           debugobject.name = $"{i + 1}";
           Destroy(debugobject.GetComponent<Collider>());

           Debug.Log($"x:{vertices[i].x} y:{vertices[i].z}");

        }

        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;

        //var normals = Enumerable.Repeat( new Vector3(0,1,0) , polygonPoints.Length).ToArray();
        //msh.normals = normals;

        msh.RecalculateNormals();
        msh.RecalculateBounds();

        GameObject gam = new GameObject();
        gam.transform.parent = parent;
        gam.AddComponent<MeshRenderer>().material = lineMaterial;
        gam.AddComponent<MeshFilter>().mesh = msh;
    }
}
    


