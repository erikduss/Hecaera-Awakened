using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class NetworkObjectSpawner : MonoBehaviour
    {
        [Header("Object")]
        [SerializeField] GameObject networkGameObject;
        [SerializeField] GameObject instantiatedGameObject;

        private void Awake()
        {

        }

        private void Start()
        {
            if(WorldGameSessionManager.Instance.AmITheHost())
                StartCoroutine(SpawnObject());
            else
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator SpawnObject()
        {
            //We want to wait till the character spawn in first. This is to prevent issues with objects not being correctly linked to a character.
            while (WorldAIManager.Instance.spawnedInBosses.Count == 0)
                yield return new WaitForEndOfFrame();

            WorldObjectManager.Instance.SpawnObject(this);
            gameObject.SetActive(false);
        }

        public void AttemptToSpawnCharacter()
        {
            if (networkGameObject != null)
            {
                instantiatedGameObject = Instantiate(networkGameObject);
                instantiatedGameObject.transform.position = transform.position;
                instantiatedGameObject.transform.rotation = transform.rotation;
                instantiatedGameObject.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
