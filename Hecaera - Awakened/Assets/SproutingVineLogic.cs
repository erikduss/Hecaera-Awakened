using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class SproutingVineLogic : Projectile
    {
        private float initialVineSpeed = 8f;
        private float addedVineSpeed = 0f;
        private float addedSpeedRate = 15f;
        private bool moveVine = false;

        protected override void Update()
        {
            base.Update();

            if (NetworkManager.Singleton.IsServer)
            {
                //if (moveVine)
                //    MoveShockwave();
            }
        }

        public IEnumerator SpawnVine(float spawnDelay)
        {
            yield return new WaitForSeconds(spawnDelay);
            float elapsedTime = 0;
            float duration = 0.2f;
            Vector3 startpos = transform.position;
            Vector3 spawnLocation = transform.position;
            spawnLocation.y = transform.position.y + 5.5f;

            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(startpos, spawnLocation, (elapsedTime / duration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(.33f);

            spawnLocation.y = transform.position.y - 5.5f; //end location
            elapsedTime = 0;
            duration = 0.2f;
            startpos = transform.position;

            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(startpos, spawnLocation, (elapsedTime / duration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(.5f);
        }

        protected override void OnObjectEnabledChange(bool oldID, bool newID)
        {
            base.OnObjectEnabledChange(oldID, newID);

            addedVineSpeed = 0;

            //Set initial position below ground to give player a slight bit of time.
            if (newID)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    networkPosition.Value = transform.position;
                    networkRotation.Value = transform.rotation;

                    StartCoroutine(SpawnVine(1.5f));
                }
                else
                {
                    transform.position = networkPosition.Value;
                    transform.rotation = networkRotation.Value;
                }
            }
        }
    }
}
