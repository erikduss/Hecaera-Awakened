using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Erikduss
{
    public class UI_Boss_HP_Bar : UI_StatBar
    {
        [SerializeField] AIBossCharacterManager bossCharacter;

        [SerializeField] TextMeshProUGUI bossNameText;
        [SerializeField] TextMeshProUGUI bossPhaseText;

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
                RemoveHPBar(2.5f);
            }
        }

        public void RemoveHPBar(float time)
        {
            Destroy(gameObject.transform.parent.gameObject, time);
        }
    }
}
