using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class Projectile : NetworkBehaviour
    {
        public NetworkVariable<bool> objectEnabled = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<ulong> projectileOwnerNetworkID = new NetworkVariable<ulong>(ulong.MinValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public Vector3 networkPositionVelocity;
        public float networkPositionSmoothTime = 0.1f;
        public float networkRotationSmoothTime = 0.1f;

        public PooledObjectType projectileType;

        public Transform startingPoint;

        private float activeTime = 0f;
        private bool startedTimer = false;
        public float maxActiveTime = 5f;

        private NetworkObject netObj;
        protected Rigidbody rb;
        public ProjectileDamageCollider projectileCollider;

        private void Start()
        {
            netObj = GetComponent<NetworkObject>();
            rb = GetComponent<Rigidbody>();
            projectileCollider = GetComponent<ProjectileDamageCollider>();
        }

        protected virtual void Update()
        {
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

            if (NetworkManager.Singleton.IsServer)
            {
                if (objectEnabled.Value)
                {
                    if (activeTime <= 0 && startedTimer)
                    {
                        ReturnThisProjectileToPool();
                    }
                    else if (!startedTimer)
                    {
                        StartReturnTimer();
                    }
                    else
                    {
                        activeTime -= Time.deltaTime;
                    }
                }
            }
        }

        public void StartReturnTimer()
        {
            activeTime = maxActiveTime;
            startedTimer = true;
            projectileCollider.EnableDamageCollider();
        }

        public void ReturnThisProjectileToPool()
        {
            projectileCollider.DisableDamageCollider();
            ResetProjectileOwner();
            startedTimer = false;
            objectEnabled.Value = false;
            WorldNetworkObjectPoolManager.Instance.m_PooledObjects[WorldNetworkObjectPoolManager.Instance.GetGameObjectWithPoolType(projectileType)].Release(netObj);
        }

        public override void OnNetworkSpawn()
        {
            //if (!NetworkManager.Singleton.IsServer)
            //    return;

            base.OnNetworkSpawn();

            if (!NetworkManager.Singleton.IsServer)
            {
                if (transform.position != networkPosition.Value)
                {
                    transform.position = networkPosition.Value;
                }

                if (transform.rotation != networkRotation.Value)
                {
                    transform.rotation = networkRotation.Value;
                }

                //if we're active but the network variable says we shouldnt be active. Disable.
                if (gameObject.activeSelf && !objectEnabled.Value)
                {
                    gameObject.SetActive(false);
                }

                objectEnabled.OnValueChanged += OnObjectEnabledChange;
            }

            projectileOwnerNetworkID.OnValueChanged += OnProjectileOwnerNetworkIDChange; //should be done on both the server and the client.
        }

        public override void OnNetworkDespawn()
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            base.OnNetworkDespawn();
        }

        public override void OnDestroy()
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            base.OnDestroy();
        }

        public void OnObjectEnabledChange(bool oldID, bool newID)
        {
            if (newID)
                projectileCollider.EnableDamageCollider();
            else
            {
                projectileCollider.DisableDamageCollider();
                ResetProjectileOwner();
            }

            gameObject.SetActive(newID);

            if (!NetworkManager.Singleton.IsServer)
                return;
        }

        private void ResetProjectileOwner()
        {
            //We need to reset both the network variable and the owner assigned to this projectile when we return it to the pool.

            //NOTE: We are using the NetworkObjectID instead of the NetworkBehaviourID because the networkbehaviourID can be 0
            //When this is 0 it doesnt recognize the id change which gives an error due to it not setting the projectile owner this way.

            projectileCollider.characterCausingDamage = null;

            if (NetworkManager.Singleton.IsServer)
                projectileOwnerNetworkID.Value = 0;
        }

        public void OnProjectileOwnerNetworkIDChange(ulong oldID, ulong newID)
        {
            PlayerManager projectileOwner = WorldGameSessionManager.Instance.GetPlayerWithNetworkID(newID);

            //If the player is not found, the projectile must be fired by an NPC

            if (projectileOwner == null) return;

            projectileCollider.characterCausingDamage = projectileOwner;
        }
    }
}
