using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using ConvertCoordinates;
using System.Linq;
using Netherlands3D;

namespace Amsterdam3D.DataGeneration
{
	public class GenerateTreeDataUtrecht : MonoBehaviour
	{
		[Serializable]
		private class Tree
		{
			public string OBJECTNUMMER;
			public string Soortnaam_NL;
			public string Boomnummer;
			public string Soortnaam_WTS;
			public string Boomtype;
			public string Boomhoogte;
			public int Plantjaar;
			public string Eigenaar;
			public string Beheerder;
			public string Categorie;
			public string SOORT_KORT;
			public string SDVIEW;
			public string RADIUS;

			public Vector3RD RD;
			public Vector3 position;
			public float averageTreeHeight;

			public GameObject prefab;
		}

		private const string treeTileAssetsFolder = "Assets/TreeTileAssets/";
		private const float raycastYRandomOffsetRange = 0.08f;
		[SerializeField]
		private GameObjectsGroup treeTypes;

		[SerializeField]
		private TextAsset[] bomenCsvDataFiles;

		private List<Tree> trees;

		[SerializeField]
		private Material previewMaterial;
		[SerializeField]
		private Material treesMaterial;

		private double tileSize = 1000.0;
		
        //private string sourceGroundTilesFolder = "C:/Projects/GemeenteAmsterdam/1x1kmGroundTiles";
        private string sourceGroundTilesFolder = @"F:\Files\Assetbundles\terrain_rd";


        private string[] treeNameParts;
		private string treeTypeName = "";

		[SerializeField]
		private List<string> noPrefabFoundNames;

		private Vector2RD tileOffset;
		private Vector3 unityTileOffset;

        public GameObject TestCube;

        public string CsvFile;

        public List<Material> _terrainMaterials;

        public void Start()
		{

            Directory.CreateDirectory("Assets/TreeTileAssets/");

			//Calculate offset. ( Our viewer expects tiles with the origin in the center )
			tileOffset = Config.activeConfiguration.RelativeCenterRD;// CoordConvert.referenceRD;
			tileOffset.x -= 500;
			tileOffset.y -= 500;			
			unityTileOffset = CoordConvert.RDtoUnity( new Vector2((float)tileOffset.x, (float)tileOffset.y ));

			trees = new List<Tree>();
		
			noPrefabFoundNames = new List<string>();
           
			ReadTreesFromCsv();
            
			//DrawTrees(bomen.Take(100).ToList());
            //DrawTrees(bomen);

            StartCoroutine(TraverseTileFiles());

        }

        void DrawTrees(List<Tree> trees)
        {
            foreach(var tree in trees)
            {
                var boom = Instantiate(treeTypes.items[0]);
                boom.transform.position = new Vector3(  (float)(tileOffset.x - tree.RD.x), 0, (float) (tileOffset.y -tree.RD.y));
                boom.transform.parent = transform;
            }
        }

        void ReadTreesFromCsv()
        {
            var lines = File.ReadAllLines(CsvFile);

            foreach(var line in lines.Skip(1))
            {
                try
                {
                    var columns = line.Split(';');
                    var tree = new Tree();

                    tree.OBJECTNUMMER = columns[0];                    
                    tree.Soortnaam_NL = columns[1];
                    tree.Boomhoogte = columns[2];
                    tree.Plantjaar = int.Parse(columns[3]);
                    tree.RD = new Vector3RD(Convert.ToDouble(columns[5]), Convert.ToDouble(columns[6]), 0);

                    //var longlat = ConvertToLatLong(tree.RD.x, tree.RD.y);

                    tree.position = CoordConvert.RDtoUnity(tree.RD);
                    //tree.position = CoordConvert.WGS84toUnity(longlat.longitude, longlat.latitude);


                    tree.averageTreeHeight = EstimateTreeHeight(tree.Boomhoogte);
                    tree.prefab = FindClosestPrefabTypeByName(tree.Soortnaam_NL);
                    //tree.prefab = TestCube;
                    trees.Add(tree);
                }
                catch
                {
                }
            }

            //398 soorten bomen
            var soorten = trees.GroupBy(o => o.Soortnaam_NL).ToArray();
            var hoogtrd = trees.GroupBy(o => o.Boomhoogte).ToArray();
            var oldestTree = trees.Min(o => o.Plantjaar);

            Debug.Log($"Aantal bomen:{trees.Count} soorten:{soorten.Length} Oudste boom:{oldestTree}");

            var minx = trees.Min(o => o.RD.x);
            var miny = trees.Min(o => o.RD.y);
            var maxx = trees.Max(o => o.RD.x);
            var maxy = trees.Max(o => o.RD.y);

            var avgHoogteMin = trees.Min(o => o.averageTreeHeight);
            var avgHoogteMax = trees.Max(o => o.averageTreeHeight);

            Debug.Log($"minx:{minx} maxx:{maxx} miny:{miny} maxy:{maxy}");

            //minx:126805.07 maxx:141827.31 miny:448979.02 maxy:461149.85

        }

