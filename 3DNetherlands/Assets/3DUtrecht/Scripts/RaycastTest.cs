using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour
{

    RaycastHit hit;


    void Start()
    {
        
    }

    
    void Update()
    {

        //if(Input.GetKeyDown(KeyCode.R))
        //{
        //    float raycastHitY = 0;

        //    if (Physics.Raycast(transform.up * 100.0f, -transform.up, out hit, Mathf.Infinity))
        //    //if (Physics.SphereCast(tree.position + Vector3.up * 1000.0f, 100, Vector3.down, out RaycastHit hit, Mathf.Infinity))
        //    {
        //        raycastHitY = hit.point.y;
        //        Debug.Log($"raycastHitY:{raycastHitY}");
        //    }
        //    else
        //    {                
        //        Debug.Log("behhh... no raycasthit");
        //    }

        //}


        if (Physics.Raycast(transform.position + (transform.up * 1000), -transform.up, out hit))
        //if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            Debug.Log($"hit:{hit.point}");
        }



    }
}
