using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Erikduss
{ 
    public class WorldBossEncounterManager : MonoBehaviour
    {
        public static WorldBossEncounterManager Instance;

        public List<BossEncounter> bossEncounter = new List<BossEncounter>();

        public int currentActiveBossFightID = -1;

        public int maxAmountOfRespawns = 3;
        public int amountOfRespawnsLeft = 3;

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

        public bool EveryoneIsDead()
        {
            bool everyoneIsDead = true;

            foreach (PlayerManager player in WorldGameSessionManager.Instance.players)
            {
                if (!player.characterNetworkManager.isDead.Value) everyoneIsDead = false;
            }

            return everyoneIsDead;
        }

        public void CheckIfEncounterNeedsToBeRestarted()
        {
            //only the host should call to reset everything
            if (!WorldGameSessionManager.Instance.AmITheHost()) return;

            if (EveryoneIsDead())
            {
                //TODO: RESET THE ENCOUNTER
                Debug.Log("EVERYONE IS DEAD, RESET ENCOUNTER");
                NetworkManager.Singleton.SceneManager.LoadScene("ReloadingWorldScene", LoadSceneMode.Single);
            }
        }
    }
}
