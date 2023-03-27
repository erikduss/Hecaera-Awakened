using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike
{
    public class PlayerCombatManager : MonoBehaviour
    {
        CameraHandler cameraHandler;
        PlayerAnimatorManager playerAnimatorManager;
        PlayerManager playerManager;
        PlayerStatsManager playerStatsManager;
        InputHandler inputHandler;

        public string lastAttack;

        LayerMask backStabLayer = 1 << 14;
        LayerMask riposteLayer = 1 << 15;
        private void Awake()
        {
            playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
            cameraHandler = FindObjectOfType<CameraHandler>();
            playerManager = GetComponent<PlayerManager>();
            playerStatsManager = GetComponent<PlayerStatsManager>();
            inputHandler = GetComponent<InputHandler>();
        }

        public void HandleWeaponCombo()
        {
            if (playerStatsManager.currentStamina <= 0)
            {
                return;
            }

            if (inputHandler.comboFlag)
            {
                playerAnimatorManager.anim.SetBool("canDoCombo", false);

            }
        }

        public void HandleLightAttack()
        {
            if (playerStatsManager.currentStamina <= 0)
            {
                return;
            }
        }

        public void HandleHeavyAttack()
        {
        }

        #region Input Actions
        public void HandleRBAction()
        {
        }

        public void HandleRTAction()
        {
        }

        public void HandleLBAction()
        {
            PerformLBBlockingAction();
        }

        public void HandleLTAction()
        {
        }
        #endregion

        #region Attack Actions
        private void PerformRBMeleeAction()
        {
        }

        private void PerformRTMeleeAction()
        {
            if (playerManager.canDoCombo)
            {
                inputHandler.comboFlag = true;
                inputHandler.comboFlag = false;
            }
            else
            {
                if (playerManager.isInteracting)
                {
                    return;
                }
                if (playerManager.canDoCombo)
                {
                    return;
                }
                playerAnimatorManager.anim.SetBool("isUsingRightHand", true);
            }
        }

        private void PerformRBMagicAction()
        {
            
        }

        private void PerformLTWeaponArt(bool isTwoHanding)
        {
            if (playerManager.isInteracting) return;

            if (isTwoHanding)
            {
                //if we are 2handing perform weapon art for the right weapon
            }
            else
            {
            }
        }

        private void SuccessfullyCastSpell()
        {
            playerAnimatorManager.anim.SetBool("isFiringSpell", true);
        }
        #endregion

        #region Defense Actions
        private void PerformLBBlockingAction()
        {
            if (playerManager.isInteracting)
            {
                return;
            }

            if (playerManager.isBlocking) return;

            playerAnimatorManager.PlayTargetAnimation("Block_Start", false, true);
            playerManager.isBlocking = true;
        }
        #endregion

        public void AttemptBackStabOrRiposte()
        {
            if (playerStatsManager.currentStamina <= 0)
            {
                return;
            }

            RaycastHit hit;

            if(Physics.Raycast(inputHandler.criticalAttackRayCastStartPoint.position, transform.TransformDirection(Vector3.forward), out hit, 0.5f, backStabLayer))
            {
                //Check for team ID
                CharacterManager enemyCharacterManager = hit.transform.gameObject.GetComponentInParent<CharacterManager>();

                if(enemyCharacterManager != null)
                {
                    playerManager.transform.position = enemyCharacterManager.backStabCollider.criticalDamagerStandPosition.position;
                    Vector3 rotationDirection = playerManager.transform.root.eulerAngles;
                    rotationDirection = hit.transform.position - playerManager.transform.position;
                    rotationDirection.y = 0;
                    rotationDirection.Normalize();
                    Quaternion tr = Quaternion.LookRotation(rotationDirection);
                    Quaternion targetRotation = Quaternion.Slerp(playerManager.transform.rotation, tr, 500 * Time.deltaTime);
                    playerManager.transform.rotation = targetRotation;

                    playerAnimatorManager.PlayTargetAnimation("Back Stab", true);
                    enemyCharacterManager.GetComponentInChildren<AnimatorManager>().PlayTargetAnimationWithRootRotation("Back Stabbed", true);
                }
            }
            else if (Physics.Raycast(inputHandler.criticalAttackRayCastStartPoint.position, transform.TransformDirection(Vector3.forward), out hit, 1f, riposteLayer))
            {
                //Check for team ID
                CharacterManager enemyCharacterManager = hit.transform.gameObject.GetComponentInParent<CharacterManager>();

                if(enemyCharacterManager != null && enemyCharacterManager.canBeRiposted)
                {
                    playerManager.transform.position = enemyCharacterManager.riposteCollider.criticalDamagerStandPosition.position;

                    Vector3 rotationDirection = playerManager.transform.root.eulerAngles;
                    rotationDirection = hit.transform.position - playerManager.transform.position;
                    rotationDirection.y = 0;
                    rotationDirection.Normalize();
                    Quaternion tr = Quaternion.LookRotation(rotationDirection);
                    Quaternion targetRotation = Quaternion.Slerp(playerManager.transform.rotation, tr, 500 * Time.deltaTime);
                    playerManager.transform.rotation = targetRotation;

                    playerAnimatorManager.PlayTargetAnimation("Riposte", true);
                    enemyCharacterManager.GetComponentInChildren<AnimatorManager>().PlayTargetAnimation("Riposte_Stabbed", true);
                }
            }
        }
    }
}
