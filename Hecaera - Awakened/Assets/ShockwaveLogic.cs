using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class ShockwaveLogic : Projectile
    {
        private float initialShockwaveSpeed = 8f;
        private float addedShockwaveSpeed = 0f;
        private float addedSpeedRate = 15f;
        private bool moveShockwave = false;

        protected override void Update()
        {
            base.Update();

            if (NetworkManager.Singleton.IsServer)
            {
                if (moveShockwave)
                    MoveShockwave();
            }
        }

        private void MoveShockwave()
        {
            transform.position += (transform.forward * (initialShockwaveSpeed + addedShockwaveSpeed)) * Time.deltaTime;
            addedShockwaveSpeed = addedSpeedRate * Time.deltaTime;
        }

        private IEnumerator StartSpawnShockwave(float surfacingTime)
        {
            Vector3 startingPos = transform.position;
            Vector3 finalPos = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);

            float elapsedTime = 0;

            while (elapsedTime < surfacingTime)
            {
                transform.position = Vector3.Lerp(startingPos, finalPos, (elapsedTime / surfacingTime));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            moveShockwave = true;
        }

        protected override void OnObjectEnabledChange(bool oldID, bool newID)
        {
            base.OnObjectEnabledChange(oldID, newID);

            addedShockwaveSpeed = 0;

            //Set initial position below ground to give player a slight bit of time.
            if(newID)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y - 2, transform.position.z);

                    networkPosition.Value = transform.position;
                    networkRotation.Value = transform.rotation;

                    StartCoroutine(StartSpawnShockwave(0.5f));
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

