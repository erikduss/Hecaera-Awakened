using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike
{
    public class DamageCollider : MonoBehaviour
    {
        public CharacterManager characterManager;
        Collider damageCollider;
        public bool enabledDamageColliderOnStartup = false;

        [Header("Poise")]
        public float poiseBreak;
        public float offensivePoiseBonus;

        [Header("Damage")]
        public int currentWeaponDamage = 25;

        private void Awake()
        {
            damageCollider = GetComponent<Collider>();
            damageCollider.gameObject.SetActive(true);
            damageCollider.isTrigger = true;
            damageCollider.enabled = enabledDamageColliderOnStartup;
        }

        public void EnableDamageCollider()
        {
            damageCollider.enabled = true;
        }

        public void DisableDamageCollider()
        {
            damageCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider collision)
        {
            if(collision.tag == "Player")
            {
                PlayerStatsManager playerStats = collision.GetComponent<PlayerStatsManager>();
                CharacterManager playerCharacterManager = collision.GetComponent<CharacterManager>();

                if(playerCharacterManager != null)
                {
                    if (playerCharacterManager.isParrying)
                    {
                        //get parried if parryable
                        characterManager.GetComponentInChildren<AnimatorManager>().PlayTargetAnimation("Parried", true);
                        return;
                    }
                }

                if(playerStats != null)
                {
                    playerStats.poiseResetTimer = playerStats.totalPoiseResetTime;
                    playerStats.totalPoiseDefense = playerStats.totalPoiseDefense - poiseBreak;

                    Vector3 contactPoint = collision.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

                    if (playerStats.totalPoiseDefense > poiseBreak)
                    {
                        playerStats.TakeDamage(currentWeaponDamage, false);
                    }
                    else
                    {
                        playerStats.TakeDamage(currentWeaponDamage, true);
                    }
                }
            }

            if(collision.tag == "Enemy")
            {
                CharacterManager enemyCharacterManager = collision.GetComponent<CharacterManager>();

                if (enemyCharacterManager != null)
                {
                    if (enemyCharacterManager.isParrying)
                    {
                        //get parried if parryable
                        characterManager.GetComponentInChildren<AnimatorManager>().PlayTargetAnimation("Parried", true);
                        return;
                    }
                }
            }

            if(collision.tag == "Illusionary Wall")
            {
                IllusionaryWall illusionaryWall = collision.GetComponent<IllusionaryWall>();

                illusionaryWall.wallHasBeenHit = true;
            }
        }
    }
}
