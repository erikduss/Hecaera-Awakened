using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class WorldGroundIndicatorManager : MonoBehaviour
    {
        public static WorldGroundIndicatorManager Instance;

        //public List<Projectile> projectiles = new List<Projectile>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void SpawnGroundIndicatorFromObjectPool(ulong clientID, int indicatorObjectTypeID, Vector3 spawnLoc, Quaternion spawnRot, float indicatorSize, bool isNPC, Projectile attachedProjectile)
        {
            PooledObjectType type = (PooledObjectType)indicatorObjectTypeID;

            Vector3 spawnLocation = spawnLoc;
            Quaternion spawnRotation = spawnRot;
            PlayerManager indicatorOwner = null;

            if (!isNPC)
            {
                indicatorOwner = WorldGameSessionManager.Instance.players.Where(a => a.OwnerClientId == clientID).FirstOrDefault();
                spawnLocation = indicatorOwner.characterCombatManager.magicHandTransform.position;
                spawnRotation = indicatorOwner.transform.rotation;
            }

            GameObject prefab = WorldNetworkObjectPoolManager.Instance.GetGameObjectWithPoolType(type);
            NetworkObject obj = WorldNetworkObjectPoolManager.Instance.GetNetworkObject(prefab, spawnLocation, spawnRotation);

            CircleGroundIndicator indicator = obj.GetComponent<CircleGroundIndicator>();

            if(indicator != null)
            {
                SphereIndicatorDamageCollider collider = indicator.GetComponentInChildren<SphereIndicatorDamageCollider>();

                collider.DisableDamageCollider();

                indicator.StartFadeingIndicator(indicatorSize, attachedProjectile);
            }
        }

        private IEnumerator SpawnIndicatorWithDelay(ulong clientID, int indicatorObjectTypeID, float spawnDelay, Vector3 spawnLocation, Quaternion spawnRotation, float indicatorSize, bool isNPC, Projectile attachedProjectile)
        {
            //This is used to make sure the indicator spawns exactly when we want it to.
            //Could change this into a synced animation event.
            yield return new WaitForSeconds(spawnDelay);

            SpawnGroundIndicatorFromObjectPool(clientID, indicatorObjectTypeID, spawnLocation, spawnRotation, indicatorSize, isNPC, attachedProjectile);
            NotifyTheServerOfSpawnActionClientRpc(clientID, indicatorObjectTypeID);
        }

        [ServerRpc]
        public void NotifyTheServerOfSpawnActionServerRpc(ulong clientID, int indicatorObjectTypeID, float spawnDelay, Vector3 spawnLocation, Quaternion spawnRotation, float indicatorSize, Projectile attachedProjectile, bool isNPC = false)
        {
            //can only be called by the server.
            if (NetworkManager.Singleton.IsServer)
            {
                StartCoroutine(SpawnIndicatorWithDelay(clientID, indicatorObjectTypeID, spawnDelay, spawnLocation, spawnRotation, indicatorSize, isNPC, attachedProjectile));
            }
        }

        [ClientRpc]
        private void NotifyTheServerOfSpawnActionClientRpc(ulong clientID, int projectileObjectTypeID)
        {
            //we do not want to play the action again for the character that called the action
            if (clientID != NetworkManager.Singleton.LocalClientId)
            {
                //SyncProjectileSpawn(clientID, projectileObjectTypeID);
            }
        }
    }
}
