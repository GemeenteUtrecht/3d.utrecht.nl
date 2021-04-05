using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerBall : MonoBehaviour
{
    public GameObject pointer;
    public Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }


    void Update()
    {
        RaycastHit hit;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            pointer.transform.position = hit.point;
        }
    }
}