		/// <summary>
		/// Find a prefab in our list of tree prefabs that has a substring matching a part of our prefab name.
		/// Make sure prefab names are unique to get unique results.
		/// </summary>
		/// <param name="treeTypeDescription">The string containing the tree type word</param>
		/// <returns>The prefab with a matching substring</returns>
		private GameObject FindClosestPrefabTypeByName(string treeTypeDescription)
		{
			treeNameParts = treeTypeDescription.Replace("\"", "").Split(' ');

			foreach (var namePart in treeNameParts)
			{
				treeTypeName = namePart.ToLower();
				foreach (GameObject tree in treeTypes.items)
				{
					if (tree.name.ToLower().Contains(treeTypeName))
					{
						return tree;
					}
				}
			}
			noPrefabFoundNames.Add(treeTypeDescription);
			return treeTypes.items[3]; //Just use an average tree prefab as default
		}

		/// <summary>
		/// Estimate the tree height according to the height description.
		/// We try to parse every number found, and use the average.
		/// </summary>
		/// <param name="description">For example: "6 to 8 m"</param>
		/// <returns></returns>
		private float EstimateTreeHeight(string description)
		{
            try
            {


                if (description.Contains(','))
                {
                    return float.Parse(description.Replace(',', '.'));
                }
                else if (description.Contains('.'))
                {
                    return float.Parse(description);
                }
            }
            catch(Exception e)
            {

            }

            float treeHeight = 10.0f;

			string[] numbers = description.Split(' ');
			int numbersFoundInString = 0;
			float averageHeight = 0;
			foreach (string nr in numbers)
			{
				float parsedNumber = 10;

				if (float.TryParse(nr, out parsedNumber))
				{
					numbersFoundInString++;
					averageHeight += parsedNumber;
				}
			}
			if (numbersFoundInString > 0)
			{
				treeHeight = averageHeight / numbersFoundInString;
			}
            
			return treeHeight;
		}

		/// <summary>
		/// Load all the large ground tiles from AssetBundles, spawn it in our world, and start filling it with the trees that match the tile
		/// its RD coordinate rectangle. The tiles are named after the RD coordinates in origin at the bottomleft of the tile.
		/// </summary>
		private IEnumerator TraverseTileFiles()
		{
			var info = new DirectoryInfo(sourceGroundTilesFolder);
			var fileInfo = info.GetFiles();

			var currentFile = 0;
			while(currentFile < fileInfo.Length)
			{
				FileInfo file = fileInfo[currentFile];
				if (!file.Name.Contains(".manifest") && file.Name.Contains("_"))
				{
					Debug.Log($"Filling tile {currentFile}/{fileInfo.Length} {file.Name}" );
					yield return new WaitForEndOfFrame();


					string[] splitted = file.Name.Split('_');
                    string[] coordinates = splitted[1].Split('-');

                    Vector3RD tileRDCoordinatesBottomLeft = new Vector3RD(double.Parse(coordinates[0]), double.Parse(coordinates[1]), 0);

                    var assetBundleTile = AssetBundle.LoadFromFile(file.FullName);
					
					var mesh = assetBundleTile.LoadAllAssets<Mesh>().First();
					
					if(mesh.bounds.size == Vector3.zero)
                    {
						Debug.Log($"mesh bound is zero {file.Name}");
						currentFile++;
						continue;
                    }

					GameObject newTile = new GameObject();
					newTile.isStatic = true;
					newTile.name = file.Name;
					newTile.AddComponent<MeshFilter>().sharedMesh = mesh;
					newTile.AddComponent<MeshCollider>().sharedMesh = mesh;
					newTile.AddComponent<MeshRenderer>().material = previewMaterial;
                    newTile.GetComponent<MeshRenderer>().materials = _terrainMaterials.ToArray();

                    newTile.transform.position = CoordConvert.RDtoUnity(tileRDCoordinatesBottomLeft);

					GameObject treeRoot = new GameObject();
					treeRoot.name = file.Name.Replace("terrain", "trees");
					treeRoot.transform.position = newTile.transform.position;

					yield return new WaitForEndOfFrame(); //Make sure collider is processed
                   // yield return new WaitForSeconds(0.3f);


                    SpawnTreesInTile(treeRoot, tileRDCoordinatesBottomLeft);
				}
				currentFile++;
			}
		}

