using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class AIIxeleceCombatManager : AICharacterCombatManager
    {
        [Header("Damage Colliders")]
        [SerializeField] IxeleceClawDamageCollider rightClawDamageCollider;
        [SerializeField] IxeleceClawDamageCollider leftClawDamageCollider;

        [Header("Damage")]
        [SerializeField] int baseDamage = 25;
        [SerializeField] float attack01DamageModifier = 1f;
        [SerializeField] float attack02DamageModifier = 1.4f;

        public override void FindATargetViaLineOfSight(AICharacterManager aiCharacter)
        {
            base.FindATargetViaLineOfSight(aiCharacter);

            if(currentTarget == null || allowTargetSwitchingWhenAlreadyHavingTarget)
            {
                SwitchToNewTarget();
            }

            //Possibly fixes issue with boss being stuck in isperforming when the player is behind the boss during teleport.
            aiCharacter.isPerformingAction = false;
        }

        public void SwitchToNewTarget()
        {
            //Change this to a more reliable target switching option
            //Based on the amount of players
            //Wont target the same player the whole fight
            //Likelyness to target per player

            List<PlayerManager> alivePlayers = new List<PlayerManager>();

            foreach(PlayerManager player in WorldGameSessionManager.Instance.players)
            {
                if(!player.characterNetworkManager.isDead.Value) alivePlayers.Add(player);
            }

            int playerToTarget = Random.Range(0, alivePlayers.Count);

            if(alivePlayers.Count > 0)
                SetTarget(alivePlayers[playerToTarget]);
            //currentTarget = alivePlayers[playerToTarget];
        }

        public void SetAttack01Damage()
        {
            rightClawDamageCollider.physicalDamage = baseDamage * attack01DamageModifier;
            leftClawDamageCollider.physicalDamage = baseDamage * attack01DamageModifier;
        }
        public void SetAttack02Damage()
        {
            rightClawDamageCollider.physicalDamage = baseDamage * attack02DamageModifier;
            leftClawDamageCollider.physicalDamage = baseDamage * attack02DamageModifier;
        }

        public void SetSunbeamDamage()
        {
            rightClawDamageCollider.physicalDamage = baseDamage * attack01DamageModifier;
            leftClawDamageCollider.physicalDamage = baseDamage * attack01DamageModifier;
        }

        public void OpenRightClawDamageCollider()
        {
            rightClawDamageCollider.EnableDamageCollider();
        }

        public void CloseRightClawDamageCollider()
        {
            rightClawDamageCollider.DisableDamageCollider();
        }

        public void OpenLeftClawDamageCollider()
        {
            leftClawDamageCollider.EnableDamageCollider();
        }

        public void CloseLeftClawDamageCollider()
        {
            leftClawDamageCollider.DisableDamageCollider();
        }
    }
}
