using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike
{
    public class AnimatorManager : MonoBehaviour
    {
        public Animator anim;
        protected CharacterManager characterManager;
        protected CharacterStatsManager characterStatsManager;
        public bool canRotate;

        protected virtual void Awake()
        {
            characterManager = GetComponent<CharacterManager>();
            characterStatsManager = GetComponent<CharacterStatsManager>();
        }

        public void PlayTargetAnimation(string targetAnim, bool isInteracting, bool canRotate = false)
        {
            anim.applyRootMotion = isInteracting;
            anim.SetBool("canRotate", canRotate);
            anim.SetBool("isInteracting", isInteracting);
            anim.CrossFade(targetAnim, 0.2f);
        }

        public void PlayTargetAnimationWithRootRotation(string targetAnim, bool isInteracting)
        {
            anim.applyRootMotion = isInteracting;
            anim.SetBool("isRotatingWithRootMotion", true);
            anim.SetBool("isInteracting", isInteracting);
            anim.CrossFade(targetAnim, 0.2f);
        }

        public virtual void CanRotate()
        {
            anim.SetBool("canRotate", true);
        }

        public virtual void StopRotation()
        {
            anim.SetBool("canRotate", false);
        }

        public virtual void EnableCombo()
        {
            anim.SetBool("canDoCombo", true);
        }

        public virtual void DisableCombo()
        {
            anim.SetBool("canDoCombo", false);
        }

        public virtual void EnableIsInvulnerable()
        {
            anim.SetBool("isInvulnerable", true);
        }

        public virtual void DisableIsInvulnerable()
        {
            anim.SetBool("isInvulnerable", false);
        }

        public virtual void EnableIsParrying()
        {
            characterManager.isParrying = true;
        }

        public virtual void DisableIsParrying()
        {
            characterManager.isParrying = false;
        }

        public virtual void EnableCanbeRiposted()
        {
            characterManager.canBeRiposted = true;
        }

        public virtual void DisableCanbeRiposted()
        {
            characterManager.canBeRiposted = false;
        }

        public virtual void TakeCriticalDamageAnimationEvent()
        {
            characterStatsManager.TakeDamage(characterManager.pendingCriticalDamage, false);
            characterManager.pendingCriticalDamage = 0;
        }
    }
}
