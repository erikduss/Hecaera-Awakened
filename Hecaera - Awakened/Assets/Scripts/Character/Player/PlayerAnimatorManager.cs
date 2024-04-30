using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class PlayerAnimatorManager : CharacterAnimatorManager
    {
        PlayerManager player;

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
            player.animator.SetBool("UseTHAnimations", player.UseTHAnimations);
        }

        private void OnAnimatorMove()
        {
            if (player.characterAnimatorManager.applyRootMotion)
            {
                Vector3 velocity = player.animator.deltaPosition;

                if(player.characterController.enabled)
                    player.characterController.Move(velocity);

                player.transform.rotation *= player.animator.deltaRotation;
            }
        }

        //animation event calls
        public override void EnableCanDoCombo()
        {
            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                player.playerCombatManager.canComboWithMainHandWeapon = true;
            }
        }

        public override void DisableCanDoCombo()
        {
            player.playerCombatManager.canComboWithMainHandWeapon = false;
        }
    }
}
