using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    [CreateAssetMenu(menuName = "A.I/States/Idle")]
    public class IdleState : AIState
    {
        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if (aiCharacter.characterCombatManager.currentTarget != null && !aiCharacter.characterCombatManager.currentTarget.characterNetworkManager.isDead.Value)
            {
                //we also need to switch states if the target is dead.
                if (aiCharacter.characterCombatManager.currentTarget.characterNetworkManager.isDead.Value)
                {
                    //return this state to continually search for a target
                    aiCharacter.aICharacterCombatManager.FindATargetViaLineOfSight(aiCharacter);
                    return this;
                }

                return SwitchState(aiCharacter, aiCharacter.pursueTarget);
            }
            else
            {
                //return this state to continually search for a target
                aiCharacter.aICharacterCombatManager.FindATargetViaLineOfSight(aiCharacter);
                return this;
            }
        }
    }
}
