using FIMSpace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

public class WorldNetworkObjectPoolManager : NetworkBehaviour
{
    public static WorldNetworkObjectPoolManager Instance { get; private set; }

    [SerializeField]
    List<PoolConfigObject> PooledPrefabsList; //List of all pooled prefabs & the amount that need to be spawned.

    public HashSet<GameObject> m_Prefabs = new HashSet<GameObject>();

    public Dictionary<GameObject, ObjectPool<NetworkObject>> m_PooledObjects = new Dictionary<GameObject, ObjectPool<NetworkObject>>();

    [SerializeField] private GameObject ObjectPoolParentObjectPrefab;
    private GameObject objectPoolParentObject;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("DESTROY WORLD MANAGER");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        //if (objectPoolParentObject == null)
        //{
        //    objectPoolParentObject = GameObject.Instantiate(ObjectPoolParentObjectPrefab);
        //    DontDestroyOnLoad(objectPoolParentObject);
        //}
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public void SetUpObjectPool()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        //if (objectPoolParentObject == null)
        //{
        //    objectPoolParentObject = GameObject.Instantiate(ObjectPoolParentObjectPrefab);
        //    DontDestroyOnLoad(objectPoolParentObject);
        //}

        //var poolNetObj = objectPoolParentObject.GetComponent<NetworkObject>();
        //if (poolNetObj != null)
        //{
        //    if (!poolNetObj.IsSpawned)
        //        poolNetObj.Spawn();
        //}

        // Registers all objects in PooledPrefabsList to the cache.
        foreach (var configObject in PooledPrefabsList)
        {
            Debug.Log("Spawning spells!");
            //Add prefab to cache
            //RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount, objectPoolParentObject.transform);
            RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount, configObject.pooledObjectType);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        Debug.Log("ON NETWORK DESPAWN CALL");

        // Unregisters all objects in PooledPrefabsList from the cache.
        foreach (var prefab in m_Prefabs)
        {
            // Unregister Netcode Spawn handlers
            NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
            m_PooledObjects[prefab].Clear();
        }
        m_PooledObjects.Clear();
        m_Prefabs.Clear();
    }

    public void OnValidate()
    {
        for (var i = 0; i < PooledPrefabsList.Count; i++)
        {
            var prefab = PooledPrefabsList[i].Prefab;
            if (prefab != null)
            {
                Assert.IsNotNull(prefab.GetComponent<NetworkObject>(), $"{nameof(WorldNetworkObjectPoolManager)}: Pooled prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
            }
        }
    }

    /// <summary>
    /// Gets an instance of the given prefab from the pool. The prefab must be registered to the pool.
    /// </summary>
    /// <remarks>
    /// To spawn a NetworkObject from one of the pools, this must be called on the server, then the instance
    /// returned from it must be spawned on the server. This method will then also be called on the client by the
    /// PooledPrefabInstanceHandler when the client receives a spawn message for a prefab that has been registered
    /// here.
    /// </remarks>
    /// <param name="prefab"></param>
    /// <param name="position">The position to spawn the object at.</param>
    /// <param name="rotation">The rotation to spawn the object with.</param>
    /// <returns></returns>
    public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var networkObject = m_PooledObjects[prefab].Get();

        var noTransform = networkObject.transform;
        noTransform.position = position;
        noTransform.rotation = rotation;

        return networkObject;
    }

    public GameObject GetGameObjectWithPoolType(PooledObjectType type)
    {
        return PooledPrefabsList.Where(a => a.pooledObjectType == type).FirstOrDefault().Prefab;
    }

    /// <summary>
    /// Return an object to the pool (reset objects before returning).
    /// </summary>
    public void ReturnNetworkObject(NetworkObject networkObject, GameObject prefab)
    {
        m_PooledObjects[prefab].Release(networkObject);
    }

    /// <summary>
    /// Builds up the cache for a prefab.
    /// </summary>
    void RegisterPrefabInternal(GameObject prefab, int prewarmCount, PooledObjectType prefabType)
    {
        //This is called once!
        //This creates the pool (ObjectPool), which requires:
        //Create function, which initiates the prefab.
        //Get funtion, that lets you enable the pooled object.
        //Release function, lets you disable the pooled object.
        //Destroy function.

        NetworkObject CreateFunc()
        {
            return Instantiate(prefab).GetComponent<NetworkObject>();
            //return Instantiate(prefab, prefabPoolParent).GetComponent<NetworkObject>();
        }

        void ActionOnGet(NetworkObject networkObject)
        {
            networkObject.gameObject.SetActive(true);
        }

        void ActionOnRelease(NetworkObject networkObject)
        {
            networkObject.gameObject.SetActive(false);
        }

        void ActionOnDestroy(NetworkObject networkObject)
        {
            Destroy(networkObject.gameObject);
        }

        m_Prefabs.Add(prefab);

        // Create the pool
        m_PooledObjects[prefab] = new ObjectPool<NetworkObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);

        // Populate the pool
        var prewarmNetworkObjects = new List<NetworkObject>();
        for (var i = 0; i < prewarmCount; i++)
        {
            prewarmNetworkObjects.Add(m_PooledObjects[prefab].Get());
        }
        foreach (var networkObject in prewarmNetworkObjects)
        {
            networkObject.Spawn();

            if (prefabType == PooledObjectType.Instant_Magic_Spell)
            {
                Projectile projectileComp = networkObject.GetComponent<Projectile>();
                projectileComp.objectEnabled.Value = false;
                projectileComp.networkPosition.Value = networkObject.transform.position;
                projectileComp.networkRotation.Value = networkObject.transform.rotation;
                projectileComp.networkPositionVelocity = Vector3.zero;
            }

            //networkObject.gameObject.transform.parent = prefabPoolParent;
            //Make sure the objects start as inactive.
            m_PooledObjects[prefab].Release(networkObject);
        }

        // Register Netcode Spawn handlers
        NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledPrefabInstanceHandler(prefab));
    }
}

//Struct is used to assign the amount of specified objects there are in the pool.
[Serializable]
struct PoolConfigObject
{
    public GameObject Prefab;
    public int PrewarmCount;
    public PooledObjectType pooledObjectType;
}

class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
{
    GameObject m_Prefab;

    public PooledPrefabInstanceHandler(GameObject prefab)
    {
        m_Prefab = prefab;
    }

    NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        return WorldNetworkObjectPoolManager.Instance.GetNetworkObject(m_Prefab, position, rotation);
    }

    void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
    {
        WorldNetworkObjectPoolManager.Instance.ReturnNetworkObject(networkObject, m_Prefab);
    }
}