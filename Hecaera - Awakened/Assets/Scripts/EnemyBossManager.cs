using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike
{
    public class EnemyBossManager : MonoBehaviour
    {
        public string bossName;

        UIBossHealthBar bossHealthBar;
        EnemyStatsManager enemyStats;
        EnemyAnimatorManager enemyAnimatorManager;

        [Header("Second Phase FX")]
        public GameObject particleFX;

        //Handle switching phases
        //Handle switching attack patterns
        public int currentActivePhase = 0;
        public bool switchToNextPhase = false;

        private void Awake()
        {
            bossHealthBar = FindObjectOfType<UIBossHealthBar>();
            enemyStats = GetComponent<EnemyStatsManager>();
            enemyAnimatorManager = GetComponent<EnemyAnimatorManager>();
        }

        private void Start()
        {
            bossHealthBar.SetBossName(bossName);
            bossHealthBar.SetBossMaxHealth(enemyStats.maxHealth);
        }

        private void Update()
        {
            if (switchToNextPhase)
            {
                switchToNextPhase = false;

                switch (currentActivePhase)
                {
                    case 0:
                        ShiftToFirstPhase();
                        break;
                    case 1:
                        ShiftToSecondPhase();
                        break;
                    case 2:
                        ShiftToThirdPhase();
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    case 5: 
                        break;
                    default:
                        break;
                }
            }
        }

        public void UpdateBossHealthBar(int currentHealth, int maxHealth)
        {
            bossHealthBar.SetBossCurrentHealth(currentHealth);
        }

        public void ShiftToFirstPhase()
        {
            enemyAnimatorManager.anim.SetBool("isInvulnerable", true);
            enemyAnimatorManager.anim.SetBool("isPhaseShifting", true);
            enemyAnimatorManager.PlayTargetAnimation("Phase Shift", true);
            currentActivePhase = 1;
        }

        public void ShiftToSecondPhase()
        {
            enemyAnimatorManager.anim.SetBool("isInvulnerable", true);
            enemyAnimatorManager.anim.SetBool("isPhaseShifting", true);
            enemyAnimatorManager.PlayTargetAnimation("Phase Shift", true);
            currentActivePhase = 2;
        }

        public void ShiftToThirdPhase()
        {
            enemyAnimatorManager.anim.SetBool("isInvulnerable", true);
            enemyAnimatorManager.anim.SetBool("isPhaseShifting", true);
            enemyAnimatorManager.PlayTargetAnimation("Phase Shift", true);
            currentActivePhase = 3;
        }
    }
}
