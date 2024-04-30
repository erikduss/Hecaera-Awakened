using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class WorldGroundIndicatorManager : MonoBehaviour
    {
        public static WorldGroundIndicatorManager Instance;

        [SerializeField] private GameObject getOutRocksPrefab;
        [SerializeField] private GameObject vinesPrefab;
        [SerializeField] private GameObject lightEmbracePrefab;

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

        private void SpawnGroundIndicatorFromObjectPool(ulong clientID, int indicatorObjectTypeID, Vector3 spawnLoc, Quaternion spawnRot, float indicatorSize, bool isNPC, Projectile attachedProjectile, bool enableDamageCollider, float damageColliderEnableDelay, float colliderActiveTime)
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
            Vector3 eulerAngles = spawnRotation.eulerAngles;
            eulerAngles.x = 90; //Always rotate the projector down towards the ground.
            spawnRotation.eulerAngles = eulerAngles; //Quaternion.Euler(90,spawnRotation.y,spawnRotation.z);
            NetworkObject obj = WorldNetworkObjectPoolManager.Instance.GetNetworkObject(prefab, spawnLocation, spawnRotation);

            GroundIndicator indicator = obj.GetComponent<GroundIndicator>();

            if(indicator != null)
            {
                if(indicator.damageCollider == null) indicator.damageCollider = indicator.GetComponentInChildren<DamageCollider>();

                indicator.damageCollider.groupOfAttack = CharacterGroup.Team02;
                indicator.damageCollider.DisableDamageCollider();

                indicator.StartFadeingIndicator(indicatorSize, attachedProjectile, enableDamageCollider, damageColliderEnableDelay, colliderActiveTime);
            }
        }

        private IEnumerator SpawnIndicatorWithDelay(ulong clientID, int indicatorObjectTypeID, float spawnDelay, Vector3 spawnLocation, Quaternion spawnRotation, float indicatorSize, bool isNPC, Projectile attachedProjectile, bool enableDamageCollider, float damageColliderEnableDelay, float colliderActiveTime)
        {
            //This is used to make sure the indicator spawns exactly when we want it to.
            //Could change this into a synced animation event.
            yield return new WaitForSeconds(spawnDelay);

            SpawnGroundIndicatorFromObjectPool(clientID, indicatorObjectTypeID, spawnLocation, spawnRotation, indicatorSize, isNPC, attachedProjectile, enableDamageCollider, damageColliderEnableDelay, colliderActiveTime);
            NotifyTheServerOfSpawnActionClientRpc(clientID, indicatorObjectTypeID, spawnRotation);
        }

        public IEnumerator SpawnGetOutRocks(float spawnDelay, Vector3 spawnLocation)
        {
            spawnLocation.y = 0;
            GameObject spawnedPrefab = Instantiate(getOutRocksPrefab, spawnLocation, Quaternion.identity);
            yield return new WaitForSeconds(spawnDelay);
            spawnLocation.y = 3.4f; //end location
            float elapsedTime = 0;
            float duration = 0.2f;
            Vector3 startpos = spawnedPrefab.transform.position;

            while (elapsedTime < duration)
            {
                spawnedPrefab.transform.position = Vector3.Lerp(startpos, spawnLocation, (elapsedTime / duration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(1f);

            spawnLocation.y = 0f; //end location
            elapsedTime = 0;
            duration = 0.2f;
            startpos = spawnedPrefab.transform.position;

            while (elapsedTime < duration)
            {
                spawnedPrefab.transform.position = Vector3.Lerp(startpos, spawnLocation, (elapsedTime / duration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(.5f);

            Destroy(spawnedPrefab);
        }

        public IEnumerator SpawnVines(float spawnDelay, Vector3 spawnLocation)
        {
            spawnLocation.y = -13f;
            GameObject spawnedPrefab = Instantiate(vinesPrefab, spawnLocation, Quaternion.identity);
            yield return new WaitForSeconds(spawnDelay);
            spawnLocation.y = 3.4f; //end location
            float elapsedTime = 0;
            float duration = 0.2f;
            Vector3 startpos = spawnedPrefab.transform.position;

            while (elapsedTime < duration)
            {
                spawnedPrefab.transform.position = Vector3.Lerp(startpos, spawnLocation, (elapsedTime / duration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(1f);

            spawnLocation.y = -13f; //end location
            elapsedTime = 0;
            duration = 0.2f;
            startpos = spawnedPrefab.transform.position;

            while (elapsedTime < duration)
            {
                spawnedPrefab.transform.position = Vector3.Lerp(startpos, spawnLocation, (elapsedTime / duration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(.5f);

            Destroy(spawnedPrefab);
        }

        public IEnumerator SpawnLightEmbraceVisual(float spawnDelay, Vector3 spawnLocation, quaternion spawnRotation)
        {
            yield return new WaitForSeconds(spawnDelay);
            GameObject spawnedPrefab = Instantiate(lightEmbracePrefab, spawnLocation, spawnRotation);
            yield return new WaitForSeconds(1.5f);
            Destroy(spawnedPrefab);
        }

        [ServerRpc]
        public void NotifyTheServerOfSpawnActionServerRpc(ulong clientID, int indicatorObjectTypeID, float spawnDelay, Vector3 spawnLocation, Quaternion spawnRotation, float indicatorSize, Projectile attachedProjectile, bool isNPC = false, bool enableDamageCollider = false, float damageColliderEnableDelay = 0f, float colliderActiveTime = 3f)
        {
            //can only be called by the server.
            if (NetworkManager.Singleton.IsServer)
            {
                StartCoroutine(SpawnIndicatorWithDelay(clientID, indicatorObjectTypeID, spawnDelay, spawnLocation, spawnRotation, indicatorSize, isNPC, attachedProjectile, enableDamageCollider, damageColliderEnableDelay, colliderActiveTime));
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
