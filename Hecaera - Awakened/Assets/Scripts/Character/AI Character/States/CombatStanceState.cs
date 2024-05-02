using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Erikduss
{
    [CreateAssetMenu(menuName = "A.I/States/Combat Stance")]
    public class CombatStanceState : AIState
    {
        //1. Select attack for the attack state, depending on distance, angle and likelyness
        //2. Process combat logic while waiting to attack (block, strafe, dodge)
        //3. If target is too far, switch to pursue state
        //4. If target is null, back to idle.
        public bool isStationaryBoss = false;
        public bool canSwitchTargets = true;
        private float targetSwitchChancePerAttack = 25f;

        [Header("Attacks")]
        public List<AICharacterAttackAction> aiCharacterAttacks; //A list of all attacks this character can do.
        protected List<AICharacterAttackAction> potentialAttacks; //possible attacks at the moment based on distance, angle, etc
        private AICharacterAttackAction chosenAttack;
        private AICharacterAttackAction previousAttack;
        protected bool hasAttack = false;

        [Header("Combo")]
        [SerializeField] protected bool canPerformCombo = false;
        [SerializeField] protected int chanceToPerformCombo = 25;
        protected bool hasRolledForComboChance = false;

        [Header("Engagement Distance")]
        [SerializeField] public float maximumEngagementDistance = 5; //The distance we have to be away to enter pursue target state.

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if (aiCharacter.isPerformingAction)
                return this;

            if (!aiCharacter.navMeshAgent.enabled)
                aiCharacter.navMeshAgent.enabled = true;

            //Face and turn towards target.
            if (!aiCharacter.aICharacterNetworkManager.isMoving.Value)
            {
                if (aiCharacter.aICharacterCombatManager.viewableAngle < -30 || aiCharacter.aICharacterCombatManager.viewableAngle > 30)
                {
                    //aiCharacter.aICharacterCombatManager.PivotTowardsTarget(aiCharacter);
                }
            }

            aiCharacter.aICharacterCombatManager.RotateTowardsTarget(aiCharacter);

            if (canSwitchTargets)
            {
                float targetSwitchRoll = Random.Range(0f, 100f);

                if (targetSwitchRoll <= targetSwitchChancePerAttack)
                {
                    //only do this if there are more than 1 player alive.
                    if(WorldGameSessionManager.Instance.players.Count - WorldBossEncounterManager.Instance.amountOfPermaDeadPlayers > 1)
                    {
                        return SwitchState(aiCharacter, aiCharacter.idle);
                    }
                }
            }

            //aiCharacter.aICharacterCombatManager.RotateTowardsAgent(aiCharacter);

            //if we dont have a target anymore, return to idle.
            if (aiCharacter.aICharacterCombatManager.currentTarget == null || aiCharacter.aICharacterCombatManager.currentTarget.characterNetworkManager.isDead.Value)
                return SwitchState(aiCharacter, aiCharacter.idle);

            if (!hasAttack)
            {
                GetNewAttack(aiCharacter);
            }
            else
            {
                aiCharacter.attack.currentAttack = chosenAttack;

                //roll for combo chance
                if (chosenAttack.actionHasComboAction)
                {
                    aiCharacter.attack.willPerformCombo = RollForOutcomeChance(chanceToPerformCombo);
                }

                return SwitchState(aiCharacter, aiCharacter.attack);
            }

            if (!isStationaryBoss)
            {
                if (aiCharacter.aICharacterCombatManager.distanceFromTarget > maximumEngagementDistance)
                    return SwitchState(aiCharacter, aiCharacter.pursueTarget);

                //if we're waiting for recovery timer, we dont just want to stand still
                NavMeshPath path = new NavMeshPath();
                aiCharacter.navMeshAgent.CalculatePath(aiCharacter.aICharacterCombatManager.currentTarget.transform.position, path);
                aiCharacter.navMeshAgent.SetPath(path);
            }

            return this;
        }

        protected virtual void GetNewAttack(AICharacterManager aiCharacter)
        {
            potentialAttacks = new List<AICharacterAttackAction>();

            foreach (var potentialAttack in aiCharacterAttacks)
            {
                //if we're too close, skip this attack.
                if (potentialAttack.minimimAttackDistance > aiCharacter.aICharacterCombatManager.distanceFromTarget)
                    continue;

                //if we're too far, skip this attack
                if (potentialAttack.maximumAttackDistance < aiCharacter.aICharacterCombatManager.distanceFromTarget)
                    continue;

                //if the target is outside of the minimum view, skip this attack
                if (potentialAttack.minimumAttackAngle > aiCharacter.aICharacterCombatManager.viewableAngle)
                    continue;

                //if the target is outside of the maximum view, skip this attack.
                if (potentialAttack.maximumAttackAngle < aiCharacter.aICharacterCombatManager.viewableAngle)
                    continue;

                potentialAttacks.Add(potentialAttack);
            }

            if (potentialAttacks.Count <= 0)
                return;

            //Get a total value of all the possible attacks.
            int totalWeight = 0;
            foreach (AICharacterAttackAction attack in potentialAttacks)
            {
                totalWeight += attack.attackWeight;
            }

            //Pick a random value
            int randomWeightValue = Random.Range(1, totalWeight + 1);
            int processedWeight = 0;

            //Check which attack it will be.
            foreach (AICharacterAttackAction attack in potentialAttacks)
            {
                //Explaination:
                //Every attack has a possibility range between 0 and 100
                //These get added together, for example: 3 attacks, 20 weight, 50 weight and 5 weight
                //This will become: 20+50+5 = 75
                //Any number between 1 and 20 will cause it to choose the first attack
                //Any number between 21 and 70 will choose the second attack
                //Any number between 71 and 75 will be the last attack.

                processedWeight += attack.attackWeight;

                if (randomWeightValue <= processedWeight)
                {
                    chosenAttack = attack;
                    previousAttack = chosenAttack;
                    hasAttack = true;
                    return;
                }
            }
        }

        protected virtual bool RollForOutcomeChance(int outcomeChance)
        {
            bool outcomeWillBePerformed = false;

            int randomPercentage = Random.Range(0, 100);

            if (randomPercentage < outcomeChance)
                outcomeWillBePerformed = true;

            return outcomeWillBePerformed;
        }

        protected override void ResetStateFlags(AICharacterManager aICharacterManager)
        {
            base.ResetStateFlags(aICharacterManager);

            hasAttack = false;
            hasRolledForComboChance = false;
        }
    }
}
