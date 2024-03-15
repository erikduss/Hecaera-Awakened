using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class IxeleceClawDamageCollider : DamageCollider
    {
        [SerializeField] AIBossCharacterManager bossCharacter;

        [SerializeField] List<IxeleceClawSubDamageCollider> subColliders = new List<IxeleceClawSubDamageCollider>();

        protected override void Awake()
        {
            base.Awake();

            damageCollider = GetComponent<Collider>();
            bossCharacter = GetComponentInParent<AIBossCharacterManager>();
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
            damageEffect.angleHitFrom = Vector3.SignedAngle(bossCharacter.transform.forward, damageTarget.transform.forward, Vector3.up);

            Debug.Log("Dealing: " + damageEffect.physicalDamage + " Damage");

            //use aiCharacterCausingDamage.IsOwner to check from server.
            //use damageTarget.IsOwner too check from the person being damaged.
            //using checking for damage from the player that's damaged to hopefully improve experience on higher ping.
            if (damageTarget.IsOwner)
            {
                damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                    damageTarget.NetworkObjectId,
                    bossCharacter.NetworkObjectId,
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

        public override void EnableDamageCollider()
        {
            foreach (IxeleceClawSubDamageCollider col in subColliders)
            {
                col.EnableDamageCollider();
            }

            base.EnableDamageCollider();
        }

        public override void DisableDamageCollider()
        {
            foreach (IxeleceClawSubDamageCollider col in subColliders)
            {
                col.DisableDamageCollider();
            }

            base.DisableDamageCollider();
        }
    }
}
