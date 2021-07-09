using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

public class CreatePolygon : MonoBehaviour
{
    public Material LineMaterial;
    
    void Start()
    {

        //List<Vector2> polygon = new List<Vector2>();
        //polygon.Add(new Vector2(0, 0));
        //polygon.Add(new Vector2(0, 4));
        //polygon.Add(new Vector2(2, 3));
        //polygon.Add(new Vector2(4, 4));
        //polygon.Add(new Vector2(4, 0));
        //polygon.Add(new Vector2(2, 1));
        ////polygon.Add(new Vector2(0, 0));

        //RenderPolygon(polygon.ToArray(), LineMaterial, gameObject);

        string bbox = "157000,466000,15800,467000";
        string url = $"https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=kadastralekaartv4:perceel&STARTINDEX=0&COUNT=1000&SRSNAME=urn:ogc:def:crs:EPSG::28992&BBOX={bbox},urn:ogc:def:crs:EPSG::28992&outputFormat=json";
        string url_bebouwing = $"https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=kadastralekaartv4:bebouwing&STARTINDEX=0&COUNT=1000&SRSNAME=urn:ogc:def:crs:EPSG::28992&BBOX={bbox},urn:ogc:def:crs:EPSG::28992&outputFormat=json";
        //var polygons = GetPerceelPolygon(url);
        Vector2 center = new Vector2(157500, 466500);
        var polygons = GetBebouwingPolygons(url_bebouwing, center);

        foreach (var polygon in polygons)
        {
            RenderPolygon(polygon, LineMaterial, gameObject);
        }

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

                var offsetX = feature.properties.perceelnummerPlaatscoordinaatX;
                var offsetY = feature.properties.perceelnummerPlaatscoordinaatY;

                var deltaX = feature.properties.perceelnummerVerschuivingDeltaX;
                var deltaY = feature.properties.perceelnummerVerschuivingDeltaY;

                //Debug.Log($"perceelnummerVerschuivingDeltaX: {feature.properties.perceelnummerVerschuivingDeltaX} ");
                //Debug.Log($"perceelnummerRotatie: {feature.properties.perceelnummerRotatie} ");

                var coordinates = feature.geometry.coordinates;
                foreach(var points in coordinates)
                {
                    foreach(var point in points)
                    {
                        polygonList.Add( new Vector2( point[0]-offsetX + deltaX, 
                                                      point[1]-offsetY + deltaY));
                    }
                }
                list.Add(polygonList.ToArray());
            }           
        }


        return list;
    }

    List<Vector2[]> GetBebouwingPolygons(string url, Vector2 center)
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
                        polygonList.Add(new Vector2(point[0] - center.x, point[1] - center.y));
                    }
                }
                list.Add(polygonList.ToArray());
            }
        }


        return list;
    }

    void RenderPolygon(Vector2[] polygonPoints, Material lineMaterial, GameObject gam)
    {
        List<int> indices = new List<int>();
        List<Vector3> points = new List<Vector3>();

        foreach (var point in polygonPoints){
            points.Add(new Vector3(point.x, 0, point.y));
        }

        for (int i = 0; i < polygonPoints.Length; i++){
            indices.Add(i);

            if (i == polygonPoints.Length - 1)
            {
                indices.Add(0);
            }
            else
            {
                indices.Add(i + 1);
            }
        }

        GameObject newgameobject = new GameObject();

        MeshFilter filter = newgameobject.AddComponent<MeshFilter>();
       // newgameobject.AddComponent<Renderer>();
        newgameobject.AddComponent<MeshRenderer>().material = lineMaterial;

        var mesh = new Mesh();
        mesh.vertices = points.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        filter.sharedMesh = mesh;
    }


}
