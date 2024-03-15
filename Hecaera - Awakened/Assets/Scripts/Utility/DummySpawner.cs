using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class DummySpawner : NetworkBehaviour
    {
        [SerializeField] GameObject dummyPrefab;

        [SerializeField] List<Vector3> spawnLocations = new List<Vector3>();

        List<NetworkObject> spawnedDummies = new List<NetworkObject>();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        // Start is called before the first frame update
        public override void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            base.OnNetworkSpawn();

            foreach (Vector3 t in spawnLocations)
            {
                var dummy = Instantiate(dummyPrefab, Vector3.zero, Quaternion.identity);

                dummy.transform.position = t;
                dummy.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                NetworkObject netComponent = dummy.GetComponent<NetworkObject>();
                netComponent.Spawn();

                spawnedDummies.Add(netComponent);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }
    }
}
