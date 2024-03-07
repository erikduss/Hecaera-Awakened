using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AIBossCharacterManager : AICharacterManager
{
    public int bossID = 0;
    [SerializeField] bool hasBeenDefeated = false;
    [SerializeField] bool hasBeenAwakened = false;
    [SerializeField] List<FogWallInteractable> fogWalls;

    [Header("DEBUG")]
    [SerializeField] bool wakeBossUp = false;

    protected override void Update()
    {
        base.Update();

        if (wakeBossUp)
        {
            wakeBossUp = false;
            WakeBoss();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            //if our save data does not contain information on this boss, add it now.
            if(!WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.ContainsKey(bossID))
            {
                WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, false);
                WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Add(bossID, false); //cant defeat a boss that isnt awakened.
            }
            else //load the data
            {
                hasBeenDefeated = WorldSaveGameManager.instance.currentCharacterData.bossesDefeated[bossID];
                hasBeenAwakened = WorldSaveGameManager.instance.currentCharacterData.bossesAwakened[bossID];
            }

            StartCoroutine(GetFogWallsFromWorldObjectManager());

            if (hasBeenAwakened && !hasBeenDefeated) //if the boss is awakened but not defeated
            {
                for(int i = 0; i < fogWalls.Count; i++)
                {
                    fogWalls[i].isActive.Value = true;
                }
            }
            else if (hasBeenDefeated) //if the boss is defeated
            {
                for (int i = 0; i < fogWalls.Count; i++)
                {
                    fogWalls[i].isActive.Value = false;
                }

                aICharacterNetworkManager.isActive.Value = false;
            }
            else //If the boss is not interacted with yet.
            {
                for (int i = 0; i < fogWalls.Count; i++)
                {
                    fogWalls[i].isActive.Value = false;
                }
            }
        }
    }

    private IEnumerator GetFogWallsFromWorldObjectManager()
    {
        while(WorldObjectManager.Instance.fogWalls.Count == 0)
            yield return new WaitForEndOfFrame();

        fogWalls = new List<FogWallInteractable>();

        foreach (var fogWall in WorldObjectManager.Instance.fogWalls)
        {
            Debug.Log("Add fog walls");
            if (fogWall.fogWallID == bossID)
            {
                Debug.Log("Found matching fog wall");
                fogWalls.Add(fogWall);
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

    public void WakeBoss()
    {
        hasBeenAwakened = true;

        if (!WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.ContainsKey(bossID))
        {
            WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, true);
        }
        else
        {
            WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Remove(bossID);
            WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, true);
        }

        for(int i = 0; i < fogWalls.Count; i++)
        {
            fogWalls[i].isActive.Value = true;
        }
    }
}
