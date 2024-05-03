using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Erikduss
{ 
    public class WorldBossEncounterManager : NetworkBehaviour
    {
        public static WorldBossEncounterManager Instance;

        public NetworkVariable<float> bossEmpowerDamageMultiplier = new NetworkVariable<float>(1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public List<BossEncounter> bossEncounter = new List<BossEncounter>();

        public int currentActiveBossFightID = -1;

        public int maxAmountOfRespawns = 3;
        public int amountOfRespawnsLeft = 3;

        public int amountOfPermaDeadPlayers = 0;
        public float damageUpdateTimer = 0f;
        public float damageMultiplierUpdateTickDelay = 1f;
        public float damageUpdateAmountPerTick = 0.025f;
        public float currentDamageUpdateAmountPerTick = 0.025f;
        public float bufferTimeBeforeAddingDeadPlayer = 5f;

        public bool amITheHost = false;
        private bool spawnedRagdoll = false;
        private bool bossIsDead = false;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            if (!amITheHost) return;

            if (amountOfPermaDeadPlayers <= 0) return;

            //The value gets updated based on the update delay, making sure its not updated every single update tick.
            if(damageUpdateTimer <= 0)
            {
                //set the correct tick update amount based on player count
                currentDamageUpdateAmountPerTick = (float)((float)amountOfPermaDeadPlayers / (float)WorldGameSessionManager.Instance.players.Count) * damageUpdateAmountPerTick;

                //using the fixed value of the tick update amount, update the empower value with the new value
                float newMultiplier = bossEmpowerDamageMultiplier.Value + (amountOfPermaDeadPlayers * currentDamageUpdateAmountPerTick);
                damageUpdateTimer = damageMultiplierUpdateTickDelay;
                bossEmpowerDamageMultiplier.Value = newMultiplier;
            }
            else
            {
                damageUpdateTimer -= Time.deltaTime;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            bossEmpowerDamageMultiplier.OnValueChanged += OnBossEmpowerDamageMultiplierChanged;
            OnBossEmpowerDamageMultiplierChanged(1f, bossEmpowerDamageMultiplier.Value);

            if (WorldGameSessionManager.Instance.AmITheHost())
            {
                amITheHost = true;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            bossEmpowerDamageMultiplier.OnValueChanged -= OnBossEmpowerDamageMultiplierChanged;
        }

        public void OnBossEmpowerDamageMultiplierChanged(float oldValue, float newValue)
        {
            //in case needed, so far doesnt seem needed due to damage colliders checking the networked value directly.
        }

        public IEnumerator UpdateBossEmpowerDamageMultiplier(int amountOfNewDeadPlayers)
        {
            yield return new WaitForSeconds(bufferTimeBeforeAddingDeadPlayer);
            amountOfPermaDeadPlayers = amountOfNewDeadPlayers;
        }

        public void SetMaxSpawnAmount()
        {
            //max spawns = max players * 0.5 -> rounded up.
            maxAmountOfRespawns = Mathf.CeilToInt(WorldGameSessionManager.Instance.players.Count * 0.5f);

            amountOfRespawnsLeft = maxAmountOfRespawns;
        }

        public void TeleportAllOtherPlayersToEncounter(CharacterManager playerThatTriggeredBossFight, int encounterID)
        {
            //if (!WorldGameSessionManager.Instance.AmITheHost()) return;

            int idOfTeleportLocation = 0;

            foreach(PlayerManager player in WorldGameSessionManager.Instance.players)
            {
                //Only teleport the players that didnt trigger the enounter.
                if (player.NetworkObjectId != playerThatTriggeredBossFight.NetworkObjectId)
                {
                    TeleportPlayerToSpawnPoint(player, idOfTeleportLocation, encounterID, false, true);
                }
                else
                {
                    TeleportPlayerToSpawnPoint(player, idOfTeleportLocation, encounterID, true, true);
                }

                idOfTeleportLocation++;
            }
        }

        public void TeleportPlayerToSpawnPoint(PlayerManager player, int teleportLocationID, int encounterID, bool preventTeleport , bool setSpawnCharges, bool overrideSpawnLocation = false)
        {
            player.playerNetworkManager.NotifyTheServerOfTeleportActionServerRpc(player.NetworkObjectId, teleportLocationID, encounterID, preventTeleport , setSpawnCharges, overrideSpawnLocation);
        }

        public bool DoWeStillHaveRespawnsAvailable()
        {
            if (amountOfRespawnsLeft > 0) return true;
            else return false;
        }

        public void RespawnPlayerAtEnouncterSpawnPoint(PlayerManager player, int encounterID)
        {
            if (WorldGameSessionManager.Instance.AmITheHost())
            {
                //set random location?
                TeleportPlayerToSpawnPoint(player, 0, encounterID, false, false);
            }

            amountOfRespawnsLeft -= 1;
        }

        public void FollowOtherPlayerWithCamera()
        {
            if (EveryoneIsDead()) return;
            if (DoWeStillHaveRespawnsAvailable()) return;

            //Follow another alive player to spectate.
            PlayerManager playerToFollow = null;

            foreach(PlayerManager player in WorldGameSessionManager.Instance.players)
            {
                if (!player.characterNetworkManager.isDead.Value)
                {
                    playerToFollow = player;
                    break; //break out of loop to pick this player
                }
            }

            if (playerToFollow == null) Debug.LogError("SOMETHING WENT WRONG, DONT HAVE A PLAYER TO FOLLOW");

            PlayerCamera.instance.SetPlayerToFollowWhileWeAreDead(playerToFollow);
        }

        public void UpdateDeadPlayersList()
        {
            //only the host needs this
            if (!WorldGameSessionManager.Instance.AmITheHost()) return;

            if (DoWeStillHaveRespawnsAvailable()) return; //if we still have respawns, we dont add players to this.

            int countedDeadPlayers = 0;

            foreach (PlayerManager player in WorldGameSessionManager.Instance.players)
            {
                if (!player.characterNetworkManager.isDead.Value) countedDeadPlayers++;
            }

            if (!amITheHost) amITheHost = true; //in case the OnNetworkSpawn Misses this.

            StartCoroutine(UpdateBossEmpowerDamageMultiplier(countedDeadPlayers));
        }

        public bool EveryoneIsDead()
        {
            bool everyoneIsDead = true;

            foreach (PlayerManager player in WorldGameSessionManager.Instance.players)
            {
                if (!player.characterNetworkManager.isDead.Value) everyoneIsDead = false;
            }

            return everyoneIsDead;
        }

        public void SpawnRagdollOfBoss(GameObject ragdoll, Vector3 position, quaternion rotation, float delay)
        {
            //only do this once
            if (spawnedRagdoll) return;
            StartCoroutine(SpawnRagdollWithDelay(ragdoll, position, rotation, delay));
        }

        public IEnumerator SpawnRagdollWithDelay(GameObject ragdoll, Vector3 position, quaternion rotation, float delay)
        {
            spawnedRagdoll = true;
            yield return new WaitForSeconds(delay);
            Instantiate(ragdoll, position, rotation);
        }

        public IEnumerator BossDefeatedEndGame()
        {
            Debug.Log("We should end the game");

            bossIsDead = true;

            yield return new WaitForSeconds(10);

            NetworkManager.Singleton.SceneManager.LoadScene("LoadingToMainMenuVictory", LoadSceneMode.Single);
        }

        public void BossDefeated()
        {
            if (!WorldGameSessionManager.Instance.AmITheHost()) return;
            Debug.Log("Got into boss defeated void");
            StartCoroutine(BossDefeatedEndGame());
        }

        public void CheckIfEncounterNeedsToBeRestarted()
        {
            //only the host should call to reset everything
            if (!WorldGameSessionManager.Instance.AmITheHost()) return;

            if (bossIsDead) return;

            if (EveryoneIsDead())
            {
                //TODO: RESET THE ENCOUNTER
                Debug.Log("EVERYONE IS DEAD, RESET ENCOUNTER");
                NetworkManager.Singleton.SceneManager.LoadScene("ReloadingWorldScene", LoadSceneMode.Single);
            }
        }
    }
}
