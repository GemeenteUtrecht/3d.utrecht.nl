using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoadTile : MonoBehaviour
{

    //private string _assetUrl = "http://srv1.lab4242.nl/3dutrecht/trees/trees_139000-453000";
    //string _assetUrl = "http://srv1.lab4242.nl/3dutrecht/trees/trees_126000-456000";

    //string _assetUrl = "http://localhost:8080/trees_126000-456000";
    string _assetUrl = "http://localhost:8080/utrecht";


    MeshFilter _meshFilter;
    

    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();

       // StartCoroutine(GetAsset());

        StartCoroutine(GetAssetBundle());



    }


    IEnumerator GetAsset()
    {
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(_assetUrl))
        {
            yield return uwr.SendWebRequest();

            Debug.Log(uwr.downloadProgress);

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);

                Debug.Log("got bundle");

            }
        }
    }


    IEnumerator GetAssetBundle()
    {
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(_assetUrl);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            var meshes = bundle.LoadAllAssets<Mesh>();

            _meshFilter.sharedMesh = meshes[0];


            //Debug.Log(tree.Length);

           // var newtree = Instantiate(tree);
            //newtree.name = "new tree";

            
        }
    }

}
