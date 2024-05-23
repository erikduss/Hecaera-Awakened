using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class PlayerInventoryManager : CharacterInventoryManager
    {
        public WeaponItem currentRightHandWeapon;
        public WeaponItem currentLeftHandWeapon;

        [Header("Quick Slots")]
        public WeaponItem[] weaponsInRightHandSlots = new WeaponItem[3];
        public int rightHandWeaponIndex = 0;
        public WeaponItem[] weaponsInLeftHandSlots = new WeaponItem[3];
        public int leftHandWeaponIndex = 0;

        public HealingQuickUseItem currentQuickUseHealingItem;
        public int amountOfHealingItems;
        public int currentAmountOfHealingItems;
    }
}
