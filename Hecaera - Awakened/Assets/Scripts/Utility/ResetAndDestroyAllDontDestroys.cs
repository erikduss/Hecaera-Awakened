using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Erikduss
{
    public class ResetAndDestroyAllDontDestroys : MonoBehaviour
    {
        public bool resetInsteadOfDestroy = false;

        public bool startedDestroying = false;
        public string sceneNameToLoadTo = "Scene_Main_Menu_01";

        private bool forceSkipCredits = false;

        private float timer = 0f;

        // Start is called before the first frame update
        void Start()
        {
            if(!resetInsteadOfDestroy)
                StartCoroutine(DestroyEverything());
            else
                StartCoroutine(ResetEverything());
        }

        // Update is called once per frame
        void Update()
        {
            if (!startedDestroying)
            {
                startedDestroying = true;

                if (!resetInsteadOfDestroy)
                    StartCoroutine(DestroyEverything());
                else
                    StartCoroutine(ResetEverything());
            }

            if(timer < 5f)
            {
                timer += Time.deltaTime;
                return;
            }
 
            if(Input.anyKeyDown)
                forceSkipCredits = true;

            if (forceSkipCredits)
            {
                //prevent it being called every frame
                forceSkipCredits = false;
                LoadToNextScene();
            }
        }

        private IEnumerator ResetEverything()
        {
            startedDestroying = true;

            if (WorldGameSessionManager.Instance.AmITheHost())
            {
                Destroy(WorldObjectManager.Instance.fogWalls[0].gameObject); //force destroy old fogwall

                foreach (var player in WorldGameSessionManager.Instance.players)
                {
                    //player.characterNetworkManager.networkPosition.Value = Vector3.zero;
                    player.transform.position = Vector3.zero;
                    WorldBossEncounterManager.Instance.TeleportPlayerToSpawnPoint(player, 0, 0, false, false, true);
                    player.ReviveCharacter();
                }
            }

            foreach (var player in WorldGameSessionManager.Instance.players)
            {
                player.ReviveCharacter(); //set the collider back on for all characters
            }

            //readd the dictionary entry of the boss being awakened.
            WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Remove(0);
            WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(0, false);

            IxeleceMaterialManagement.Instance.ClearMaterialLists();

            //ensure the player is back at 0,0,0 before healing and returning them back to life.
            WorldGameSessionManager.Instance.TeleportLocalPlayerToSpawn();
            yield return new WaitForSeconds(1f);

            WorldGameSessionManager.Instance.HealLocalPlayerToFull();
            WorldGameSessionManager.Instance.ResetLocalPlayerUI();

            WorldSoundFXManager.instance.StopBossTrack();

            yield return new WaitForSeconds(2.5f);

            if (WorldGameSessionManager.Instance.AmITheHost())
                NetworkManager.Singleton.SceneManager.LoadScene(WorldSaveGameManager.instance.worldSceneName, LoadSceneMode.Single);

            //ensure the player is back at 0,0,0 before healing and returning them back to life.
            WorldGameSessionManager.Instance.TeleportLocalPlayerToSpawn();

            //SceneManager.LoadScene(WorldSaveGameManager.instance.worldSceneName);

            //if (WorldActionManager.Instance == null && ConnectionManager.Instance == null && WorldGameSessionManager.Instance == null
            //    && PlayerUIManager.instance == null && PlayerInputManager.instance == null && PlayerCamera.instance == null
            //    && WorldSoundFXManager.instance == null && WorldItemDatabase.Instance == null && WorldProjectilesManager.Instance == null
            //    && WorldActionManager.Instance == null && PlayerMaterialManagement.Instance == null && WorldSaveGameManager.instance == null)
            //{
            //    NetworkManager.Singleton.SceneManager.LoadScene(WorldSaveGameManager.instance.worldSceneName, LoadSceneMode.Single);
            //}
            //else
            //{
            //    startedDestroying = false;
            //    yield return null;
            //}
        }

        private IEnumerator DestroyEverything()
        {
            startedDestroying = true;

            Destroy(WorldActionManager.Instance.gameObject);
            //world connection manager
            Destroy(ConnectionManager.Instance.gameObject);
            //world network manager
            Destroy(WorldGameSessionManager.Instance.gameObject);
            //player ui manager
            Destroy(PlayerUIManager.instance.gameObject);
            //player input manager
            Destroy(PlayerInputManager.instance.gameObject);
            //player camera
            Destroy(PlayerCamera.instance.gameObject);
            //world souns fx manager
            Destroy(WorldSoundFXManager.instance.gameObject);
            //world item database
            Destroy(WorldItemDatabase.Instance.gameObject);
            //world projectile manager
            Destroy(WorldProjectilesManager.Instance.gameObject);
            //world action manager
            Destroy(WorldActionManager.Instance.gameObject);
            //player material manager
            Destroy(PlayerMaterialManagement.Instance.gameObject);
            //this
            Destroy(WorldSaveGameManager.instance.gameObject);

            yield return new WaitForSeconds(1);

            if(SceneManager.GetActiveScene().name == "LoadingToMainMenuVictory")
            {
                yield return new WaitForSeconds(30f);
            }

            if (!LoadToNextScene()) yield return null;
        }

        public bool LoadToNextScene()
        {
            bool success = false;

            if (WorldActionManager.Instance == null && ConnectionManager.Instance == null && WorldGameSessionManager.Instance == null
                && PlayerUIManager.instance == null && PlayerInputManager.instance == null && PlayerCamera.instance == null
                && WorldSoundFXManager.instance == null && WorldItemDatabase.Instance == null && WorldProjectilesManager.Instance == null
                && WorldActionManager.Instance == null && PlayerMaterialManagement.Instance == null && WorldSaveGameManager.instance == null)
            {
                success = true;
                SceneManager.LoadScene(sceneNameToLoadTo); //go back to splash screen
            }
            else
            {
                success = false;
                startedDestroying = false;
            }

            return success;
        }
    }
}
