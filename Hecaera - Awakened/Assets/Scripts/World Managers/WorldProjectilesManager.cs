using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class WorldProjectilesManager : MonoBehaviour
    {
        public static WorldProjectilesManager Instance;

        public List<Projectile> projectiles = new List<Projectile>();

        [SerializeField] private float fireFruitIndicatorRadius = 3f;
        [SerializeField] private float sproutingVineIndicatorRadius = 3f;

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

        //Called through animation event.
        public void SpawnProjectile(int type)
        {
            //ProjectileType fixedType = (ProjectileType)type;

            //if (projectiles.Count == 0) return;

            //var projectileToSpawn = projectiles.Where(a => a.projectileType == fixedType).FirstOrDefault();

            //if (projectileToSpawn != null)
            //{
            //    if (NetworkManager.Singleton.IsServer)
            //    {

            //    }
            //    //var spawnedProjectile = Instantiate(projectileToSpawn.gameObject, Vector3.zero, Quaternion.identity);
            //}
        }

        private void SpawnProjectileFromObjectPool(ulong clientID, int projectileObjectTypeID, Vector3 spawnLoc, Quaternion spawnRot, bool isNPC)
        {
            PooledObjectType type = (PooledObjectType)projectileObjectTypeID;

            Vector3 spawnLocation = spawnLoc;
            Quaternion spawnRotation = spawnRot;
            PlayerManager projectileOwner = null;

            if (!isNPC)
            {
                projectileOwner = WorldGameSessionManager.Instance.players.Where(a => a.OwnerClientId == clientID).FirstOrDefault();
                spawnLocation = projectileOwner.characterCombatManager.magicHandTransform.position;
                spawnRotation = projectileOwner.transform.rotation;
            }

            GameObject prefab = WorldNetworkObjectPoolManager.Instance.GetGameObjectWithPoolType(type);
            NetworkObject obj = WorldNetworkObjectPoolManager.Instance.GetNetworkObject(prefab, spawnLocation, spawnRotation);

            Projectile spawnedProjectile = obj.GetComponent<Projectile>();
            if (spawnedProjectile != null)
            {
                if (!isNPC)
                {
                    spawnedProjectile.projectileOwnerNetworkID.Value = projectileOwner.NetworkObjectId;
                    spawnedProjectile.projectileCollider.groupOfAttack = projectileOwner.characterGroup;
                }
                else
                {
                    spawnedProjectile.projectileOwnerNetworkID.Value = clientID; //on spawning with isNPC this gets set to the npc id instead of client

                    if(type == PooledObjectType.FireFruit)
                    {
                        Vector3 indicatorLocation = new Vector3(spawnLocation.x, 5f, spawnLocation.z);
                        WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(clientID, (int)PooledObjectType.DamageIndicator, 0, indicatorLocation, Quaternion.identity, fireFruitIndicatorRadius, spawnedProjectile, true);
                    }
                    else if(type == PooledObjectType.SproutingVine)
                    {
                        Vector3 indicatorLocation = new Vector3(spawnLocation.x, spawnLocation.y + 5.5f, spawnLocation.z);
                        WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(clientID, (int)PooledObjectType.DamageIndicator, 0, indicatorLocation, Quaternion.identity, sproutingVineIndicatorRadius, spawnedProjectile, true);
                    }
                }
                //spawnedProjectile.projectileCollider.characterCausingDamage = projectileOwner;
                spawnedProjectile.objectEnabled.Value = true;
                spawnedProjectile.networkPosition.Value = spawnLocation;
                spawnedProjectile.networkRotation.Value = spawnRotation;
            }
        }

        private void SyncProjectileSpawn(ulong clientID, int projectileObjectTypeID)
        {
            //WeaponItemAction weaponAction = WorldActionManager.Instance.GetWeaponItemActionByID(actionID);

            //if (weaponAction != null)
            //{
            //    weaponAction.AttemptToPerformAction(player, WorldItemDatabase.Instance.GetWeaponByID(weaponID));
            //}
            //else
            //{
            //    Debug.LogError("WEAPON ACTION ID OUT OF BOUNDS, CANNOT PERFORM ACTION " + actionID + " OF WEAPON " + weaponID);
            //}
        }

        private IEnumerator SpawnProjectileWithDelay(ulong clientID, int projectileObjectTypeID, float spawnDelay, Vector3 spawnLocation, Quaternion spawnRotation, bool isNPC)
        {
            //This is used to make sure the projectile spawns exactly when we want it to.
            //Could change this into a synced animation event.
            yield return new WaitForSeconds(spawnDelay);

            SpawnProjectileFromObjectPool(clientID, projectileObjectTypeID, spawnLocation, spawnRotation, isNPC);
            NotifyTheServerOfSpawnActionClientRpc(clientID, projectileObjectTypeID);
        }

        [ServerRpc]
        public void NotifyTheServerOfSpawnActionServerRpc(ulong clientID, int projectileObjectTypeID, float spawnDelay, Vector3 spawnLocation, Quaternion spawnRotation, bool isNPC = false)
        {
            //can only be called by the server.
            if (NetworkManager.Singleton.IsServer)
            {
                StartCoroutine(SpawnProjectileWithDelay(clientID, projectileObjectTypeID, spawnDelay, spawnLocation, spawnRotation, isNPC));
            }
        }

        [ClientRpc]
        private void NotifyTheServerOfSpawnActionClientRpc(ulong clientID, int projectileObjectTypeID)
        {
            //we do not want to play the action again for the character that called the action
            if (clientID != NetworkManager.Singleton.LocalClientId)
            {
                SyncProjectileSpawn(clientID, projectileObjectTypeID);
            }
        }
    }
}
