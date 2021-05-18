using Netherlands3D.InputHandler;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D
{
    

    public class Billboard : Interactable
    {
        public int Index;
        public Action<int> ClickAction;

        GameObject textmeshGameObject;

        private void Start()
        {
            textmeshGameObject = GetComponentInChildren<TextMesh>().gameObject;
        }

        public override void Select()
        {
            ClickAction.Invoke(Index);           
        }

        void LateUpdate()
        {
            var lookPos = Camera.main.transform.position - transform.position;

            var distance = Vector3.Distance(Camera.main.transform.position, transform.position);
            
            textmeshGameObject.gameObject.SetActive(distance < 1200);

            lookPos.y = 0;
            transform.rotation = Quaternion.LookRotation(lookPos);

        }
    }

}