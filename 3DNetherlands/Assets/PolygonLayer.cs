using Netherlands3D.LayerSystem;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class PolygonLayer : Layer
{
    public override void HandleTile(TileChange tileChange, Action<TileChange> callback = null)
    {
		TileAction action = tileChange.action;
		var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
		switch (action)
		{
			case TileAction.Create:
				Tile newTile = CreateNewTile(tileChange, callback);
				tiles.Add(tileKey, newTile);
				break;
			case TileAction.Remove:
				InteruptRunningProcesses(tileKey);
				RemoveTile(tileChange, callback);
				return;
			default:
				callback(tileChange);
				break;
		}
	}

	private Tile CreateNewTile(TileChange tileChange, System.Action<TileChange> callback = null)
	{
		Tile tile = new Tile();
		tile.LOD = 0;
		tile.tileKey = new Vector2Int(tileChange.X, tileChange.Y);
		tile.layer = transform.gameObject.GetComponent<Layer>();
		tile.gameObject = new GameObject("perceel-" + tileChange.X + "_" + tileChange.Y);
		tile.gameObject.transform.parent = transform.gameObject.transform;
		tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
		tile.gameObject.SetActive(false);
		Generate(tileChange, tile, callback);
		return tile;
	}

	public void Generate(TileChange tileChange, Tile tile, System.Action<TileChange> callback = null)
	{
		tile.runningCoroutine = StartCoroutine(BuildLineNetwork(tileChange, tile, callback));
	}

	IEnumerator BuildLineNetwork(TileChange tileChange, Tile tile, Action<TileChange> callback = null)
	{
		var bbox = tile.tileKey.x + "," + tile.tileKey.y + "," + (tile.tileKey.x + tileSize) + "," + (tile.tileKey.y + tileSize);
		string url = $"https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=kadastralekaartv4:perceel&STARTINDEX=0&COUNT=1000&SRSNAME=urn:ogc:def:crs:EPSG::28992&BBOX={bbox},urn:ogc:def:crs:EPSG::28992&outputFormat=json";
		var polygonsPerceel = GetPerceelPolygon(url);

		//RenderPolygons(polygonsPerceel, center, PerceelMaterial, Perceel);

		//Finaly activate our new tile gameobject (if layer is not disabled)
		tile.gameObject.SetActive(isEnabled);

		yield return new WaitForEndOfFrame();

		yield return null;
		callback(tileChange);
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

	void RenderPolygons(List<Vector2[]> polygons, Vector2 center, Material material, Transform parent)
	{
		
		foreach (var polygon in polygons)
		{
			List<int> indices = new List<int>();
			List<Vector3> points = new List<Vector3>();

			foreach (var point in polygon)
			{
				points.Add(new Vector3(point.x - center.x, 0, point.y - center.y));
			}

			for (int i = 0; i < polygon.Length-1; i++)
			{
				indices.Add(i);
				indices.Add(i+1);
			}
		}
	}

	void RenderPolygon(Vector2[] polygonPoints, Material lineMaterial, Vector2 center, Transform parent)
	{

		List<int> indices = new List<int>();
		List<Vector3> points = new List<Vector3>();

		foreach (var point in polygonPoints)
		{
			points.Add(new Vector3(point.x - center.x, 0, point.y - center.y));
		}

		for (int i = 0; i < polygonPoints.Length; i++)
		{
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
		newgameobject.transform.parent = parent;

		MeshFilter filter = newgameobject.AddComponent<MeshFilter>();
		// newgameobject.AddComponent<Renderer>();
		newgameobject.AddComponent<MeshRenderer>().material = lineMaterial;

		var mesh = new Mesh();
		mesh.vertices = points.ToArray();
		mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
		filter.sharedMesh = mesh;
	}

	private void RemoveTile(TileChange tileChange, System.Action<TileChange> callback = null)
	{
		var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
		if (tiles.ContainsKey(tileKey))
		{
			Tile tile = tiles[tileKey];
			if (tile.gameObject)
			{
				MeshFilter[] meshFilters = tile.gameObject.GetComponentsInChildren<MeshFilter>();

				foreach (var meshfilter in meshFilters)
				{
					Destroy(meshfilter.sharedMesh);
				}

				Destroy(tile.gameObject);
			}

			tiles.Remove(tileKey);
		}
		callback(tileChange);
	}

}
