using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Light Attack Action")]
public class LightAttackWeaponItemAction : WeaponItemAction
{
    [SerializeField] string light_Attack_01 = "Main_Light_Attack_01";
    [SerializeField] string light_Attack_02 = "Main_Light_Attack_02";
    [SerializeField] string light_Attack_03 = "Main_Light_Attack_03";

    //Modified attack animations, 2 handed attacks, temp solution of implementing them.
    [SerializeField] string light_Attack_01_M = "Main_Light_Attack_01_M";
    [SerializeField] string light_Attack_02_M = "Main_Light_Attack_02_M";
    [SerializeField] string light_Attack_03_M = "Main_Light_Attack_03_M";

    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

        if (!playerPerformingAction.IsOwner)
            return;

        if (playerPerformingAction.playerNetworkManager.currentHealth.Value <= 0)
            return;

        if (!playerPerformingAction.characterLocomotionManager.isGrounded)
            return;

        //prevent the attack being used when not having enough stamina.
        if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
            return;
        //if (playerPerformingAction.playerNetworkManager.currentStamina.Value < (weaponPerformingAction.baseStaminaCost * weaponPerformingAction.lightAttackStaminaCostMultiplier))
        //    return;

        PerformLightAttack(playerPerformingAction, weaponPerformingAction);
    }

    private void PerformLightAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        if (playerPerformingAction.UseTHAnimations)
        {
            //if we are attacking and are able to perform a combo, perform the combo.
            if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon = false;

                //perform an attack based on the previous attack.
                if (playerPerformingAction.characterCombatManager.lastAttackAnimationPerformed == light_Attack_01_M)
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.LightAttack02, light_Attack_02_M, true);
                }
                else if (playerPerformingAction.characterCombatManager.lastAttackAnimationPerformed == light_Attack_02_M)
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.LightAttack03, light_Attack_03_M, true);
                }
                else //if its null or at the end of the combo, start over.
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.LightAttack01, light_Attack_01_M, true);
                }
            }
            else if (!playerPerformingAction.isPerformingAction) //regular action
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.LightAttack01, light_Attack_01_M, true);
            }
        }
        else
        {
            //if we are attacking and are able to perform a combo, perform the combo.
            if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon = false;

                //perform an attack based on the previous attack.
                if (playerPerformingAction.characterCombatManager.lastAttackAnimationPerformed == light_Attack_01)
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.LightAttack02, light_Attack_02, true);
                }
                else if (playerPerformingAction.characterCombatManager.lastAttackAnimationPerformed == light_Attack_02)
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.LightAttack03, light_Attack_03, true);
                }
                else //if its null or at the end of the combo, start over.
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.LightAttack01, light_Attack_01, true);
                }
            }
            else if (!playerPerformingAction.isPerformingAction) //regular action
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.LightAttack01, light_Attack_01, true);
            }
        }
    }
}
