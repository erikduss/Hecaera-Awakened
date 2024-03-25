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
