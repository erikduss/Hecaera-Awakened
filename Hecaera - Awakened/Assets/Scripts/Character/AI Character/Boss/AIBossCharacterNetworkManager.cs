using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class AIBossCharacterNetworkManager : AICharacterNetworkManager
    {
        AIBossCharacterManager aIBossCharacter;

        protected override void Awake()
        {
            base.Awake();

            aIBossCharacter = GetComponent<AIBossCharacterManager>();
        }

        public override void CheckHP(int oldValue, int newValue)
        {
            base.CheckHP(oldValue, newValue);

            if (aIBossCharacter.IsOwner)
            {
                //TODO: Actually implement phase shifting
                float healthNeededForShift = maxHealth.Value * (aIBossCharacter.minimumHealthPercentageToShift / 100);

                if (currentHealth.Value <= healthNeededForShift)
                {
                    aIBossCharacter.PhaseShift();
                }

                float damageTaken =  oldValue - newValue; //this way around due to this being the health values of the character. For example: 750 - 700, if being dealt 50 dmg

                aIBossCharacter.CheckPoiseBreak(damageTaken);

                if (currentHealth.Value <= 0)
                    return;
            }
        }
    }
}
