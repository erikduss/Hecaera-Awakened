using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class AIIxeleceCharacterManager : AIBossCharacterManager
    {
        [SerializeField] TeleportState teleportState;

        [SerializeField] AIIxeleceSoundFXManager soundManager;

        [Header("Ixelece Specific")]
        [SerializeField] private Vector3 middleArenaTeleportLocation;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                teleportState = Instantiate(teleportState);

                teleportState.teleportDestination = middleArenaTeleportLocation;

                aICharacterNetworkManager.maxStamina.Value = 500;
                aICharacterNetworkManager.currentStamina.Value = 500;
                aICharacterNetworkManager.maxHealth.Value = 1000;
                aICharacterNetworkManager.currentHealth.Value = 1000;
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
    }
}
