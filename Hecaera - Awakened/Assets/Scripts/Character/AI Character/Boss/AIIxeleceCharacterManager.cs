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

        //General overtime attacks vaiables
        private float stopDOAAtPercentage = -1f;

        //Death From Above Variables
        private int spawnedFireFruits = 0;
        private int minimumAmountOfFireFruitSpawns = 5;
        private float chanceIncreasePerDOASpawn = 2.5f;

        //sprouting Vines Variables
        private int spawnedSproutingVines = 0;
        private int minimumAmountOfSproutingVinesSpawns = 5;
        private float chanceIncreasePerVineSpawn = 2.5f;

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
            soundManager.PlayIxeleceAttackVoice();

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
            soundManager.PlayIxeleceAttackVoice();
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
            StartCoroutine(WorldGroundIndicatorManager.Instance.SpawnGetOutRocks(1.5f, indicatorLocation));

            soundManager.PlayIxeleceAttackVoice();
        }

        public void SpawnSideShockwaves()
        {
            soundManager.PlayIxeleceAttackVoice();
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
            StartCoroutine(WorldGroundIndicatorManager.Instance.SpawnVines(2.5f, indicatorLocation));

            soundManager.PlayIxeleceAttackVoice();
        }
        #endregion

        #region Nature's Fury Attack
        public void ExecuteNatureFury()
        {
            SetNewPoiseValueToBreak(50);
        }

        public void DetonateNatureFury()
        {
            soundManager.PlayIxeleceAttackVoice();
            //prevent poise break if too late.
            currentlyUsePoise = false;
            currentlySpawnedNatureFury = Instantiate(natureFuryPrefab, transform.position, Quaternion.identity);
            Destroy(currentlySpawnedNatureFury, 1.5f);
        }

        #endregion

        #region Light Embrace Attack & combo attack
        public void ExecuteLightEmbrace()
        {
            soundManager.PlayIxeleceAttackVoice();

            //Vector3 indicatorLocation = new Vector3(transform.position.x, 5f, transform.position.z);
            float indicatorSize = 24f;

            //apply offsets if needed.
            Vector3 spawnLocation = new Vector3(transform.position.x, 5, transform.position.z);
            spawnLocation += (transform.forward * (indicatorSize/2));


            Vector3 relativePos;

            if (IsOwner)
                relativePos = combatManager.currentTarget.transform.position - transform.position;
            else
                relativePos = WorldGameSessionManager.Instance.GetPlayerWithNetworkID(characterNetworkManager.currentTargetNetworkObjectID.Value).transform.position - transform.position;

            Quaternion spawnRotation = Quaternion.LookRotation(relativePos);

            if(IsOwner)
                WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.ConeDamageIndicator, 0, spawnLocation, spawnRotation, indicatorSize, null, true, true, 1.5f, .6f);
            
            StartCoroutine(WorldGroundIndicatorManager.Instance.SpawnLightEmbraceVisual(1.5f, transform.position, spawnRotation));
        }

        public void ExecuteLightEmbraceCombo()
        {
            soundManager.PlayIxeleceAttackVoice();

            //Vector3 indicatorLocation = new Vector3(transform.position.x, 5f, transform.position.z);
            float indicatorSize = 24f;

            //apply offsets if needed.
            Vector3 spawnLocation = new Vector3(transform.position.x, 5, transform.position.z);
            spawnLocation += (transform.forward * (indicatorSize / 2));


            Vector3 relativePos;

            if (IsOwner)
                relativePos = combatManager.currentTarget.transform.position - transform.position;
            else
                relativePos = WorldGameSessionManager.Instance.GetPlayerWithNetworkID(characterNetworkManager.currentTargetNetworkObjectID.Value).transform.position - transform.position;

            Quaternion spawnRotation = Quaternion.LookRotation(relativePos);

            if (IsOwner)
                WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.ConeDamageIndicator, 0, spawnLocation, spawnRotation, indicatorSize, null, true, true, 1.5f, .6f);

            StartCoroutine(WorldGroundIndicatorManager.Instance.SpawnLightEmbraceVisual(1.5f, transform.position, spawnRotation));
        }

        #endregion

        #region Sprouting Vines Attack

        public void ExcecuteSproutingVines()
        {
            if (!IsOwner) return;

            if (stopDOAAtPercentage == -1f)
            {
                //set the percentage at which to stop attacking at.
                stopDOAAtPercentage = Random.Range(minimumAmountOfSproutingVinesSpawns * chanceIncreasePerVineSpawn, 100f);
            }

            float currentPercentage = spawnedSproutingVines * chanceIncreasePerVineSpawn;

            if (currentPercentage > stopDOAAtPercentage)
            {
                StopDeathFromAbove();
                return;
            }

            float indicatorSize = 4f;

            //spawn vines under every player
            foreach(var player in WorldGameSessionManager.Instance.players)
            {
                //WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.DamageIndicator, 0, player.playerNetworkManager.networkPosition.Value, Quaternion.identity, indicatorSize, null, true, true, 2.5f, .6f);
                Vector3 vineSpawnLocation = new Vector3(player.playerNetworkManager.networkPosition.Value.x, player.playerNetworkManager.networkPosition.Value.y - 5.5f, player.playerNetworkManager.networkPosition.Value.z);
                WorldProjectilesManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.SproutingVine, 0, vineSpawnLocation, Quaternion.identity, true);
            }

            spawnedSproutingVines++;
        }

        public void StopSproutingVines()
        {
            stopDOAAtPercentage = -1f;
            spawnedSproutingVines = 0;

            characterAnimatorManager.PlayTargetActionAnimation("Sprouting_Vines_Stop", true, false);
        }

        #endregion

        #region Uthanors Wrath

        public void StartExecutingUthanorsWrath()
        {
            if (!IsOwner) return;

            Vector3 spawnLocation = transform.position;
            Quaternion spawnRotation = Quaternion.identity;

            WorldSyncedObjectsManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.UthanorWrathPillar, 0, spawnLocation, spawnRotation);
        }

        public void ExecuteUthanorsWrath()
        {

        }

        #endregion
    }
}
