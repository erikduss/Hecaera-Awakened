using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Erikduss
{
    [CreateAssetMenu(menuName = "A.I/States/Attack")]
    public class AttackState : AIState
    {
        [Header("Current Attack")]
        [HideInInspector] public AICharacterAttackAction currentAttack;
        [HideInInspector] public bool willPerformCombo = false;

        [Header("State Flags")]
        protected bool hasPerformedAttack = false;
        protected bool hasPerformedCombo = false;

        [Header("Pivot After Attack")]
        [SerializeField] protected bool pivotAfterAttack = false;

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if (aiCharacter.aICharacterCombatManager.currentTarget == null)
            {
                return SwitchState(aiCharacter, aiCharacter.idle);
            }

            if (aiCharacter.aICharacterCombatManager.currentTarget.characterNetworkManager.isDead.Value)
            {
                return SwitchState(aiCharacter, aiCharacter.idle);
            }

            //rotate to target while attacking.
            aiCharacter.aICharacterCombatManager.RotateTowardsTargetWhilstAttacking(aiCharacter);

            //set movement values to 0
            aiCharacter.characterAnimatorManager.UpdateAnimatorMovementParameters(0, 0, false);

            if (aiCharacter.isPerformingAction)
            {
                return this;
            }

            if (!hasPerformedAttack)
            {
                //if we are still recovering from an action. Wait
                if (aiCharacter.aICharacterCombatManager.actionRecoveryTimer > 0)
                {
                    return this;
                }

                PerformAttack(aiCharacter);

                //return to the top in case we have a combo coming after this attack.
                return this;
            }

            if (pivotAfterAttack)
                aiCharacter.aICharacterCombatManager.PivotTowardsTarget(aiCharacter);

            //performa combo
            if (willPerformCombo && !hasPerformedCombo)
            {
                if (currentAttack.comboAction != null)
                {
                    //if can combo
                    hasPerformedCombo = true;
                    currentAttack.comboAction.AttemptToPerformAction(aiCharacter);
                }
            }

            return SwitchState(aiCharacter, aiCharacter.combbatStance);
        }

        protected void PerformAttack(AICharacterManager aiCharacter)
        {
            hasPerformedAttack = true;
            currentAttack.AttemptToPerformAction(aiCharacter);
            aiCharacter.aICharacterCombatManager.actionRecoveryTimer = currentAttack.actionRecoveryTime;
        }

        protected override void ResetStateFlags(AICharacterManager aICharacterManager)
        {
            base.ResetStateFlags(aICharacterManager);

            hasPerformedAttack = false;
            hasPerformedCombo = false;
            willPerformCombo = false;
        }
    }
}