		/// <summary>
		/// Spawn all the trees located within the RD coordinate bounds of the 1x1km tile.
		/// </summary>
		/// <param name="treeTile">The target 1x1 km ground tile</param>
		/// <param name="tileCoordinates">RD Coordinates of the tile</param>
		/// <returns></returns>
		private void SpawnTreesInTile(GameObject treeTile, Vector3RD tileCoordinates)
		{
			//TODO: Add all trees within this tile (1x1km)
			int treeChecked = trees.Count -1;
			while (treeChecked >= 0)
			{
				Tree tree = trees[treeChecked];

				if (tree.RD.x > tileCoordinates.x && tree.RD.y > tileCoordinates.y && tree.RD.x < tileCoordinates.x + tileSize && tree.RD.y < tileCoordinates.y + tileSize)
				{
					SpawnTreeOnGround(treeTile, tree);
					trees.RemoveAt(treeChecked);
				}
				treeChecked--;
			}

			//Define a preview position to preview the tree tile in our scene
			Vector3 previewPosition = treeTile.transform.position + Vector3.down * Config.activeConfiguration.zeroGroundLevelY;
			treeTile.transform.position = unityTileOffset;

			CreateTreeTile(treeTile, previewPosition);
		}

		/// <summary>
		/// Spawn a new tree object matching the tree data properties.
		/// </summary>
		/// <param name="treeTile">The root parent for the new tree</param>
		/// <param name="tree">The tree data object containing our tree properties</param>
		private void SpawnTreeOnGround(GameObject treeTile, Tree tree)
		{
            Debug.Log("SpawnTreeOnGround");

			GameObject newTreeInstance = Instantiate(tree.prefab, treeTile.transform);

			//Apply properties/variations based on tree data
			newTreeInstance.name = tree.OBJECTNUMMER;
			newTreeInstance.transform.localScale = Vector3.one * 0.1f * tree.averageTreeHeight;
			newTreeInstance.transform.Rotate(0, UnityEngine.Random.value * 360.0f, 0);

            newTreeInstance.transform.position = new Vector3(tree.position.x, treeTile.transform.position.y, tree.position.z);
            newTreeInstance.transform.position+= new Vector3(-500,0,-500);


            float raycastHitY = treeTile.transform.position.y;
            //if (Physics.Raycast(tree.position + (Vector3.up * 1000.0f), Vector3.down, out RaycastHit hit))
            if (Physics.Raycast(newTreeInstance.transform.position + (Vector3.up * 1000.0f), Vector3.down, out RaycastHit hit))           
            {
                raycastHitY = hit.point.y;
                Debug.Log($"raycastHitY:{raycastHitY}");
            }
            else
            {
                //throw new Exception("no raycasthit");
                Debug.Log("no raycasthit");
            }

            //Add a little random variation in our hitpoint, to avoid z-fighting on the same trees that are intersecting with eachother
            raycastHitY -= UnityEngine.Random.value * raycastYRandomOffsetRange;

            newTreeInstance.transform.position = new Vector3(tree.position.x, raycastHitY, tree.position.z);
           
        }

