using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AICharacterSpawner : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] GameObject characterGameObject;
    [SerializeField] GameObject instantiatedGameObject;

    [SerializeField] bool isBoss = false;

    private void Awake()
    {
        
    }

    private void Start()
    {
        WorldAIManager.Instance.SpawnCharacter(this);
        gameObject.SetActive(false);
    }

    public void AttemptToSpawnCharacter()
    {
        if(characterGameObject != null)
        {
            //if (isBoss) return; //TODO FIX ISSUES WITH BOSS SPAWNING

            Debug.Log(characterGameObject);

            instantiatedGameObject = Instantiate(characterGameObject);
            instantiatedGameObject.transform.position = transform.position;
            instantiatedGameObject.transform.rotation = transform.rotation;
            instantiatedGameObject.GetComponent<NetworkObject>().Spawn();

            if (isBoss)
            {
                //This is the center of the boss fight arena
                instantiatedGameObject.transform.parent = WorldAIManager.Instance.gameObject.transform;
            }
        }
    }
}
