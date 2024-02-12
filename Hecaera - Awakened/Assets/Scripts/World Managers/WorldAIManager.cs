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
    [SerializeField] GameObject[] aiCharacters;
    [SerializeField] List<GameObject> spawnedInCharacter;
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
        foreach (GameObject character in aiCharacters)
        {
            GameObject instantiatedCharacter = Instantiate(character);
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
