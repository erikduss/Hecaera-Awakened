using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Erikduss
{
    public class CharacterAnimatorManager : MonoBehaviour
    {
        CharacterManager character;

        int vertical;
        int horizontal;

        [Header("Flags")]
        public bool applyRootMotion = false;

        [Header("Damage Animations")]
        public string lastDamageAnimationPlayed;

        [SerializeField] private string hit_Forward_Medium_01 = "Hit_Forward_Medium_01";
        [SerializeField] private string hit_Forward_Medium_02 = "Hit_Forward_Medium_02";
        [SerializeField] private string hit_Forward_Medium_03 = "Hit_Forward_Medium_03";

        [SerializeField] private string hit_Backward_Medium_01 = "Hit_Backward_Medium_01";
        [SerializeField] private string hit_Backward_Medium_02 = "Hit_Backward_Medium_02";
        [SerializeField] private string hit_Backward_Medium_03 = "Hit_Backward_Medium_03";

        [SerializeField] private string hit_Left_Medium_01 = "Hit_Left_Medium_01";
        [SerializeField] private string hit_Left_Medium_02 = "Hit_Left_Medium_02";
        [SerializeField] private string hit_Left_Medium_03 = "Hit_Left_Medium_03";

        [SerializeField] private string hit_Right_Medium_01 = "Hit_Right_Medium_01";
        [SerializeField] private string hit_Right_Medium_02 = "Hit_Right_Medium_02";
        [SerializeField] private string hit_Right_Medium_03 = "Hit_Right_Medium_03";

        public List<string> forward_Medium_Damage = new List<string>();
        public List<string> backward_Medium_Damage = new List<string>();
        public List<string> left_Medium_Damage = new List<string>();
        public List<string> right_Medium_Damage = new List<string>();

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();

            vertical = Animator.StringToHash("Vertical");
            horizontal = Animator.StringToHash("Horizontal");
        }

        protected virtual void Start()
        {
            forward_Medium_Damage.Add(hit_Forward_Medium_01);
            forward_Medium_Damage.Add(hit_Forward_Medium_02);
            forward_Medium_Damage.Add(hit_Forward_Medium_03);

            backward_Medium_Damage.Add(hit_Backward_Medium_01);
            backward_Medium_Damage.Add(hit_Backward_Medium_02);
            backward_Medium_Damage.Add(hit_Backward_Medium_03);

            left_Medium_Damage.Add(hit_Left_Medium_01);
            left_Medium_Damage.Add(hit_Left_Medium_02);
            left_Medium_Damage.Add(hit_Left_Medium_03);

            right_Medium_Damage.Add(hit_Right_Medium_01);
            right_Medium_Damage.Add(hit_Right_Medium_02);
            right_Medium_Damage.Add(hit_Right_Medium_03);
        }

        public string GetRandomAnimationFromList(List<string> animationList)
        {
            List<string> finalList = new List<string>();

            foreach (string animation in animationList)
            {
                finalList.Add(animation);
            }

            //Remove last played animation so we dont pick it again.
            finalList.Remove(lastDamageAnimationPlayed);

            //check the list for null entries.
            for (int i = finalList.Count - 1; i > -1; i--)
            {
                if (finalList[i] == null)
                {
                    finalList.RemoveAt(i);
                }
            }

            int randomValue = Random.Range(0, finalList.Count);

            return finalList[randomValue];
        }

        public void UpdateAnimatorMovementParameters(float horizontalValue, float verticalValue, bool isSprinting)
        {
            float snappedHorizontal;
            float snappedVertical;

            //Snap horizontal values
            if (horizontalValue > 0 && horizontalValue <= 0.5f)
            {
                snappedHorizontal = 0.5f;
            }
            else if (horizontalValue > 0.5f && horizontalValue <= 1f)
            {
                snappedHorizontal = 1f;
            }
            else if (horizontalValue < 0 && horizontalValue >= -0.5f)
            {
                snappedHorizontal = -0.5f;
            }
            else if (horizontalValue < -0.5f && horizontalValue >= -1f)
            {
                snappedHorizontal = -1f;
            }
            else
            {
                snappedHorizontal = 0f;
            }

            //Snap vertical values
            if (verticalValue > 0 && verticalValue <= 0.5f)
            {
                snappedVertical = 0.5f;
            }
            else if (verticalValue > 0.5f && verticalValue <= 1f)
            {
                snappedVertical = 1f;
            }
            else if (verticalValue < 0 && verticalValue >= -0.5f)
            {
                snappedVertical = -0.5f;
            }
            else if (verticalValue < -0.5f && verticalValue >= -1f)
            {
                snappedVertical = -1f;
            }
            else
            {
                snappedVertical = 0f;
            }


            if (isSprinting)
            {
                snappedVertical = 2;
            }

            character.animator.SetFloat(horizontal, snappedHorizontal, 0.1f, Time.deltaTime);
            character.animator.SetFloat(vertical, snappedVertical, 0.1f, Time.deltaTime);
        }

        public virtual void PlayTargetActionAnimation(
            string targetAnimation,
            bool isPerformingAction,
            bool applyRootMotion = true,
            bool canRotate = false,
            bool canMove = false)
        {

            //if the animation string is empty it will break the isperformingaction and rotate rules.
            if (targetAnimation.Length <= 1) return;

            character.characterAnimatorManager.applyRootMotion = applyRootMotion;
            character.animator.CrossFade(targetAnimation, 0.2f);

            //Can be used to stop character from performing new actions.
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;

            Debug.Log(targetAnimation);

            //Tell the server/host we played an animation and to play that animation.
            character.characterNetworkManager.NotifyTheServerOfActionAnimationServerRpc(NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion);
        }

        public virtual void PlayTargetAttackActionAnimation(AttackType attackType,
            string targetAnimation,
            bool isPerformingAction,
            bool applyRootMotion = true,
            bool canRotate = false,
            bool canMove = false)
        {
            character.characterCombatManager.currentAttackType = attackType;
            character.characterCombatManager.lastAttackAnimationPerformed = targetAnimation;
            character.characterAnimatorManager.applyRootMotion = applyRootMotion;
            character.animator.CrossFade(targetAnimation, 0.2f);
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;

            //Tell the server/host we played an animation and to play that animation.
            character.characterNetworkManager.NotifyTheServerOfAttackActionAnimationServerRpc(NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion);
        }

        public virtual void EnableCanDoCombo()
        {

        }

        public virtual void DisableCanDoCombo()
        {

        }
    }
}
