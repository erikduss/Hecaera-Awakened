using Erikduss;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Erikduss
{
    public class AvoidThisProjectileLogic : Projectile
    {
        [SerializeField] private int amountOfTimesThisMoves = 5;
        [SerializeField] private int amountOfTimesMoved = 0;

        [SerializeField] private float moveDelay = 2.5f;
        [SerializeField] private float moveDelayTimer = 0;

        [SerializeField] private float initialProjectileSpeed = 15f;
        [SerializeField] private float addedProjectileSpeed = 0f;
        [SerializeField] private float addedSpeedRate = 25f;

        [SerializeField] private Vector3 currentEndLocation = Vector3.zero;
        [SerializeField] private bool reachedEndLocation = false;

        [SerializeField] private bool pickedNewLocation = false;

        protected override void Update()
        {
            #region Network Position and rotation syncing
            //Projectiles are being handles by the server only!, assign its network position to the position of our transform.
            if (NetworkManager.Singleton.IsServer)
            {
                networkPosition.Value = transform.position;
                networkRotation.Value = transform.rotation;
            }
            //if the character is being controlled from elsewhere, then assign its position here locally.
            else
            {
                //Instantly move the transform is its far away (happens while initiating from the object pool)
                if (Vector3.Distance(transform.position, networkPosition.Value) > 1f)
                {
                    transform.position = networkPosition.Value;
                }

                //Position
                transform.position = Vector3.SmoothDamp
                    (transform.position,
                    networkPosition.Value,
                    ref networkPositionVelocity,
                    networkPositionSmoothTime);

                //Rotation
                transform.rotation = Quaternion.Slerp
                    (transform.rotation,
                    networkRotation.Value,
                    networkPositionSmoothTime);
            }
            #endregion


            if (!NetworkManager.Singleton.IsServer) return;

            //we picked a new location, we show a visual indicator of where we go, but we wait for the delay.
            if (pickedNewLocation)
            {
                if (moveDelayTimer <= 0f)
                {
                    //Delay is over, now we start moving.
                    reachedEndLocation = false;
                    pickedNewLocation = false;
                }
                else
                {
                    moveDelayTimer -= Time.deltaTime;
                    return;
                }
            }

            if(reachedEndLocation)
            {
                PickNewEndLocation();
            }

            if (Vector3.Distance(transform.position, currentEndLocation) < .5f)
            {
                if(!reachedEndLocation)
                {
                    reachedEndLocation = true;
                }
            }

            if (currentEndLocation != Vector3.zero)
            {
                MoveProjectile();
            }
        }

        private void MoveProjectile()
        {
            transform.position = Vector3.Lerp(transform.position, currentEndLocation, .6f * Time.deltaTime);

            //transform.position += (transform.forward * (initialProjectileSpeed + addedProjectileSpeed)) * Time.deltaTime;
            //addedProjectileSpeed = addedSpeedRate * Time.deltaTime;
        }

        protected override void OnObjectEnabledChange(bool oldID, bool newID)
        {
            base.OnObjectEnabledChange(oldID, newID);

            PickNewEndLocation();
        }

        private void PickNewEndLocation()
        {
            if (!IsServer) return;

            currentEndLocation = Vector3.zero;
            moveDelayTimer = 0;
            addedProjectileSpeed = 0;

            if (amountOfTimesMoved >= amountOfTimesThisMoves)
            {
                amountOfTimesMoved = 0;
                projectileCollider.charactersDamaged.Clear();

                ReturnThisProjectileToPool();
                return;
            }

            //we reached the final location, we need to pick a new location since we still have more time that we need to move.
            Vector3 referenceEndPosition = WorldAIManager.Instance.spawnedInBosses.FirstOrDefault().transform.position;

            Vector3 relativePos = referenceEndPosition - transform.position;

            Quaternion newRotation = Quaternion.LookRotation(relativePos);
            float rand = UnityEngine.Random.Range(-30f, 30f);
            Quaternion fixedRotation = newRotation * Quaternion.Euler(0, rand, 0);

           // Quaternion fixedRotation = newRotation;

            transform.rotation = fixedRotation;

            currentEndLocation = transform.position + transform.forward * 30f;

            Vector3 indicatorLocation = transform.position;
            WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.RectangleDamageIndicator, 0, indicatorLocation, fixedRotation, 30, null, true, false, 0, 0, moveDelay - 1.5f);

            pickedNewLocation = true;
            moveDelayTimer = moveDelay;
            projectileCollider.charactersDamaged.Clear(); //every time this attacks, we can hit everyone again.
            amountOfTimesMoved++;
        }
    }
}
