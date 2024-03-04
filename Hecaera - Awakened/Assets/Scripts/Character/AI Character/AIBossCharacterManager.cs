using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBossCharacterManager : AICharacterManager
{
    public int bossID = 0;
    [SerializeField] bool hasBeenDefeated = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            //if our save data does not contain information on this boss, add it now.
            if(WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.ContainsKey(bossID))
            {
                WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, false);
                WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Add(bossID, false); //cant defeat a boss that isnt awakened.
            }
            else //load the data
            {
                hasBeenDefeated = WorldSaveGameManager.instance.currentCharacterData.bossesDefeated[bossID];

                if (hasBeenDefeated)
                {
                    aICharacterNetworkManager.isActive.Value = false;
                }
            }
        }
    }

    public override IEnumerator ProcessDeathEvent(bool manuallySelectDeathAnimation = false)
    {
        if (IsOwner)
        {
            characterNetworkManager.currentHealth.Value = 0;
            characterNetworkManager.isDead.Value = true;

            if (!manuallySelectDeathAnimation)
            {
                characterAnimatorManager.PlayTargetActionAnimation("Dead_01", true);
            }

            hasBeenDefeated = true;

            if (!WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.ContainsKey(bossID))
            {
                WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, true);
                WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Add(bossID, true);
            }
            else
            {
                //we re-add it to the dictionary with the true value.
                WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Remove(bossID);
                WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Remove(bossID);
                WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, true);
                WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Add(bossID, true);
            }

            //autosave the progress
            WorldSaveGameManager.instance.SaveGame();
        }

        yield return new WaitForSeconds(5f);
    }
}
