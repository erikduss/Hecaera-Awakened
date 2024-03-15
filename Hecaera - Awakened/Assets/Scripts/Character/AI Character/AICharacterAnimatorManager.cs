using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class AICharacterAnimatorManager : CharacterAnimatorManager
    {
        AICharacterManager aiCharacter;

        protected override void Awake()
        {
            base.Awake();

            aiCharacter = GetComponent<AICharacterManager>();
        }

        private void OnAnimatorMove()
        {
            //host
            if (aiCharacter.IsOwner)
            {
                if (!aiCharacter.characterLocomotionManager.isGrounded)
                {
                    return;
                }

                Vector3 velocity = aiCharacter.animator.deltaPosition;

                //sync the position and rotation to the active animation (sometimes gets buggy due to root motion)
                aiCharacter.characterController.Move(velocity);
                aiCharacter.transform.rotation *= aiCharacter.animator.deltaRotation;
            }
            //client
            else
            {
                if (!aiCharacter.characterLocomotionManager.isGrounded)
                {
                    return;
                }

                Vector3 velocity = aiCharacter.animator.deltaPosition;

                //sync the position and rotation to the active animation (sometimes gets buggy due to root motion)
                aiCharacter.characterController.Move(velocity);
                aiCharacter.transform.position = Vector3.SmoothDamp(transform.position,
                    aiCharacter.characterNetworkManager.networkPosition.Value,
                    ref aiCharacter.characterNetworkManager.networkPositionVelocity,
                    aiCharacter.characterNetworkManager.networkPositionSmoothTime);
                aiCharacter.transform.rotation *= aiCharacter.animator.deltaRotation;
            }
        }
    }
}
