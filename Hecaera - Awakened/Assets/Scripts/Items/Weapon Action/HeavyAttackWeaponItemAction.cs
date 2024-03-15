using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    [CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Heavy Attack Action")]
    public class HeavyAttackWeaponItemAction : WeaponItemAction
    {
        [SerializeField] string heavy_Attack_01 = "Main_Heavy_Attack_01";
        [SerializeField] string heavy_Attack_02 = "Main_Heavy_Attack_02";

        [SerializeField] string heavy_Jump_Attack_01 = "Main_Jump_Land_Attack_Hold_01";

        public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

            if (!playerPerformingAction.IsOwner)
                return;

            if (playerPerformingAction.playerNetworkManager.currentHealth.Value <= 0)
                return;

            //prevent the attack being used when not having enough stamina.
            if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
                return;
            //if (playerPerformingAction.playerNetworkManager.currentStamina.Value < (weaponPerformingAction.baseStaminaCost * weaponPerformingAction.heavyAttackStaminaCostMultiplier))
            //    return;

            if (!playerPerformingAction.characterLocomotionManager.isGrounded)
            {
                if (!playerPerformingAction.isPerformingAction)
                    PerformHeavyJumpAttack(playerPerformingAction, weaponPerformingAction);
            }
            else
            {
                PerformHeavyAttack(playerPerformingAction, weaponPerformingAction);
            }
        }

        private void PerformHeavyJumpAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            //we currently only have a light attack jump attack. So we see this attack as a light attack for now.
            playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.LightJumpAttack01, heavy_Jump_Attack_01, true, false, true);
            playerPerformingAction.characterLocomotionManager.currentGravityForce = playerPerformingAction.characterLocomotionManager.gravityForce * 3;
        }

        private void PerformHeavyAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            //if we are attacking and are able to perform a combo, perform the combo.
            if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon = false;

                //perform an attack based on the previous attack.
                if (playerPerformingAction.characterCombatManager.lastAttackAnimationPerformed == heavy_Attack_01)
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.HeavyAttack02, heavy_Attack_02, true);
                }
                //If we did a jump attack, we skip the first attack in the combo
                else if (playerPerformingAction.characterCombatManager.lastAttackAnimationPerformed == heavy_Jump_Attack_01)
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.HeavyAttack02, heavy_Attack_02, true);
                }
                else //if its null or at the end of the combo, start over.
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.HeavyAttack01, heavy_Attack_01, true);
                }
            }
            else if (!playerPerformingAction.isPerformingAction) //regular action
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.HeavyAttack01, heavy_Attack_01, true);
            }
        }
    }
}
