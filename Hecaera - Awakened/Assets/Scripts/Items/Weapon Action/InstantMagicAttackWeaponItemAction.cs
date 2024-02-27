using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Instant Magic Attack Action")]
public class InstantMagicAttackWeaponItemAction : WeaponItemAction
{
    [SerializeField] string instant_Magic_Attack_01 = "Main_Instant_Magic_Attack_01";

    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

        if (!playerPerformingAction.IsOwner)
            return;

        if (playerPerformingAction.playerNetworkManager.currentHealth.Value <= 0)
            return;

        if (!playerPerformingAction.characterLocomotionManager.isGrounded)
            return;

        PerformInstantMagicAttack(playerPerformingAction, weaponPerformingAction);
    }

    private void PerformInstantMagicAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        if (playerPerformingAction.playerNetworkManager.isUsingRightHand.Value)
        {
            playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(AttackType.InstantMagicAttack01, instant_Magic_Attack_01, true, false, true, true);
        }
        if (playerPerformingAction.playerNetworkManager.isUsingLeftHand.Value)
        {

        }
    }
}
