using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class WorldProjectilesManager : MonoBehaviour
{
    public static WorldProjectilesManager Instance;

    public List<Projectile> projectiles = new List<Projectile>();

    private void Awake()
    {
        if(Instance == null)
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

    private void SpawnProjectileFromObjectPool(ulong clientID, int projectileObjectTypeID)
    {
        PooledObjectType type = (PooledObjectType)projectileObjectTypeID;
        GameObject projectileOwner = WorldGameSessionManager.Instance.players.Where(a => a.OwnerClientId == clientID).FirstOrDefault().gameObject;
        Vector3 spawnLocation = projectileOwner.transform.position; //should probably be changed to player's hand location.
        Quaternion spawnRotation = projectileOwner.transform.rotation;

        GameObject prefab = WorldNetworkObjectPoolManager.Instance.GetGameObjectWithPoolType(type);
        NetworkObject obj = WorldNetworkObjectPoolManager.Instance.GetNetworkObject(prefab, spawnLocation, spawnRotation);

        Projectile spawnedProjectile = obj.GetComponent<Projectile>();
        if (spawnedProjectile != null)
        {
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

    [ServerRpc]
    public void NotifyTheServerOfSpawnActionServerRpc(ulong clientID, int projectileObjectTypeID)
    {
        //can only be called by the server.
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Spawn projectile!");

            SpawnProjectileFromObjectPool(clientID, projectileObjectTypeID);
            NotifyTheServerOfSpawnActionClientRpc(clientID, projectileObjectTypeID);
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
