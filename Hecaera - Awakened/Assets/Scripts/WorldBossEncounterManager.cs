using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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

        public void TeleportPlayerToSpawnPoint(PlayerManager player, int teleportLocationID, int encounterID, bool preventTeleport , bool setSpawnCharges)
        {
            player.playerNetworkManager.NotifyTheServerOfTeleportActionServerRpc(player.NetworkObjectId, teleportLocationID, encounterID, preventTeleport , setSpawnCharges);
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
    }
}
