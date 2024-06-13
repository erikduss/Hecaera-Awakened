using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
        public NetworkVariable<bool> bossIsClone = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField] List<FogWallInteractable> fogWalls;
        [SerializeField] string sleepAnimation;
        [SerializeField] string awakenAnimation;

        [Header("Phase Shift")]
        public float minimumHealthPercentageToShift = 0;
        [SerializeField] string phaseShiftAnimation = "Phase_Change_01";
        [SerializeField] CombatStanceState phase02CombatStanceState;
        [SerializeField] CombatStanceState phase03CombatStanceState;
        [SerializeField] CombatStanceState phase04CombatStanceState;
        [SerializeField] CombatStanceState cloneCombatStanceState;
        public AICharacterAttackAction bossSpecialPhaseAttack;

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

            bossIsClone.OnValueChanged += OnBossIsCloneValueChanged;

            if (!bossIsClone.Value)
            {
                bossFightIsActive.OnValueChanged += OnBossFightIsActiveChanged;
                OnBossFightIsActiveChanged(false, bossFightIsActive.Value);
            }

            if (IsOwner)
            {
                sleepState = Instantiate(sleepState);

                if (!bossIsClone.Value)
                    currentState = sleepState;
                else
                {
                    currentState = idle;
                    combatStance = cloneCombatStanceState;

                    aICharacterNetworkManager.currentHealth.Value = 1;
                    aICharacterNetworkManager.maxHealth.Value = 1;

                    aICharacterNetworkManager.CheckHP(1, 1);
                }
            }

            if (!bossIsClone.Value)
            {
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
            else
            {
                animator.SetBool("IsAwakened", true);
            }
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
            if (currentBossPhase.Value >= amountOfPhases.Value || bossIsClone.Value)
            {
                if(!bossIsClone.Value)
                    PlayerUIManager.instance.playerUIPopUpManager.SendBossDefeatedPopUp("IXELECE DEFEATED");

                if (IsOwner)
                {
                    characterNetworkManager.currentHealth.Value = 0;
                    characterNetworkManager.isDead.Value = true;

                    if (!bossIsClone.Value)
                    {
                        bossFightIsActive.Value = false;

                        foreach (var fogWall in fogWalls)
                        {
                            fogWall.isActive.Value = false;
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
                    

                    if (!manuallySelectDeathAnimation)
                    {
                        characterAnimatorManager.PlayTargetActionAnimation("Dead_01", true);
                    }
                }

                if (!bossIsClone.Value)
                {
                    WorldBossEncounterManager.Instance.SpawnRagdollOfBoss(ragdollObject, transform.position, transform.rotation, 1.5f);
                    WorldBossEncounterManager.Instance.BossDefeated();
                }

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

                    if(!WorldGameSessionManager.Instance.skipPhase1)
                        aICharacterNetworkManager.currentHealth.Value = aICharacterNetworkManager.maxHealth.Value;
                    else
                        aICharacterNetworkManager.currentHealth.Value = 1;

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

        public virtual void PhaseShift()
        {
            currentState = idle;

            if (bossIsClone.Value) return;

            //switch to second phase
            if(currentBossPhase.Value == 1)
            {
                characterAnimatorManager.PlayTargetActionAnimation(phaseShiftAnimation, true);
                combatStance = Instantiate(phase02CombatStanceState);
                //currentState = combatStance;

                if(!WorldGameSessionManager.Instance.skipPhase2)
                    aICharacterNetworkManager.currentHealth.Value = aICharacterNetworkManager.maxHealth.Value; //retore health
                else
                    aICharacterNetworkManager.currentHealth.Value = 1;

                aICharacterNetworkManager.isDead.Value = false;
                isPerformingAction = false;
                aICharacterCombatManager.actionRecoveryTimer = 0;

                currentBossPhase.Value = currentBossPhase.Value + 1;

                if (bossSpecialPhaseAttack != null)
                {
                    base.attack.currentAttack = bossSpecialPhaseAttack;
                    attack.hasPerformedAttack = false;
                    currentState = attack;
                }
            }
            else if (currentBossPhase.Value == 2)
            {
                characterAnimatorManager.PlayTargetActionAnimation(phaseShiftAnimation, true);
                combatStance = Instantiate(phase03CombatStanceState);
                //currentState = combatStance;

                if (!WorldGameSessionManager.Instance.skipPhase3)
                    aICharacterNetworkManager.currentHealth.Value = aICharacterNetworkManager.maxHealth.Value; //retore health
                else
                    aICharacterNetworkManager.currentHealth.Value = 1;

                aICharacterNetworkManager.isDead.Value = false;
                isPerformingAction = false;
                aICharacterCombatManager.actionRecoveryTimer = 0;

                currentBossPhase.Value = currentBossPhase.Value + 1;

                if (bossSpecialPhaseAttack != null)
                {
                    base.attack.currentAttack = bossSpecialPhaseAttack;
                    attack.hasPerformedAttack = false;
                    currentState = attack;
                }
            }
            else if (currentBossPhase.Value == 3)
            {
                characterAnimatorManager.PlayTargetActionAnimation(phaseShiftAnimation, true);
                combatStance = Instantiate(phase04CombatStanceState);
                //currentState = combatStance;

                if (!WorldGameSessionManager.Instance.skipPhase4)
                    aICharacterNetworkManager.currentHealth.Value = aICharacterNetworkManager.maxHealth.Value; //retore health
                else
                    aICharacterNetworkManager.currentHealth.Value = 1;

                aICharacterNetworkManager.isDead.Value = false;
                isPerformingAction = false;
                aICharacterCombatManager.actionRecoveryTimer = 0;

                currentBossPhase.Value = currentBossPhase.Value + 1;

                if (bossSpecialPhaseAttack != null)
                {
                    base.attack.currentAttack = bossSpecialPhaseAttack;
                    attack.hasPerformedAttack = false;
                    currentState = attack;
                }
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

        private void OnBossIsCloneValueChanged(bool oldStatus, bool newStatus)
        {
            if (newStatus)
            {
                if (IsOwner)
                {
                    currentState = idle;
                    combatStance = cloneCombatStanceState;

                    aICharacterNetworkManager.currentHealth.Value = 1;
                    aICharacterNetworkManager.maxHealth.Value = 1;

                    aICharacterNetworkManager.CheckHP(1, 1);
                }

                if (IsServer)
                {
                    hasBeenAwakened.Value = true;
                    hasBeenDefeated.Value = true;
                }

                animator.SetBool("IsAwakened", true);
            }
        }
    }
}
