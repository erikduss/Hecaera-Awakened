using System.Collections;
using Unity.Netcode;
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

        public IxeleceMaterialManagement ixeleceMaterialManagement;

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

        public LayerMask uthanorWrathBlockLayer;

        public float thisEndsNowProjectileDelay = 1f;
        private float thisEndsNowSpawnTimer = 0;
        public bool spawnThisEndsNowProjectiles = false;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (ixeleceMaterialManagement == null) ixeleceMaterialManagement = GetComponent<IxeleceMaterialManagement>();

            if (IsOwner)
            {
                teleportState = Instantiate(teleportState);

                teleportState.teleportDestination = middleArenaTeleportLocation;
                teleportState.characterManager = this;

                if (!bossIsClone.Value)
                {
                    aICharacterNetworkManager.maxStamina.Value = 500;
                    aICharacterNetworkManager.currentStamina.Value = 500;

                    aICharacterNetworkManager.maxHealth.Value = 1000;

                    if (!WorldGameSessionManager.Instance.skipPhase1)
                    {
                        aICharacterNetworkManager.currentHealth.Value = 1000;
                    }
                    else
                    {
                        aICharacterNetworkManager.currentHealth.Value = 1;
                    }
                }
            }

            combatManager = GetComponent<AIIxeleceCombatManager>();
            aIBossUIManager = GetComponent<AIBossUIManager>();

            if (bossIsClone.Value)
            {
                ixeleceMaterialManagement.SetCloneMaterial();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!IsServer) return;

            if (spawnThisEndsNowProjectiles && !aICharacterNetworkManager.isDead.Value)
            {
                if (thisEndsNowSpawnTimer <= 0)
                {
                    //spawn
                    thisEndsNowSpawnTimer = thisEndsNowProjectileDelay;
                    SpawnThisEndsNowProjectile();
                }
                else
                    thisEndsNowSpawnTimer -= Time.deltaTime;
            }
        }

        public override void PhaseShift()
        {
            base.PhaseShift();

            if(currentBossPhase.Value == 4)
            {
                //Set a delay for when the boss is still phasing.
                thisEndsNowSpawnTimer = 10f;

                spawnThisEndsNowProjectiles = true;
            }
        }

        public void SpawnThisEndsNowProjectile()
        {
            float randomX = Random.Range(-maxArenaLengthFromMiddle, maxArenaLengthFromMiddle);
            float randomY = Random.Range(30f, 60f);
            float randomZ = Random.Range(-maxArenaLengthFromMiddle, maxArenaLengthFromMiddle);

            Vector3 spawnLocation = new Vector3(middleArenaTeleportLocation.x + randomX, middleArenaTeleportLocation.y + randomY, middleArenaTeleportLocation.z + randomZ);

            WorldProjectilesManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.ThisEndsNow, 0, spawnLocation, Quaternion.identity, true);
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
            WorldGroundIndicatorManager.Instance.SpawnGetOutRocks(1.5f, indicatorLocation);

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
            WorldGroundIndicatorManager.Instance.SpawnVines(2.5f, indicatorLocation);

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
            
            WorldGroundIndicatorManager.Instance.SpawnLightEmbraceVisual(1.5f, transform.position, spawnRotation);
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

            WorldGroundIndicatorManager.Instance.SpawnLightEmbraceVisual(1.5f, transform.position, spawnRotation);
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

            Vector3 indicatorLocation = transform.position;
            indicatorLocation.y += 2f;

            float indicatorSize = 75f;

            WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.DamageIndicator, 0, indicatorLocation, Quaternion.identity, indicatorSize, null, true);

            for (int i = 0; i < 4; i++)
            {
                Vector3 spawnLocation = transform.position;
                spawnLocation.y = -10f;

                if(i == 0)
                {
                    spawnLocation.x = transform.position.x + 7.5f;
                    spawnLocation.z = transform.position.z + 7.5f;
                }
                else if (i == 1)
                {
                    spawnLocation.x = transform.position.x + -10f;
                    spawnLocation.z = transform.position.z + -10f;
                }
                else if (i == 2)
                {
                    spawnLocation.x = transform.position.x + -7.5f;
                    spawnLocation.z = transform.position.z + 7.5f;
                }
                else if (i == 3)
                {
                    spawnLocation.x = transform.position.x + 10f;
                    spawnLocation.z = transform.position.z + -10f;
                }

                Quaternion spawnRotation = Quaternion.identity;

                WorldSyncedObjectsManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.UthanorWrathPillar, 0, spawnLocation, spawnRotation);
            }
        }

        public void ExecuteUthanorsWrath()
        {
            if(!IsOwner) return;

            Vector3 spawnLocation = transform.position;
            Quaternion spawnRotation = Quaternion.identity;

            WorldSyncedObjectsManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.UthanorsWrathVisual, 0, spawnLocation, spawnRotation);
            
            StartCoroutine(DamageEveryPlayerInEncounterIfNotBehindCover(99999));
        }

        public IEnumerator DamageEveryPlayerInEncounterIfNotBehindCover(float damage)
        {
            yield return new WaitForSeconds(.5f);

            foreach(PlayerManager player in WorldGameSessionManager.Instance.players)
            {
                RaycastHit hit;
                
                Vector3 fromPos = transform.position;
                fromPos.y += 1f;

                Vector3 toPos = player.transform.position;
                toPos.y += 1f;

                Vector3 dir =  toPos - fromPos;

                // Does the ray hit the pillar?
                if (Physics.Raycast(fromPos, dir, out hit, Mathf.Infinity, uthanorWrathBlockLayer))
                {
                    continue;
                }

                Debug.Log("Hit Player!" + player.NetworkBehaviourId);

                //can it be dodged?
                //if (damageTarget != null && !damageTarget.characterNetworkManager.isInvincible.Value)
                //{
                //    contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

                //    DamageTarget(damageTarget);
                //}

                //if this is an attack from group 2, the multiplier is equal to the boss empower multiplier. Otherwise its 1 and wont do anything.
                float multiplier = WorldBossEncounterManager.Instance.bossEmpowerDamageMultiplier.Value;
                Vector3 contactPoint = player.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

                TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeDamageEffect);
                damageEffect.characterCausingDamage = this;
                damageEffect.physicalDamage = damage * multiplier;
                damageEffect.magicDamage = 0 * multiplier;
                damageEffect.fireDamage = 0 * multiplier;
                damageEffect.holyDamage = 0 * multiplier;
                damageEffect.contactPoint = contactPoint;

                damageEffect.playDamageAnimation = true;
                damageEffect.willPlayDamageSFX = true;

                player.characterEffectsManager.ProcessInstantEffect(damageEffect, true);
            }
        }

        #endregion

        #region More Trouble

        public void InitializeMoreTrouble()
        {
            if (!IsOwner) return;

            //how many clones to spawn?
            int amountOfClonesToSpawn = 3;

            //clones spawn positions?

            //spawn delays?

            //clones alternate colors?

            Transform parentOfClones = this.transform.parent;

            StartCoroutine(spawnClones(1.5f, amountOfClonesToSpawn, parentOfClones));
        }

        public IEnumerator spawnClones(float spawnDelay, int amountOfClonesToSpawn, Transform parentOfClones)
        {
            for (int i = 0; i < amountOfClonesToSpawn; i++)
            {
                Vector3 spawnPosition;

                if (i == 0)
                {
                    spawnPosition = new Vector3(transform.position.x - (10), transform.position.y, transform.position.z - 10);
                }
                else if(i == 1)
                {
                    spawnPosition = new Vector3(transform.position.x + (10), transform.position.y, transform.position.z - 10);
                }
                else
                {
                    spawnPosition = new Vector3(transform.position.x - (0), transform.position.y, transform.position.z - 15);
                }

                GameObject clone = Instantiate(this.gameObject, spawnPosition, Quaternion.identity, parentOfClones);
                AIBossCharacterManager cloneManager = clone.GetComponent<AIBossCharacterManager>();
                cloneManager.bossIsClone.Value = true;
                cloneManager.aICharacterNetworkManager.maxHealth.Value = 1;
                cloneManager.aICharacterNetworkManager.currentHealth.Value = 1;
                cloneManager.aICharacterNetworkManager.CheckHP(1,1);
                clone.GetComponent<NetworkObject>().Spawn();

                yield return new WaitForSeconds(spawnDelay);
            }
        }

        #endregion

        #region Avoid This Attack

        public void ExecuteAvoidThisAttack()
        {
            if (!IsOwner) return;

            int randomSpawn = Random.Range(0,4);

            Vector3 spawnOffSet = Vector3.zero;

            if(randomSpawn == 0)
            {
                spawnOffSet = new Vector3(15,0,0);
            }
            if (randomSpawn == 1)
            {
                spawnOffSet = new Vector3(-15, 0, 0);
            }
            if (randomSpawn == 2)
            {
                spawnOffSet = new Vector3(0, 0, 15);
            }
            if (randomSpawn == 3)
            {
                spawnOffSet = new Vector3(0, 0, -15);
            }

            //apply offsets if needed.
            Vector3 spawnLocation = transform.position + spawnOffSet;

            WorldProjectilesManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.AvoidThisProjectile, 0, spawnLocation, Quaternion.identity, true);
        }

        #endregion

        #region Shedding Light

        public void ExecuteSheddingLight()
        {
            //decide of we rotate clockwise or counter clockwise

            int rand = Random.Range(0,2);
            //rotation direction = 0, left -> 1, right.

            for(int i = 0; i < 4; i++)
            {
                Quaternion rotation = Quaternion.Euler(0, i * 90, 0); ;
                //spawn in rectangle damage colliders which damage after spawning
                WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.RectangleDamageIndicator, 0, transform.position, rotation, 20, null, true, true, 1f, 5f, 5f, true, rand);
            }
            //rotate them overtime.
        }

        #endregion

        #region Assist Me Attack

        public void ExecuteAssistMe()
        {
            if (!IsOwner) return;

            for(int i= 0; i < 4; i++)
            {
                Vector3 spawnOffSet = Vector3.zero;

                if (i == 0)
                {
                    spawnOffSet = new Vector3(10, 0, 0);
                }
                if (i == 1)
                {
                    spawnOffSet = new Vector3(-10, 0, 0);
                }
                if (i == 2)
                {
                    spawnOffSet = new Vector3(0, 0, 10);
                }
                if (i == 3)
                {
                    spawnOffSet = new Vector3(0, 0, -10);
                }

                //apply offsets if needed.
                Vector3 spawnLocation = transform.position + spawnOffSet;

                WorldSyncedObjectsManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.AssistMeOrb, 0, spawnLocation, Quaternion.identity);
            }
        }

        #endregion

        #region Play On Emotions Attacks

        public void ExecutePlayOnEmotionsSorrow()
        {
            if (!IsOwner)
                return;

            //we will pick the person that this attaches to in the world projectiles manager script to make it easier.

            WorldProjectilesManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.EmotionSorrow, 0, Vector3.zero, Quaternion.identity, true);
        }

        public void ExecutePlayOnEmotionsHatred()
        {
            if (!IsOwner)
                return;

            //we will pick the person that this attaches to in the world projectiles manager script to make it easier.

            WorldProjectilesManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.EmotionHatred, 0, Vector3.zero, Quaternion.identity, true);
        }

        #endregion

        #region Share my emotions attacks
        public void ExecuteShareMySorrow()
        {
            if (!IsOwner)
                return;

            //we will pick the person that this attaches to in the world projectiles manager script to make it easier.

            WorldProjectilesManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)PooledObjectType.ShareSorrow, 0, Vector3.zero, Quaternion.identity, true);
        }

        #endregion
    }
}
