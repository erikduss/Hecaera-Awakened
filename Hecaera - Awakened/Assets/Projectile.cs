using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public ProjectileType projectileType;

    public Transform startingPoint;

    public override void OnNetworkSpawn()
    {
        Debug.Log("Spawned");

        if (!NetworkManager.Singleton.IsServer)
            return;

        base.OnNetworkSpawn();
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
}
