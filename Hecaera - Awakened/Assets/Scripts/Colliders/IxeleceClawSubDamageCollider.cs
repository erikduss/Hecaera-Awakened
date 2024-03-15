using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class IxeleceClawSubDamageCollider : DamageCollider
    {
        public IxeleceClawDamageCollider mainClawDamageCollider;

        protected override void Awake()
        {
            damageCollider = GetComponent<Collider>();

            physicalDamage = mainClawDamageCollider.physicalDamage;
            magicDamage = mainClawDamageCollider.magicDamage;
            fireDamage = mainClawDamageCollider.fireDamage;
            lightningDamage = mainClawDamageCollider.lightningDamage;
            holyDamage = mainClawDamageCollider.holyDamage;
        }

        protected override void DamageTarget(CharacterManager damageTarget)
        {
            //We need to add it to the main damage collider to make sure all sub damage colliders know we already damaged this.
            if (mainClawDamageCollider.charactersDamaged.Contains(damageTarget))
            {
                Debug.Log("We already damaged this character!");
                return;
            }

            mainClawDamageCollider.charactersDamaged.Add(damageTarget);

            base.DamageTarget(damageTarget);
        }
    }
}
