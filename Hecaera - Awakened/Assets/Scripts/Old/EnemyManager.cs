using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SoulsLike
{
    public class EnemyManager : CharacterManager
    {
        EnemyAnimatorManager enemyAnimationManager;
        EnemyStatsManager enemyStatsManager;
        public NavMeshAgent navmeshAgent;

        public bool isPerformingAction;
        public float rotationSpeed = 15;
        public float maximumAggroRadius = 1.5f;

        public CharacterStatsManager currentTarget;
        public Rigidbody enemyRigidbody;

        [Header("A.I Settings")]
        public float detectionRadius = 20;
        public float maximumDetectionAngle = 50;
        public float minimumDetectionAngle = -50;

        public float currentRecoveryTime = 0;

        [Header("A.I Combat Settings")]
        public bool allowAIToPerformCombos;
        public bool isPhaseShifting;
        public float comboLikelyHood;

        private void Awake()
        {
            enemyAnimationManager = GetComponent<EnemyAnimatorManager>();
            navmeshAgent = GetComponentInChildren<NavMeshAgent>();
            enemyStatsManager = GetComponent<EnemyStatsManager>();
            enemyRigidbody = GetComponent<Rigidbody>();
            navmeshAgent.enabled = false;
        }

        private void Start()
        {
            enemyRigidbody.isKinematic = false;
        }

        private void Update()
        {
            HandleRecoveryTime();
            //if (canBeRiposted) return;

            isRotatingWithRootMotion = enemyAnimationManager.anim.GetBool("isRotatingWithRootMotion");
            isInteracting = enemyAnimationManager.anim.GetBool("isInteracting");
            isPhaseShifting = enemyAnimationManager.anim.GetBool("isPhaseShifting");
            isInvulnerable = enemyAnimationManager.anim.GetBool("isInvulnerable");
            canDoCombo = enemyAnimationManager.anim.GetBool("canDoCombo");
            canRotate = enemyAnimationManager.anim.GetBool("canRotate");
            enemyAnimationManager.anim.SetBool("isDead", enemyStatsManager.isDead);
        }

        private void LateUpdate()
        {
            navmeshAgent.transform.localPosition = Vector3.zero;
            navmeshAgent.transform.localRotation = Quaternion.identity;
        }

        private void HandleRecoveryTime()
        {
            if(currentRecoveryTime > 0)
            {
                currentRecoveryTime -= Time.deltaTime;
            }

            if (isPerformingAction)
            {
                if(currentRecoveryTime <= 0)
                {
                    isPerformingAction = false;
                }
            }
        }

        
    }
}
