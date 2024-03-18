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
                if (currentHealth.Value <= 0)
                    return;

                float healthNeededForShift = maxHealth.Value * (aIBossCharacter.minimumHealthPercentageToShift / 100);

                if (currentHealth.Value <= healthNeededForShift)
                {
                    aIBossCharacter.PhaseShift();
                }
            }
        }
    }
}
