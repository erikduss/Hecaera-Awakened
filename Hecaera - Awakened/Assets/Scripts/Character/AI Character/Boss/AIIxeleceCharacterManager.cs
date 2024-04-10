using System.Collections;
using System.Collections.Generic;
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
            combatManager.OpenRightClawDamageCollider();
            combatManager.OpenLeftClawDamageCollider();
        }

        public void DeactivateSunbeamDamage()
        {
            currentSpawnedSunbeam.DeativateSunbeam();
            combatManager.CloseLeftClawDamageCollider();
            combatManager.CloseRightClawDamageCollider();
        }

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
            SpawnIndicator(new Vector3(spawnLocation.x, 5f, spawnLocation.z), 5f);

            spawnedFireFruits++;
        }

        private void SpawnIndicator(Vector3 location, float size)
        {
            var damageIndicatorInst = Instantiate(sphereGroundIndicatorPrefab, location, Quaternion.identity);
            damageIndicatorInst.GetComponent<GroundIndicator>().SetIndicatorSize(size);
            var damageIndicatorCollider = damageIndicatorInst.GetComponentInChildren<DamageCollider>();
            damageIndicatorCollider.groupOfAttack = CharacterGroup.Team02;
            damageIndicatorCollider.DisableDamageCollider();
            Destroy(damageIndicatorInst, 2f);
        }

        public void StopDeathFromAbove()
        {
            stopDOAAtPercentage = -1f;
            spawnedFireFruits = 0;

            characterAnimatorManager.PlayTargetActionAnimation("Death_From_Above_Stop", true, false);
        }
    }
}
