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

    [Header("SpawningObjects")]
    public GameObject bossHolder;

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
        if (bossHolder == null) 
        {
            //Animations keep teleporting the boss back to 0,0,0. Using a boss holder as parent to move the boss to the correct position.
            //Sync this boss holder object's position with the boss's position.
            bossHolder = Instantiate(new GameObject());
            bossHolder.name = "Boss Holder";
            NetworkObject bossHolderNetObj = bossHolder.AddComponent<NetworkObject>();
            bossHolderNetObj.Spawn();
        }

        foreach (EntitySpawnInformation entity in aiCharacters)
        {
            GameObject instantiatedCharacter;
            bool isBoss = false;

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
                            isBoss = true;
                        break;
                    default:
                            entityToSpawn = basicDummyPrefab;
                        break;
                }

                if (isBoss) bossHolder.transform.position = entity.SpawnPosition;

                instantiatedCharacter = Instantiate(entityToSpawn, Vector3.zero, Quaternion.identity);
            }

            instantiatedCharacter.transform.position = entity.SpawnPosition;
            instantiatedCharacter.transform.localRotation = entity.SpawnRotation;

            instantiatedCharacter.GetComponent<NetworkObject>().Spawn();
            if (isBoss)
            {
                instantiatedCharacter.transform.parent = bossHolder.transform;
            }
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
