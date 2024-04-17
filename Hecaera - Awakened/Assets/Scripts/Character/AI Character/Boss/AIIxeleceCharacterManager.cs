using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Erikduss
{
    public class AIIxeleceCharacterManager : AIBossCharacterManager
    {
        [SerializeField] TeleportState teleportState;

        [SerializeField] public AIIxeleceSoundFXManager soundManager;
        [SerializeField] public AIBossUIManager aIBossUIManager;
        public AIIxeleceCombatManager combatManager;

        [Header("Ixelece Specific")]
        [SerializeField] private Vector3 middleArenaTeleportLocation;
        [SerializeField] private GameObject sphereGroundIndicatorPrefab;

        private float maxArenaLengthFromMiddle = 10f;

        [SerializeField] private GameObject sunbeamAttackPrefab;
        private SunBeamLogic currentSpawnedSunbeam;

        [SerializeField] private GameObject natureFuryPrefab;
        private GameObject currentlySpawnedNatureFury;

        //Death From Above Variables
        private int spawnedFireFruits = 0;
        private int minimumAmountOfFireFruitSpawns = 5;
        private float stopDOAAtPercentage = -1f;
        private float chanceIncreasePerDOASpawn = 2.5f;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                teleportState = Instantiate(teleportState);

                teleportState.teleportDestination = middleArenaTeleportLocation;
                teleportState.characterManager = this;

                aICharacterNetworkManager.maxStamina.Value = 500;
                aICharacterNetworkManager.currentStamina.Value = 500;
                aICharacterNetworkManager.maxHealth.Value = 1000;
                aICharacterNetworkManager.currentHealth.Value = 1000;
            }

            combatManager = GetComponent<AIIxeleceCombatManager>();
            aIBossUIManager = GetComponent<AIBossUIManager>();
        }

        public override void WakeBoss()
        {
            base.WakeBoss();

            soundManager.PlayIxeleceScream();

            currentState = teleportState;
        }

        public void FadeOutModel()
        {

        }

        #region General Attack Functions
        public void ActivateClawsDamage()
        {
            combatManager.OpenRightClawDamageCollider();
            combatManager.OpenLeftClawDamageCollider();
        }

        public void DeactivateClawsDamage()
        {
            combatManager.CloseLeftClawDamageCollider();
            combatManager.CloseRightClawDamageCollider();
        }

        public void ShowAttackIndicator(AttackIndicatorType indicatorType)
        {
            aIBossUIManager.ActivateAttackIndicator(indicatorType, 0.25f, 0.35f);
        }
        #endregion

        #region Sunbeam Activation And Deactivation
        public void StartSunbeam()
        {
            aIBossUIManager.ActivateAttackIndicator(AttackIndicatorType.YELLOW_NORMAL, 0.25f, 0.35f);
            GameObject inst = Instantiate(sunbeamAttackPrefab);
            currentSpawnedSunbeam = inst.GetComponent<SunBeamLogic>();
            currentSpawnedSunbeam.objectToFollow = aICharacterCombatManager.magicHandTransform.gameObject;
            currentSpawnedSunbeam.casterCharacter = this;
        }

        public void ActivateSunbeamDamage()
        {
            currentSpawnedSunbeam.ActivateSunbeam();
            combatManager.SetSunbeamDamage();
            ActivateClawsDamage();
        }

        public void DeactivateSunbeamDamage()
        {
            currentSpawnedSunbeam.DeativateSunbeam();
            DeactivateClawsDamage();
        }
        #endregion


        #region Death From Above Spawn & Despawn
        public void ShowAttackIndicatorDeathFromAbove()
        {
            aIBossUIManager.ActivateAttackIndicator(AttackIndicatorType.RED_INDICATED, 0.35f, 0.75f);
        }

        public void SpawnFireFruitRandom()
        {
            if (!IsOwner) return;

            if(stopDOAAtPercentage == -1f)
            {
                //set the percentage at which to stop attacking at.
                stopDOAAtPercentage = Random.Range(minimumAmountOfFireFruitSpawns * chanceIncreasePerDOASpawn, 100f);
            }

            float currentPercentage = spawnedFireFruits * chanceIncreasePerDOASpawn;

            if(currentPercentage > stopDOAAtPercentage)
            {
                StopDeathFromAbove();
                return;
            }

            float randomX = Random.Range(-maxArenaLengthFromMiddle, maxArenaLengthFromMiddle);
            float randomY = Random.Range(30f, 60f);
            float randomZ = Random.Range(-maxArenaLengthFromMiddle, maxArenaLengthFromMiddle);

            Vector3 spawnLocation = new Vector3(middleArenaTeleportLocation.x + randomX, middleArenaTeleportLocation.y + randomY, middleArenaTeleportLocation.z + randomZ);

            WorldProjectilesManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.FireFruit, 0, spawnLocation, Quaternion.identity, true);
            //SpawnIndicator(new Vector3(spawnLocation.x, 5f, spawnLocation.z), 5f);

            spawnedFireFruits++;
        }

        public void StopDeathFromAbove()
        {
            stopDOAAtPercentage = -1f;
            spawnedFireFruits = 0;

            characterAnimatorManager.PlayTargetActionAnimation("Death_From_Above_Stop", true, false);
        }
        #endregion

        #region Shockwave Activation

        public void SpawnShockwave()
        {
            if (!IsOwner) return;

            //apply offsets if needed.
            Vector3 spawnLocation = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            Vector3 relativePos = combatManager.currentTarget.transform.position - transform.position;

            Quaternion spawnRotation = Quaternion.LookRotation(relativePos);

            WorldProjectilesManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.Shockwave, 0, spawnLocation, spawnRotation, true);
        }
        #endregion

        #region Ground Slam And Side Shockwaves
        public void ExecuteGroundSlam()
        {
            Vector3 indicatorLocation = new Vector3(transform.position.x, 5f, transform.position.z);
            float indicatorSize = 14f;

            WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.DamageIndicator, 0, indicatorLocation, Quaternion.identity, indicatorSize, null, true, true, 1.5f, .6f);
        }

        public void SpawnSideShockwaves()
        {
            if (!IsOwner) return;

            Vector3 leftRelativePos = (transform.position - transform.right) - transform.position;
            Quaternion sideLeft = Quaternion.LookRotation(leftRelativePos);

            Vector3 rightRelativePos = (transform.position + transform.right) - transform.position;
            Quaternion sideRight = Quaternion.LookRotation(rightRelativePos);

            //apply offsets if needed.
            Vector3 spawnLocation = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            WorldProjectilesManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.Shockwave, 0, spawnLocation, sideLeft, true);
            WorldProjectilesManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.Shockwave, 0, spawnLocation, sideRight, true);
        }
        #endregion

        #region Get out Ground Slam
        public void ExecuteGetOutSlam()
        {
            Vector3 indicatorLocation = new Vector3(transform.position.x, 5f, transform.position.z);
            float indicatorSize = 24f;

            WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.DamageIndicator, 0, indicatorLocation, Quaternion.identity, indicatorSize, null, true, true, 2.5f, .6f);
        }
        #endregion

        #region Nature's Fury Attack
        public void ExecuteNatureFury()
        {
            SetNewPoiseValueToBreak(50);
        }

        public void DetonateNatureFury()
        {
            //prevent poise break if too late.
            currentlyUsePoise = false;
            currentlySpawnedNatureFury = Instantiate(natureFuryPrefab, transform.position, Quaternion.identity);
            Destroy(currentlySpawnedNatureFury, 1.5f);
        }

        #endregion
    }
}
