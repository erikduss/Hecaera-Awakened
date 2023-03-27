using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike
{
    public class CharacterManager : MonoBehaviour
    {
        [Header("Lock On Transform")]
        public Transform lockOnTransform;

        [Header("Combat Flags")]
        public bool canBeRiposted;
        public bool canBeParried;
        public bool isParrying;
        public bool isBlocking;

        [Header("Spells")]
        public bool isFiringSpell;

        //damage will be inflicted during an animation event
        //used in backstab or riposite animations
        public int pendingCriticalDamage;
    }
}
