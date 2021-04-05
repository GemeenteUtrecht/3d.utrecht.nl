using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTest : MonoBehaviour
{
    public GameObject SpawnCube;


    void Start()
    {
        var kube = Instantiate(SpawnCube, transform);
        kube.transform.position = new Vector3(0, 46.5f, 0);
    }

    
}
