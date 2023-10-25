using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public NetworkVariable<bool> objectEnabled = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public Vector3 networkPositionVelocity;

    public ProjectileType projectileType;

    public Transform startingPoint;

    public override void OnNetworkSpawn()
    {
        //if (!NetworkManager.Singleton.IsServer)
        //    return;

        base.OnNetworkSpawn();

        if (!NetworkManager.Singleton.IsServer)
        {
            if(transform.position != networkPosition.Value)
            {
                transform.position = networkPosition.Value;
            }

            if(transform.rotation != networkRotation.Value)
            {
                transform.rotation = networkRotation.Value;
            }

            //if we're active but the network variable says we shouldnt be active. Disable.
            if(gameObject.activeSelf && !objectEnabled.Value)
            {
                gameObject.SetActive(false);
            }

            objectEnabled.OnValueChanged += OnObjectEnabledChange;

        }
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
        gameObject.SetActive(newID);
    }
}
