using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Erikduss
{
    public class UI_Boss_HP_Bar : UI_StatBar
    {
        public AIBossCharacterManager bossCharacter;

        public TextMeshProUGUI bossNameText;
        public TextMeshProUGUI bossPhaseText;

        public bool hasAnotherPhase = false;

        public void EnableBossHPBar(AIBossCharacterManager boss)
        {
            bossCharacter = boss;
            bossCharacter.aICharacterNetworkManager.currentHealth.OnValueChanged += OnBossHPChanged;
            SetMaxStat(bossCharacter.aICharacterNetworkManager.maxHealth.Value);
            SetStat(bossCharacter.aICharacterNetworkManager.currentHealth.Value);

            bossNameText.text = bossCharacter.characterName;
        }

        private void OnDestroy()
        {
            bossCharacter.aICharacterNetworkManager.currentHealth.OnValueChanged -= OnBossHPChanged;
        }

        private void OnBossHPChanged(int oldValue, int newValue)
        {
            SetStat(newValue);

            if (newValue <= 0)
            {
                if(bossCharacter.currentBossPhase.Value >= bossCharacter.amountOfPhases.Value && bossCharacter.aICharacterNetworkManager.isDead.Value)
                {
                    RemoveHPBar(2.5f);
                }
                else
                {
                    StartCoroutine(ChangeToNextPhase());
                }
            }
        }

        public IEnumerator ChangeToNextPhase()
        {
            yield return new WaitForSeconds(1);

            if (bossCharacter.currentBossPhase.Value == 2)
            {
                SetStat(bossCharacter.aICharacterNetworkManager.maxHealth.Value);
                bossPhaseText.text = "Phase 2: Acceptance";
            }
            else if (bossCharacter.currentBossPhase.Value == 3)
            {
                SetStat(bossCharacter.aICharacterNetworkManager.maxHealth.Value);
                bossPhaseText.text = "Phase 3: Sadness";
            }
            else if (bossCharacter.currentBossPhase.Value == 4)
            {
                SetStat(bossCharacter.aICharacterNetworkManager.maxHealth.Value);
                bossPhaseText.text = "Phase 4: Hatred";
            }
        }

        public void RemoveHPBar(float time)
        {
            Destroy(gameObject.transform.parent.gameObject, time);
        }
    }
}
