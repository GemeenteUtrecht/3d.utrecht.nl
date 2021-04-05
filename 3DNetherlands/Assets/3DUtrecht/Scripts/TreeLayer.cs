using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.LayerSystem;
using System;
using UnityEngine.Networking;
using ConvertCoordinates;
using System.Linq;
using Netherlands3D;
using System.IO;
using SimpleJSON;

public class TreeLayer : Layer
{
    [SerializeField]
    private Material _material;

    [SerializeField]
    private Vector3 _offset;

    [SerializeField]
    private float _scale = 1;

    [SerializeField]
    private string _replaceString;

    Dictionary<string, GameObject> _tiles = new Dictionary<string, GameObject>();

    public override void HandleTile(TileChange tileChange, Action<TileChange> callback = null)
    {
        switch (tileChange.action)
        {
            case TileAction.Create:
                if( !tiles.ContainsKey(new Vector2Int(tileChange.X, tileChange.Y)) )
                {
                    Tile newTile = new Tile();
                    tiles.Add(new Vector2Int(tileChange.X, tileChange.Y), newTile);

                    StartCoroutine(GetAssetFromWebserver(tileChange, callback));
                }
                break;
            case TileAction.Upgrade:
                break;
            case TileAction.Downgrade:
                break;
            case TileAction.Remove:
                tiles.Remove(new Vector2Int(tileChange.X, tileChange.Y));
                callback(tileChange);
                break;
            default:
                callback(tileChange);
                break;
        }

    }

    IEnumerator GetAssetFromWebserver(TileChange tileChange, System.Action<TileChange> callback = null)
    {
        var x = tileChange.X;
        var y = tileChange.Y;

        var name = _replaceString.Replace("{x}", x.ToString()).Replace("{y}", y.ToString());

        if (_tiles.ContainsKey(name) == false)
        {
            Uri baseUri = new Uri(Config.activeConfiguration.webserverRootPath);
            var uri = new Uri(baseUri, name);
            var tilepos = CoordConvert.RDtoUnity(new Vector3(x, y, 0));
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(uri.AbsoluteUri))
            {
                yield return uwr.SendWebRequest();

                if (!uwr.isNetworkError && !uwr.isHttpError)
                {
                    AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(uwr);

                    // yield return new WaitUntil(() => pauseLoading == false);

                    var mesh = assetBundle.LoadAllAssets<Mesh>().First();
                    GameObject gam = new GameObject();
                    gam.transform.localScale = Vector3.one * _scale;
                    gam.name = name;
                    gam.transform.parent = transform;
                    gam.transform.position = tilepos + _offset;
                    gam.AddComponent<MeshFilter>().sharedMesh = mesh;
                    gam.AddComponent<MeshRenderer>().material = _material;

                    callback(tileChange);

                    _tiles.Add(name, gam);
                }
            }
        }
        callback(tileChange);
    }

    public override void OnDisableTiles(bool isenabled)
    {        
    }

    
}
