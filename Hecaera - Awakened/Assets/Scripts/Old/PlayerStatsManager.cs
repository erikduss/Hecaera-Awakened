using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike
{
    public class PlayerStatsManager : CharacterStatsManager
    {
        PlayerManager playerManager;

        PlayerAnimatorManager playerAnimatorManager;

        public float staminaRegenerationAmount = 1f;
        private float staminaRegenerationTimer = 0;

        private void Awake()
        {
            playerManager = GetComponent<PlayerManager>();
            playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
        }

        void Start()
        {
            maxHealth = SetMaxHealthFromHealthLevel();
            currentHealth = maxHealth;

            maxStamina = SetMaxStaminaFromStaminaLevel();
            currentStamina = maxStamina;

            maxFocusPoints = SetMaxFocusPointsFromFocusLevel();
            currentFocusPoints = maxFocusPoints;
        }

        public override void HandlePoiseResetTimer()
        {
            if (poiseResetTimer > 0)
            {
                poiseResetTimer = poiseResetTimer - Time.deltaTime;
            }
            else if (poiseResetTimer <= 0 && !playerManager.isInteracting)
            {
                totalPoiseDefense = armorPoiseBonus;
            }
        }

        private int SetMaxHealthFromHealthLevel()
        {
            //skill level increase etc things that boost health
            maxHealth = healthLevel * 10;
            return maxHealth;
        }

        private float SetMaxStaminaFromStaminaLevel()
        {
            //skill level increase etc things that boost stamina
            maxStamina = staminaLevel * 10;
            return maxStamina;
        }

        private float SetMaxFocusPointsFromFocusLevel()
        {
            maxFocusPoints = focusLevel * 10;
            return maxFocusPoints;
        }

        public override void TakeDamage(int damage, bool playAnimation, string damageAnimation = "Take Damage")
        {
            if (playerManager.isInvulnerable) return;

            base.TakeDamage(damage, playAnimation, damageAnimation = "Take Damage");

            if (playAnimation) playerAnimatorManager.PlayTargetAnimation(damageAnimation, true);

            if(currentHealth <= 0)
            {
                currentHealth = 0;
                if (playAnimation) playerAnimatorManager.PlayTargetAnimation("Death", true);
                isDead = true;
                //HANDLE PLAYER DEATH
            }
        }

        public void TakeStaminaDamage(int damage)
        {
            currentStamina = currentStamina - damage;
        }

        public void RegenerateStamina()
        {
            if (playerManager.isInteracting)
            {
                staminaRegenerationTimer = 0;
            }
            else
            {
                staminaRegenerationTimer += Time.deltaTime;
                if (currentStamina < maxStamina && staminaRegenerationTimer > 1f)
                {
                    currentStamina += staminaRegenerationAmount * Time.deltaTime;
                }
            }
        }

        public void HealPlayer(int healAmount)
        {
            currentHealth = currentHealth + healAmount;

            if (currentHealth > maxHealth) currentHealth = maxHealth;
        }

        public void DeductFocusPoints(int FP)
        {
            currentFocusPoints = currentFocusPoints - FP;

            if(currentFocusPoints < 0)
            {
                currentFocusPoints = 0;
            }
        }

        public void AddSouls(int souls)
        {
            soulCount = soulCount + souls;
        }
    }
}
