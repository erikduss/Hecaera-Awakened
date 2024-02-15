using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldAIManager : MonoBehaviour
{
    public static WorldAIManager Instance;

    [Header("DEBUG")]
    [SerializeField] bool despawnCharacters = false;
    [SerializeField] bool respawnCharacters = false;

    [Header("Characters")]
    [SerializeField] EntitySpawnInformation[] aiCharacters;
    [SerializeField] List<GameObject> spawnedInCharacter;

    [Header("EntityTypesPrefabs")]
    [SerializeField] GameObject basicDummyPrefab;
    [SerializeField] GameObject bossPrefab;

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

    private void Update()
    {
        if (despawnCharacters)
        {
            despawnCharacters = false;
            DespawnAllCharacters();
        }

        if (respawnCharacters)
        {
            respawnCharacters = false;
            SpawnAllCharacters();
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            //spawn all ai in the scene
            StartCoroutine(WaitForSceneToLoadThenSpawnCharacters());
        }
    }

    private IEnumerator WaitForSceneToLoadThenSpawnCharacters()
    {
        while (!SceneManager.GetActiveScene().isLoaded)
        {
            yield return null;
        }

        SpawnAllCharacters();
    }

    private void SpawnAllCharacters()
    {
        foreach (EntitySpawnInformation entity in aiCharacters)
        {
            GameObject instantiatedCharacter;

            if (entity.OverrideSpawnGameObject != null)
            {
                instantiatedCharacter = Instantiate(entity.OverrideSpawnGameObject, Vector3.zero, Quaternion.identity);
            }
            else
            {
                GameObject entityToSpawn = null;

                switch (entity.EntityType)
                {
                    case EnemySpawnType.BASIC_DUMMY:
                            entityToSpawn = basicDummyPrefab;
                        break;
                    case EnemySpawnType.BOSS:
                            entityToSpawn = bossPrefab;
                        break;
                    default:
                            entityToSpawn = basicDummyPrefab;
                        break;
                }

                instantiatedCharacter = Instantiate(entityToSpawn, Vector3.zero, Quaternion.identity);
            }

            instantiatedCharacter.transform.position = entity.SpawnPosition;
            instantiatedCharacter.transform.localRotation = entity.SpawnRotation;

            instantiatedCharacter.GetComponent<NetworkObject>().Spawn();
            spawnedInCharacter.Add(instantiatedCharacter);
        }
    }

    private void DespawnAllCharacters()
    {
        foreach(GameObject character in spawnedInCharacter)
        {
            character.GetComponent<NetworkObject>().Despawn();
        }
    }

    private void DisableAllCharacters()
    {
        //disable character gameobjects, sync disabled status on network.
        //disable gameobjects upon connecting
        //can be used to disable characters that are far from players to save memory.
    }
}
