using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class PlayerCombatManager : CharacterCombatManager
    {
        PlayerManager player;

        public WeaponItem currentWeaponBeingUsed;

        [Header("Flags")]
        public bool canComboWithMainHandWeapon = false;

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        public void PerformWeaponBasedAction(WeaponItemAction weaponAction, WeaponItem weaponPerformingAction)
        {
            if (player.IsOwner)
            {
                weaponAction.AttemptToPerformAction(player, weaponPerformingAction);

                //if (weaponAction.pooledObjectType != PooledObjectType.NONE)
                //{
                //    WorldProjectilesManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkManager.Singleton.LocalClientId, (int)weaponAction.pooledObjectType, weaponAction.objectSpawnDelay);
                //}

                player.playerNetworkManager.NotifyTheServerOfWeaponActionServerRpc(NetworkManager.Singleton.LocalClientId, weaponAction.actionID, weaponPerformingAction.itemID);
            }
        }

        public void PerformUseItemAction(HealingQuickUseItem healingItemToUse)
        {
            if (player.IsOwner)
            {
                if (player.playerInventoryManager.currentAmountOfHealingItems <= 0) return;

                if (player.characterNetworkManager.isDead.Value) return;

                if (healingItemToUse.useAnimationForActivation)
                {
                    player.playerAnimatorManager.PlayTargetActionAnimation(healingItemToUse.onUseAnimation, true, false, true, true);
                }

                player.playerInventoryManager.currentAmountOfHealingItems--;
                PlayerUIManager.instance.playerUIHudManager.amountOfAvailabeHealingItemsText.text = player.playerInventoryManager.currentAmountOfHealingItems + "x";
                PlayerUIManager.instance.playerUIHudManager.amountOfAvailabeHealingItemsBackgroundText.text = player.playerInventoryManager.currentAmountOfHealingItems + "x";

                if (player.playerInventoryManager.currentAmountOfHealingItems <= 0) PlayerUIManager.instance.playerUIHudManager.healingItemImage.color = Color.red;

                StartCoroutine(HealingDelay(1f));

                //weaponAction.AttemptToPerformAction(player, weaponPerformingAction);

                //if (weaponAction.pooledObjectType != PooledObjectType.NONE)
                //{
                //    WorldProjectilesManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkManager.Singleton.LocalClientId, (int)weaponAction.pooledObjectType, weaponAction.objectSpawnDelay);
                //}

                //player.playerNetworkManager.NotifyTheServerOfWeaponActionServerRpc(NetworkManager.Singleton.LocalClientId, weaponAction.actionID, weaponPerformingAction.itemID);
            }
        }

        private IEnumerator HealingDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if(!player.characterNetworkManager.isDead.Value)
            {
                player.playerNetworkManager.CheckHP(player.playerNetworkManager.currentHealth.Value, player.playerNetworkManager.maxHealth.Value);
                player.playerNetworkManager.currentHealth.Value = player.playerNetworkManager.maxHealth.Value;
            }
        }

        public virtual void DrainStaminaBasedOnAttack()
        {
            if (!player.IsOwner)
                return;

            if (currentWeaponBeingUsed == null)
                return;

            float staminaDeducted = 0;

            switch (currentAttackType)
            {
                case AttackType.LightAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.lightAttackStaminaCostMultiplier;
                    break;
                case AttackType.LightAttack02:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.lightAttackStaminaCostMultiplier;
                    break;
                case AttackType.LightAttack03:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.lightAttackStaminaCostMultiplier;
                    break;
                case AttackType.HeavyAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.heavyAttackStaminaCostMultiplier;
                    break;
                case AttackType.HeavyAttack02:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.heavyAttackStaminaCostMultiplier;
                    break;
                case AttackType.ChargedAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.heavyAttackStaminaCostMultiplier;
                    break;
                case AttackType.InstantMagicAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.instantMagicAttackStaminaCostMultiplier;
                    break;
                case AttackType.LightJumpAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.lightAttackStaminaCostMultiplier;
                    break;
                default:
                    Debug.LogError("ATTACK TYPE MODIFIER IS NOT IMPLEMENTED");
                    break;
            }

            player.playerNetworkManager.currentStamina.Value -= Mathf.RoundToInt(staminaDeducted);
        }

        public override void SetTarget(CharacterManager newTarget)
        {
            base.SetTarget(newTarget);

            if (player.IsOwner)
            {
                PlayerCamera.instance.SetLockCameraHeight();
            }
        }
    }
}
