using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class ProjectileDamageCollider : DamageCollider
    {
        [Header("Attacking Character")]
        public CharacterManager characterCausingDamage; //when calculating damage, this is used to process buffs etc into calculation

        [Header("Projectile Attack Modifiers")]
        public float instant_Magic_Attack_Modifier;

        protected override void Awake()
        {
            base.Awake();

            if (damageCollider == null)
            {
                damageCollider = GetComponent<Collider>();
            }

            damageCollider.enabled = false; //damage collider should only be enabled during attack animations.

            //prevent the attack not dealing damage
            if(instant_Magic_Attack_Modifier == 0) instant_Magic_Attack_Modifier = 1;
        }


        protected override void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == WorldUtilityManager.Instance.GetEnvironmentLayers())
            {
                Debug.Log("PROJECTILE HIT ENVIRONMENT");
                return;
            }

            CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();

            if (damageTarget != null && !damageTarget.characterNetworkManager.isInvincible.Value)
            {
                //dont damage yourself
                if (damageTarget == characterCausingDamage)
                    return;

                if (!WorldGameSessionManager.Instance.PVPEnabled)
                {
                    //Attack coming from NPC
                    if(characterCausingDamage == null)
                    {
                        if (damageTarget.characterGroup == groupOfAttack)
                            return;
                    }
                    else if (damageTarget.characterGroup == characterCausingDamage.characterGroup)
                        return;
                }

                contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

                DamageTarget(damageTarget);
            }
        }

        protected override void DamageTarget(CharacterManager damageTarget)
        {
            if (charactersDamaged.Contains(damageTarget))
                return;

            charactersDamaged.Add(damageTarget);

            TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeDamageEffect);
            damageEffect.physicalDamage = physicalDamage;
            damageEffect.magicDamage = magicDamage;
            damageEffect.fireDamage = fireDamage;
            damageEffect.holyDamage = holyDamage;
            damageEffect.contactPoint = contactPoint;

            if (characterCausingDamage != null)
                damageEffect.angleHitFrom = Vector3.SignedAngle(characterCausingDamage.transform.forward, damageTarget.transform.forward, Vector3.up);
            else
                damageEffect.angleHitFrom = Vector3.SignedAngle(damageTarget.transform.up, damageTarget.transform.forward, Vector3.up);

            if(characterCausingDamage != null)
            {
                switch (characterCausingDamage.characterCombatManager.currentAttackType)
                {
                    case AttackType.InstantMagicAttack01:
                        ApplyAttackDamageModifiers(instant_Magic_Attack_Modifier, damageEffect);
                        break;
                    default:
                        ApplyAttackDamageModifiers(instant_Magic_Attack_Modifier, damageEffect);
                        break;
                }
            }
            else
            {
                ApplyAttackDamageModifiers(instant_Magic_Attack_Modifier, damageEffect);
            }

            Debug.Log("Dealing: " + damageEffect.physicalDamage + " Damage");

            //damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);

            if (characterCausingDamage.IsOwner)
            {
                damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                    damageTarget.NetworkObjectId,
                    characterCausingDamage.NetworkObjectId,
                    damageEffect.physicalDamage,
                    damageEffect.magicDamage,
                    damageEffect.fireDamage,
                    damageEffect.holyDamage,
                    damageEffect.poiseDamage,
                    damageEffect.angleHitFrom,
                    damageEffect.contactPoint.x,
                    damageEffect.contactPoint.y,
                    damageEffect.contactPoint.z);
            }
        }

        private void ApplyAttackDamageModifiers(float modifier, TakeDamageEffect damage)
        {
            damage.physicalDamage *= modifier;
            damage.magicDamage *= modifier;
            damage.fireDamage *= modifier;
            damage.holyDamage *= modifier;
            damage.poiseDamage *= modifier;


        }
    }
}
