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

        public override void Select()
        {
            ClickAction.Invoke(Index);           
        }

        void LateUpdate()
        {
            var lookPos = Camera.main.transform.position - transform.position;
            lookPos.y = 0;
            transform.rotation = Quaternion.LookRotation(lookPos);

        }
    }

}