using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    [CreateAssetMenu(menuName = "A.I/Actions/Attack")]
    public class AICharacterAttackAction : ScriptableObject
    {
        [Header("Attack")]
        [SerializeField] private string attackAnimation;

        [Header("Combo Action")]
        public bool actionHasComboAction = false;
        public AICharacterAttackAction comboAction; //The combo action, if rolled for this attack.

        [Header("Action Values")]
        public int attackWeight = 50;
        [SerializeField] AttackType attackType;
        public float actionRecoveryTime = 1.5f; //The time before the character can do another attack.
        public float minimumAttackAngle = -35;
        public float maximumAttackAngle = 35;
        public float minimimAttackDistance = 0;
        public float maximumAttackDistance = 2;

        [Header("Visual Indicating")]
        public bool showVisualIndicator = false;

        public void AttemptToPerformAction(AICharacterManager aICharacter)
        {
            aICharacter.characterAnimatorManager.PlayTargetAttackActionAnimation(attackType, attackAnimation, true);
        }
    }
}