		/// <summary>
		/// Get all the child meshes of the tile, and merge them into one big tile mesh.
		/// </summary>
		/// <param name="treeTile">The root parent containing all our spawned trees</param>
		/// <param name="worldPosition">The position to move the tile to when it is done (for previewing purposes)</param>
		private void CreateTreeTile(GameObject treeTile, Vector3 worldPosition)
		{
			string assetName = treeTileAssetsFolder + treeTile.name + ".asset";

            MeshFilter[] meshFilters = treeTile.GetComponentsInChildren<MeshFilter>();
			CombineInstance[] combine = new CombineInstance[meshFilters.Length];

			var totalVertexCount = 0;
			for (int i = 0; i < combine.Length; i++)
			{
				combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

				Mesh treeMesh = meshFilters[i].mesh;						
				if (treeMesh.vertexCount > 0)
				{
					totalVertexCount += treeMesh.vertexCount;
                    AddIDToMeshUV(treeMesh, int.Parse(meshFilters[i].name));
				}
				combine[i].mesh = treeMesh;
				meshFilters[i].gameObject.SetActive(false);
			}

			Mesh newCombinedMesh = new Mesh();
			if (totalVertexCount > 65536) //In case we go over the 16bit ( 2^16 ) index count, increase the indexformat.
				newCombinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			if (meshFilters.Length > 0)
			{
				newCombinedMesh.name = treeTile.name;
				newCombinedMesh.CombineMeshes(combine, true);
			}

			treeTile.AddComponent<MeshFilter>().sharedMesh = newCombinedMesh;
			treeTile.AddComponent<MeshRenderer>().material = treesMaterial;
#if UNITY_EDITOR
			AssetDatabase.CreateAsset(newCombinedMesh, assetName);
			AssetDatabase.SaveAssets();
#endif

			treeTile.transform.position = worldPosition;
		}

		/// <summary>
		/// Adds a specific number to a mesh UV slot for all the verts
		/// </summary>
		/// <param name="treeMesh">The mesh to assign the ID to</param>
		/// <param name="objectNumber">The number to inject into the UV slot</param>
		private void AddIDToMeshUV(Mesh treeMesh, float objectNumber)
		{
			treeMesh.uv3 = new Vector2[treeMesh.vertexCount];
			Vector2 uvIds = new Vector2() { x = objectNumber, y = 0 };
			for (int j = 0; j < treeMesh.uv3.Length; j++)
			{
				treeMesh.uv3[j] = uvIds;
			}
		}


        public LongitudeLatitude ConvertToLatLong(double x, double y)
        {
            // The city "Amsterfoort" is used as reference "Rijksdriehoek" coordinate.
            int referenceRdX = 155000;
            int referenceRdY = 463000;

            double dX = (double)(x - referenceRdX) * (double)(Math.Pow(10, -5));
            double dY = (double)(y - referenceRdY) * (double)(Math.Pow(10, -5));

            double sumN =
                (3235.65389 * dY) +
                (-32.58297 * Math.Pow(dX, 2)) +
                (-0.2475 * Math.Pow(dY, 2)) +
                (-0.84978 * Math.Pow(dX, 2) * dY) +
                (-0.0655 * Math.Pow(dY, 3)) +
                (-0.01709 * Math.Pow(dX, 2) * Math.Pow(dY, 2)) +
                (-0.00738 * dX) +
                (0.0053 * Math.Pow(dX, 4)) +
                (-0.00039 * Math.Pow(dX, 2) * Math.Pow(dY, 3)) +
                (0.00033 * Math.Pow(dX, 4) * dY) +
                (-0.00012 * dX * dY);
            double sumE =
                (5260.52916 * dX) +
                (105.94684 * dX * dY) +
                (2.45656 * dX * Math.Pow(dY, 2)) +
                (-0.81885 * Math.Pow(dX, 3)) +
                (0.05594 * dX * Math.Pow(dY, 3)) +
                (-0.05607 * Math.Pow(dX, 3) * dY) +
                (0.01199 * dY) +
                (-0.00256 * Math.Pow(dX, 3) * Math.Pow(dY, 2)) +
                (0.00128 * dX * Math.Pow(dY, 4)) +
                (0.00022 * Math.Pow(dY, 2)) +
                (-0.00022 * Math.Pow(dX, 2)) +
                (0.00026 * Math.Pow(dX, 5));


            // The city "Amsterfoort" is used as reference "WGS84" coordinate.
            double referenceWgs84X = 52.15517;
            double referenceWgs84Y = 5.387206;

            double latitude = referenceWgs84X + (sumN / 3600);
            double longitude = referenceWgs84Y + (sumE / 3600);

            return new LongitudeLatitude()
            {
                longitude = longitude,
                latitude = latitude
            };
            
        }
    }
}

