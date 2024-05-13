using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class WorldSyncedObjectsManager : MonoBehaviour
    {
        public static WorldSyncedObjectsManager Instance;

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

        private void SpawnSyncedObjectFromObjectPool(ulong clientID, int syncedObjectTypeID, Vector3 spawnLoc, Quaternion spawnRot)
        {
            PooledObjectType type = (PooledObjectType)syncedObjectTypeID;

            Vector3 spawnLocation = spawnLoc;
            Quaternion spawnRotation = spawnRot;

            GameObject prefab = WorldNetworkObjectPoolManager.Instance.GetGameObjectWithPoolType(type);
            NetworkObject obj = WorldNetworkObjectPoolManager.Instance.GetNetworkObject(prefab, spawnLocation, spawnRotation);
        }

        private IEnumerator SpawnSyncedObjectWithDelay(ulong clientID, int indicatorObjectTypeID, float spawnDelay, Vector3 spawnLocation, Quaternion spawnRotation)
        {
            //This is used to make sure the indicator spawns exactly when we want it to.
            //Could change this into a synced animation event.
            yield return new WaitForSeconds(spawnDelay);

            SpawnSyncedObjectFromObjectPool(clientID, indicatorObjectTypeID, spawnLocation, spawnRotation);
            NotifyTheServerOfSpawnActionClientRpc(clientID, indicatorObjectTypeID, spawnRotation);
        }


        [ServerRpc]
        public void NotifyTheServerOfSpawnActionServerRpc(ulong clientID, int syncedObjectTypeID, float spawnDelay, Vector3 spawnLocation, Quaternion spawnRotation)
        {
            //can only be called by the server.
            if (NetworkManager.Singleton.IsServer)
            {
                StartCoroutine(SpawnSyncedObjectWithDelay(clientID, syncedObjectTypeID, spawnDelay, spawnLocation, spawnRotation));
            }
        }

        [ClientRpc]
        private void NotifyTheServerOfSpawnActionClientRpc(ulong clientID, int projectileObjectTypeID, quaternion lastRotation)
        {
            //we do not want to play the action again for the character that called the action
            if (clientID != NetworkManager.Singleton.LocalClientId)
            {
                //SyncProjectileSpawn(clientID, projectileObjectTypeID);
            }
        }
    }
}
