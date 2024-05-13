using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Erikduss
{
    public class AIBossCharacterManager : AICharacterManager
    {
        public int bossID = 0;

        [Header("Status")]
        public NetworkVariable<bool> bossFightIsActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> hasBeenDefeated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> hasBeenAwakened = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> currentBossPhase = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> amountOfPhases = new NetworkVariable<int>(4, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField] List<FogWallInteractable> fogWalls;
        [SerializeField] string sleepAnimation;
        [SerializeField] string awakenAnimation;

        [Header("Phase Shift")]
        public float minimumHealthPercentageToShift = 0;
        [SerializeField] string phaseShiftAnimation = "Phase_Change_01";
        [SerializeField] CombatStanceState phase02CombatStanceState;
        [SerializeField] CombatStanceState phase03CombatStanceState;
        [SerializeField] CombatStanceState phase04CombatStanceState;

        [Header("States")]
        [SerializeField] BossSleepState sleepState;

        [Header("Poise")]
        public bool currentlyUsePoise = false;
        public float currentPoiseValue = 0;
        public float poiseDamagePerHitMultiplier = 1f; //how much poise per damage value? for example, 10 damage = 10 poise damage.

        public GameObject ragdollObject;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            bossFightIsActive.OnValueChanged += OnBossFightIsActiveChanged;
            OnBossFightIsActiveChanged(false, bossFightIsActive.Value);

            if (IsOwner)
            {
                sleepState = Instantiate(sleepState);

                currentState = sleepState;
            }

            if (IsServer)
            {
                //if our save data does not contain information on this boss, add it now.
                if (!WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.ContainsKey(bossID))
                {
                    WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, false);
                    WorldSaveGameManager.instance.currentCharacterData.bossesDefeated.Add(bossID, false); //cant defeat a boss that isnt awakened.
                }
                else //load the data
                {
                    hasBeenDefeated.Value = WorldSaveGameManager.instance.currentCharacterData.bossesDefeated[bossID];
                    hasBeenAwakened.Value = WorldSaveGameManager.instance.currentCharacterData.bossesAwakened[bossID];
                }

                //the fogwalls are active in the build, shouldnt happen.
                //Besides, this only happens when joining into the game scene. So it should ALWAYS be false when joining the game scene.
                hasBeenAwakened.Value = false;
                hasBeenDefeated.Value = false;

                StartCoroutine(GetFogWallsFromWorldObjectManager());

                if (!hasBeenAwakened.Value)
                {
                    characterAnimatorManager.PlayTargetActionAnimation(sleepAnimation, true);
                }
            }

            animator.SetBool("IsAwakened", hasBeenAwakened.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            bossFightIsActive.OnValueChanged -= OnBossFightIsActiveChanged;
        }

        private IEnumerator GetFogWallsFromWorldObjectManager()
        {
            while (WorldObjectManager.Instance.fogWalls.Count == 0)
                yield return new WaitForEndOfFrame();

            fogWalls = new List<FogWallInteractable>();

            foreach (var fogWall in WorldObjectManager.Instance.fogWalls)
            {
                if (fogWall.fogWallID == bossID)
                {
                    fogWalls.Add(fogWall);
                }
            }

            if (hasBeenAwakened.Value && !hasBeenDefeated.Value) //if the boss is awakened but not defeated
            {
                for (int i = 0; i < fogWalls.Count; i++)
                {
                    fogWalls[i].isActive.Value = true;
                }
            }
            else if (hasBeenDefeated.Value) //if the boss is defeated
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

        public override IEnumerator ProcessDeathEvent(bool manuallySelectDeathAnimation = false)
        {
            if (currentBossPhase.Value >= amountOfPhases.Value)
            {
                Debug.Log("Boss Death");
                PlayerUIManager.instance.playerUIPopUpManager.SendBossDefeatedPopUp("IXELECE DEFEATED");
                if (IsOwner)
                {
                    characterNetworkManager.currentHealth.Value = 0;
                    characterNetworkManager.isDead.Value = true;

                    bossFightIsActive.Value = false;

                    foreach (var fogWall in fogWalls)
                    {
                        fogWall.isActive.Value = false;
                    }

                    if (!manuallySelectDeathAnimation)
                    {
                        characterAnimatorManager.PlayTargetActionAnimation("Dead_01", true);
                    }

                    hasBeenDefeated.Value = true;

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

                Debug.Log("Spawn Ragdoll");
                WorldBossEncounterManager.Instance.SpawnRagdollOfBoss(ragdollObject, transform.position, transform.rotation, 1.5f);
                WorldBossEncounterManager.Instance.BossDefeated();

                yield return new WaitForSeconds(1.5f);

                if (IsOwner)
                {
                    //NetworkObject.StopAllCoroutines();
                    //aICharacterNetworkManager.StopAllCoroutines();
                    this.NetworkObject.Despawn();
                }
            }

            yield return null;
        }

        public virtual void WakeBoss()
        {
            if (IsOwner)
            {
                Debug.Log("Boss is being woken up");

                if (!hasBeenAwakened.Value)
                {
                    characterAnimatorManager.PlayTargetActionAnimation(awakenAnimation, true);
                }

                bossFightIsActive.Value = true;
                hasBeenAwakened.Value = true;
                currentState = idle;

                if (!WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.ContainsKey(bossID))
                {
                    WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, true);
                }
                else
                {
                    WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Remove(bossID);
                    WorldSaveGameManager.instance.currentCharacterData.bossesAwakened.Add(bossID, true);
                }

                for (int i = 0; i < fogWalls.Count; i++)
                {
                    fogWalls[i].isActive.Value = true;
                }
            }
        }

        private void OnBossFightIsActiveChanged(bool oldStatus, bool newStatus)
        {
            if (bossFightIsActive.Value)
            {
                animator.SetBool("IsAwakened", true);

                //only do this when the server
                if (IsOwner)
                {
                    //TODO always scale this to at least 2 players, boss cannot be soloed.
                    //Set the boss's health value based on the amount of players
                    aICharacterNetworkManager.maxHealth.Value = aICharacterNetworkManager.maxHealth.Value * WorldGameSessionManager.Instance.players.Count;
                    aICharacterNetworkManager.currentHealth.Value = aICharacterNetworkManager.maxHealth.Value;

                    currentBossPhase.Value = 1; //set to active phase
                }

                GameObject bossHealthBar = Instantiate(PlayerUIManager.instance.playerUIHudManager.bossHealthBarObject,
                PlayerUIManager.instance.playerUIHudManager.bossHealthBarParent);

                UI_Boss_HP_Bar bossHPBar = bossHealthBar.GetComponentInChildren<UI_Boss_HP_Bar>();
                bossHPBar.EnableBossHPBar(this);
                bossHPBar.hasAnotherPhase = true;

                WorldSoundFXManager.instance.PlayBossTrack(WorldSoundFXManager.instance.ixeleceBossFightIntroMusic, WorldSoundFXManager.instance.ixeleceBossFightPhase1Music);
            }
            else
            {
                WorldSoundFXManager.instance.StopBossTrack();
            }
        }

        public void PhaseShift()
        {
            Debug.Log("Phase shift!");

            //switch to second phase
            if(currentBossPhase.Value == 1)
            {
                characterAnimatorManager.PlayTargetActionAnimation(phaseShiftAnimation, true);
                combbatStance = Instantiate(phase02CombatStanceState);
                currentState = combbatStance;

                aICharacterNetworkManager.currentHealth.Value = aICharacterNetworkManager.maxHealth.Value; //retore health

                currentBossPhase.Value = currentBossPhase.Value + 1;
            }
            else if (currentBossPhase.Value == 2)
            {
                characterAnimatorManager.PlayTargetActionAnimation(phaseShiftAnimation, true);
                combbatStance = Instantiate(phase03CombatStanceState);
                currentState = combbatStance;

                aICharacterNetworkManager.currentHealth.Value = aICharacterNetworkManager.maxHealth.Value; //retore health

                currentBossPhase.Value = currentBossPhase.Value + 1;
            }
            else if (currentBossPhase.Value == 3)
            {
                characterAnimatorManager.PlayTargetActionAnimation(phaseShiftAnimation, true);
                combbatStance = Instantiate(phase04CombatStanceState);
                currentState = combbatStance;

                aICharacterNetworkManager.currentHealth.Value = aICharacterNetworkManager.maxHealth.Value; //retore health

                currentBossPhase.Value = currentBossPhase.Value + 1;
            }
        }

        public void SetNewPoiseValueToBreak(float poiseValueToBreakPerPlayer)
        {
            currentlyUsePoise = true;

            currentPoiseValue = poiseValueToBreakPerPlayer * WorldGameSessionManager.Instance.players.Count;
        }

        public void CheckPoiseBreak(float decreasePoiseBy)
        {
            if (!currentlyUsePoise) return;

            currentPoiseValue -= decreasePoiseBy;

            if (currentPoiseValue <= 0)
            {
                if(aICharacterCombatManager.currentAttackType == AttackType.NatureFury)
                {
                    currentlyUsePoise = false;
                    characterAnimatorManager.PlayTargetActionAnimation("NatureFury_PoiseCancel", true);
                }
            }
        }
    }
}
