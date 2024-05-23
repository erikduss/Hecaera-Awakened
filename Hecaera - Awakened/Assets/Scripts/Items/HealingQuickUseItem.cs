using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    [CreateAssetMenu(menuName = "Items/QuickUseItem/Healing Item")]
    public class HealingQuickUseItem : QuickUseItem
    {
        public float healingAmount = 0;
        public bool useAnimationForActivation = true;
        public string onUseAnimation;
    }
}
