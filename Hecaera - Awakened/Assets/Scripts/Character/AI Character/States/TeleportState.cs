using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    [CreateAssetMenu(menuName = "A.I/States/Teleport")]
    public class TeleportState : AIState
    {
        public string teleportStartAnimation;
        public string teleportEndAnimation;

        public Vector3 teleportDestination;

        private float teleportDelayTimer = 0;
        public float teleportDestinationReachedDelay = 1f;

        private bool teleportStarted = false;
        private bool teleportingBack = false;
        private bool teleportFinished = false;

        //We want to teleport to a location and afterwards return to the combatstance state.
        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if(aiCharacter.isPerformingAction) return this;

            if (teleportFinished)
            {
                return SwitchState(aiCharacter, aiCharacter.combbatStance);
            }
            else
            {
                if (!teleportStarted)
                {
                    teleportStarted = true;
                    teleportDelayTimer = teleportDestinationReachedDelay;
                    //aiCharacter.navMeshAgent.enabled = false;
                    //aiCharacter.GetComponent<CharacterController>().enabled = false;
                    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(teleportStartAnimation, true, false);
                    IxeleceMaterialManagement.Instance.SetIxeleceTeleportMaterial();
                    IxeleceMaterialManagement.Instance.FadeTeleportMaterials(2f, 1f, 0f);
                }
                else if (teleportDelayTimer >= 0 && !aiCharacter.isPerformingAction)
                {
                    teleportDelayTimer -= Time.deltaTime;
                    aiCharacter.transform.position = new Vector3(10,-20,100);
                    aiCharacter.transform.rotation = new Quaternion(0,180,0,0);
                }
                else if(teleportDelayTimer <= 0 && teleportStarted && !teleportingBack)
                {
                    teleportingBack = true;
                    aiCharacter.transform.position = teleportDestination;
                    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(teleportStartAnimation, true, false);
                    IxeleceMaterialManagement.Instance.FadeTeleportMaterials(1f, 0f, 1f);
                }
                else if (!aiCharacter.isPerformingAction && teleportingBack)
                {
                    IxeleceMaterialManagement.Instance.RevertIxeleceMaterial();
                    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Idle", false);

                    teleportFinished = true;
                    //aiCharacter.navMeshAgent.enabled = true;
                    //aiCharacter.GetComponent<CharacterController>().enabled = true;
                    return SwitchState(aiCharacter, aiCharacter.combbatStance);
                }
                return this;
            }
        }
    }
}
