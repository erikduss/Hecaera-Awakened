using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class DamageCollider : MonoBehaviour
    {
        [Header("Collider")]
        [SerializeField] protected Collider damageCollider;

        [Header("Damage")]
        public float physicalDamage = 0;
        public float magicDamage = 0;
        public float fireDamage = 0;
        public float lightningDamage = 0;
        public float holyDamage = 0;

        [Header("Contact Point")]
        protected Vector3 contactPoint;

        [Header("Characters Damaged")]
        public bool canDamageSameCharacterMultipleTimes = false;
        public List<CharacterManager> charactersDamaged = new List<CharacterManager>();
        public CharacterGroup groupOfAttack = CharacterGroup.NONE;

        private float damageDelayTimer = 0;
        private float damageTickDelay = 0.5f;

        protected virtual void Awake()
        {

        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (!canDamageSameCharacterMultipleTimes) return;

            OnTriggerEnter(other);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            //This is a damageable bodypart.
            //For now, only the boss has this, if in the future adding more of these.
            //Make sure to add a check to see in which team this parent's character is and which team the other character is in.
            if (gameObject.tag == "DamageableDamageCollider")
            {
                //We hit our own collider
                if (other.tag == "DamageableDamageCollider") return;
            }

            //If the other collider is a damage collider instead of a damageable character
            if (other.gameObject.layer == WorldUtilityManager.Instance.GetDamageCollidertLayer())
            {
                //if we did hit a damage collider, we can only continue if this is tagged as a DamageableDamageCollider
                if (other.tag != "DamageableDamageCollider") return;
            }

            CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();

            if(damageTarget != null)
            {
                if (damageTarget.characterGroup == groupOfAttack) return;
            }

            if (damageTarget != null && !damageTarget.characterNetworkManager.isInvincible.Value)
            {
                contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

                DamageTarget(damageTarget);
            }
        }

        protected virtual void DamageTarget(CharacterManager damageTarget)
        {
            if (charactersDamaged.Contains(damageTarget) && !canDamageSameCharacterMultipleTimes)
                return;

            if (canDamageSameCharacterMultipleTimes)
            {
                if(damageDelayTimer > 0)
                {
                    damageDelayTimer -= Time.deltaTime;
                    return;
                }
            }

            charactersDamaged.Add(damageTarget);

            //if this is an attack from group 2, the multiplier is equal to the boss empower multiplier. Otherwise its 1 and wont do anything.
            float multiplier = groupOfAttack == CharacterGroup.Team02 ? WorldBossEncounterManager.Instance.bossEmpowerDamageMultiplier.Value : 1.0f;

            TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeDamageEffect);
            damageEffect.physicalDamage = physicalDamage * multiplier;
            damageEffect.magicDamage = magicDamage * multiplier;
            damageEffect.fireDamage = fireDamage * multiplier;
            damageEffect.holyDamage = holyDamage * multiplier;
            damageEffect.contactPoint = contactPoint;

            damageEffect.playDamageAnimation = !canDamageSameCharacterMultipleTimes;
            damageEffect.willPlayDamageSFX = !canDamageSameCharacterMultipleTimes;

            damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);

            if (canDamageSameCharacterMultipleTimes) damageDelayTimer = damageTickDelay;
        }

        public virtual void EnableDamageCollider()
        {
            damageCollider.enabled = true;
        }

        public virtual void DisableDamageCollider()
        {
            damageCollider.enabled = false;
            charactersDamaged.Clear();
        }
    }
}
