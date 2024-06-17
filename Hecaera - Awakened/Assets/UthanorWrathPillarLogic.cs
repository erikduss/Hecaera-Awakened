using Erikduss;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class UthanorWrathPillarLogic : SyncedObject
    {
        private float elapsedTime;

        public Vector3 targetLocation;

        protected override void Update()
        {
            base.Update();

            if (!IsServer) return;

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
