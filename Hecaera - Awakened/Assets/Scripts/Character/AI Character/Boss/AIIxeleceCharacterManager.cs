using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class AIIxeleceCharacterManager : AIBossCharacterManager
    {
        [SerializeField] TeleportState teleportState;

        [SerializeField] public AIIxeleceSoundFXManager soundManager;
        public AIIxeleceCombatManager combatManager;

        [Header("Ixelece Specific")]
        [SerializeField] private Vector3 middleArenaTeleportLocation;
        [SerializeField] private GameObject sphereGroundIndicatorPrefab;

        [SerializeField] private GameObject sunbeamAttackPrefab;
        private SunBeamLogic currentSpawnedSunbeam;

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

                combatManager = GetComponent<AIIxeleceCombatManager>();
            }
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
    }
}
