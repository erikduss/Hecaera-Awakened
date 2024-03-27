using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Erikduss
{ 
    public class WorldBossEncounterManager : MonoBehaviour
    {
        public static WorldBossEncounterManager Instance;

        public List<BossEncounter> bossEncounter = new List<BossEncounter>();

        public int currentActiveBossFightID = -1;

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

        public void TeleportAllOtherPlayersToEncounter(CharacterManager playerThatTriggeredBossFight, int encounterID)
        {
            if (!WorldGameSessionManager.Instance.AmITheHost()) return;

            int idOfTeleportLocation = 0;

            foreach(PlayerManager player in WorldGameSessionManager.Instance.players)
            {
                //Only teleport the players that didnt trigger the enounter.
                if(player.NetworkObjectId != playerThatTriggeredBossFight.NetworkObjectId)
                {
                    TeleportPlayerToSpawnPoint(player, idOfTeleportLocation, encounterID);
                }

                idOfTeleportLocation++;
            }
        }

        public void TeleportPlayerToSpawnPoint(PlayerManager player, int teleportLocationID, int encounterID)
        {
            player.playerNetworkManager.networkPosition.Value = bossEncounter.Where(a => a.encounterBossID == encounterID).FirstOrDefault().playerSpawnLocations[teleportLocationID].position;
        }

        public void RespawnPlayerAtEnouncterSpawnPoint()
        {

        }
    }
}
