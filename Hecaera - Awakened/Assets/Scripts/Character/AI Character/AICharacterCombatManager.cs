using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Erikduss
{
    public class AICharacterCombatManager : CharacterCombatManager
    {
        [Header("Action Recovery")]
        public float actionRecoveryTimer = 0;

        [Header("Target Information")]
        public bool allowTargetSwitchingWhenAlreadyHavingTarget = true;
        public float distanceFromTarget;
        public float viewableAngle;
        public Vector3 targetsDirection;

        [Header("Detection")]
        [SerializeField] float detectionRadius = 15;
        public float minimumFOV = -35;
        public float maximumFOV = 35;

        [Header("Attack Rotation")]
        public float attackRotationSpeed = 25;

        protected override void Awake()
        {
            base.Awake();

            if (lockOnTransform == null)
                lockOnTransform = GetComponentInChildren<LockOnTransform>().transform;
        }

        public virtual void FindATargetViaLineOfSight(AICharacterManager aiCharacter)
        {
            if (currentTarget != null && !allowTargetSwitchingWhenAlreadyHavingTarget)
                return;

            Collider[] colliders = Physics.OverlapSphere(aiCharacter.transform.position, detectionRadius, WorldUtilityManager.Instance.GetCharacterLayers());

            for (int i = 0; i < colliders.Length; i++)
            {
                CharacterManager targetCharacter = colliders[i].transform.GetComponent<CharacterManager>();

                if (targetCharacter == null) continue;

                if (targetCharacter == aiCharacter) continue;

                if (targetCharacter.characterNetworkManager.isDead.Value) continue;

                //can I attack this character?
                if (WorldUtilityManager.Instance.CanIDamageThisTarget(aiCharacter.characterGroup, targetCharacter.characterGroup))
                {
                    //is the target in our viewable angle?
                    Vector3 targetDirection = targetCharacter.transform.position - aiCharacter.transform.position;
                    float angleOfPotentialTarget = Vector3.Angle(targetDirection, aiCharacter.transform.forward);

                    if (angleOfPotentialTarget > minimumFOV && angleOfPotentialTarget < maximumFOV)
                    {
                        //check for environment blocking view
                        if (Physics.Linecast(aiCharacter.characterCombatManager.lockOnTransform.position, targetCharacter.characterCombatManager.lockOnTransform.position, WorldUtilityManager.Instance.GetEnvironmentLayers()))
                        {
                            Debug.DrawLine(aiCharacter.characterCombatManager.lockOnTransform.position, targetCharacter.characterCombatManager.lockOnTransform.position);
                        }
                        else
                        {
                            targetsDirection = targetCharacter.transform.position - transform.position;
                            viewableAngle = WorldUtilityManager.Instance.GetAngleOfTarget(transform, targetsDirection);

                            aiCharacter.characterCombatManager.SetTarget(targetCharacter);
                            PivotTowardsTarget(aiCharacter);
                        }
                    }
                }
            }
        }

        public void PivotTowardsTarget(AICharacterManager aiCharacter)
        {
            if (aiCharacter.isPerformingAction)
                return;

            //in case getting more animations for turning properly, uncomment below

            //if(viewableAngle >= 20 && viewableAngle <= 60)
            //{
            //    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_45", true);
            //}
            //else if (viewableAngle <= -20 && viewableAngle >= -60)
            //{
            //    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_45", true);
            //}
            //else if(viewableAngle >= 61 && viewableAngle <= 110)
            //{
            //    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_90", true);
            //}
            //else if (viewableAngle <= -61 && viewableAngle >= -110)
            //{
            //    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_90", true);
            //}
            //else if (viewableAngle >= 110 && viewableAngle <= 145)
            //{
            //    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_135", true);
            //}
            //else if (viewableAngle <= -110 && viewableAngle >= -145)
            //{
            //    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_135", true);
            //}
            //else if (viewableAngle >= 146 && viewableAngle <= 180)
            //{
            //    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_180", true);
            //}
            //else if (viewableAngle <= -146 && viewableAngle >= -180)
            //{
            //    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_180", true);
            //}

            if (viewableAngle >= 40 && viewableAngle <= 140)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_90", true);
            }
            else if (viewableAngle <= -40 && viewableAngle >= -140)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_90", true);
            }
            else if (viewableAngle >= 141 && viewableAngle <= 180)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_180", true);
            }
            else if (viewableAngle <= -141 && viewableAngle >= -180)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_180", true);
            }
        }

        public void RotateTowardsTarget(AICharacterManager aICharacter)
        {
            if (currentTarget == null)
            {
                return;
            }

            if (!aICharacter.characterLocomotionManager.canRotate)
            {
                return;
            }

            Vector3 targetDirection = currentTarget.transform.position - aICharacter.transform.position;
            targetDirection.y = 0;
            targetDirection.Normalize();

            if (targetDirection == Vector3.zero)
            {
                targetDirection = aICharacter.transform.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            aICharacter.transform.rotation = Quaternion.Slerp(aICharacter.transform.rotation, targetRotation, attackRotationSpeed * Time.deltaTime);
        }

        public void RotateTowardsAgent(AICharacterManager aICharacter)
        {
            if (aICharacter.aICharacterNetworkManager.isMoving.Value)
            {
                aICharacter.transform.rotation = aICharacter.navMeshAgent.transform.rotation;
            }
        }

        public void RotateTowardsTargetWhilstAttacking(AICharacterManager aICharacter)
        {
            if (currentTarget == null)
            {
                return;
            }

            if (!aICharacter.characterLocomotionManager.canRotate)
            {
                return;
            }

            if (!aICharacter.isPerformingAction)
            {
                return;
            }

            Vector3 targetDirection = currentTarget.transform.position - aICharacter.transform.position;
            targetDirection.y = 0;
            targetDirection.Normalize();

            if (targetDirection == Vector3.zero)
            {
                targetDirection = aICharacter.transform.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            aICharacter.transform.rotation = Quaternion.Slerp(aICharacter.transform.rotation, targetRotation, attackRotationSpeed * Time.deltaTime);
        }

        public void HandleActionRecovery(AICharacterManager aICharacter)
        {
            if (actionRecoveryTimer > 0)
            {
                if (!aICharacter.isPerformingAction)
                {
                    actionRecoveryTimer -= Time.deltaTime;
                }
            }
        }
    }
}
