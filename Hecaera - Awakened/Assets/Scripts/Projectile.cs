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
            if(Vector3.Distance(transform.position, networkPosition.Value) > 1f)
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
                if(activeTime <= 0 && startedTimer)
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

        if (newID)
            projectileCollider.EnableDamageCollider();
        else
            projectileCollider.DisableDamageCollider();

        if (!NetworkManager.Singleton.IsServer)
            return;
    }
}
