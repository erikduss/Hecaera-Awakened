using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "A.I/States/Pursue Target")]
public class PursueTargetState : AIState
{
    public override AIState Tick(AICharacterManager aiCharacter)
    {
        //If we're performing an action, do nothing until we finish it.
        if (aiCharacter.isPerformingAction)
            return this;

        //If we dont have a target, return to idle.
        if (aiCharacter.aICharacterCombatManager.currentTarget == null)
            return SwitchState(aiCharacter, aiCharacter.idle);

        if (!aiCharacter.navMeshAgent.enabled)
            aiCharacter.navMeshAgent.enabled = true;

        //if our target is outside of our fov, pivot to face them.
        if(aiCharacter.aICharacterCombatManager.viewableAngle < aiCharacter.aICharacterCombatManager.minimumFOV 
            || aiCharacter.aICharacterCombatManager.viewableAngle > aiCharacter.aICharacterCombatManager.maximumFOV)
        {
            aiCharacter.aICharacterCombatManager.PivotTowardsTarget(aiCharacter);
        }

        aiCharacter.aICharacterLocomotionManager.RotateTowardsAgent(aiCharacter);

        //Pursue the target by calculating a new path.
        NavMeshPath path = new NavMeshPath();
        aiCharacter.navMeshAgent.CalculatePath(aiCharacter.aICharacterCombatManager.currentTarget.transform.position, path);
        aiCharacter.navMeshAgent.SetPath(path);

        return this;
    }
}
