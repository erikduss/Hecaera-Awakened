using Erikduss;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;

namespace Erikduss
{
    public class UthanorWrathPillarLogic : SyncedObject
    {
        private float elapsedTime;

        public Vector3 targetLocation;

        protected override void Update()
        {
            base.Update();

            transform.position = Vector3.Lerp(transform.position, targetLocation, (elapsedTime / 2f));
            elapsedTime += Time.deltaTime;
        }

        protected override void ReturnThisObjectToPool()
        {
            base.ReturnThisObjectToPool();

            elapsedTime = 0;
        }
    }
}
