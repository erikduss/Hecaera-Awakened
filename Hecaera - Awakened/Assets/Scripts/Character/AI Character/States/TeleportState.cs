using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Erikduss
{
    [CreateAssetMenu(menuName = "A.I/States/Teleport")]
    public class TeleportState : AIState
    {
        public AIIxeleceCharacterManager characterManager;

        public string teleportStartAnimation;
        public string teleportEndAnimation;

        public Vector3 teleportDestination;

        private float teleportDelayTimer = 0;
        public float teleportDestinationReachedDelay = 2f;

        private bool teleportStarted = false;
        private bool teleportingBack = false;
        private bool teleportFinished = false;
        private bool spawnedDamageIndicator = false;

        //[HideInInspector] public GameObject damageIndicatorPrefab;
        //private DamageCollider damageIndicatorCollider;
        public float damageIndicatorSize = 20f;

        public AudioClip electrifiedAudio;

        //We want to teleport to a location and afterwards return to the combatstance state.
        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if(aiCharacter.isPerformingAction) return this;

            if (teleportFinished)
            {
                characterManager.isPerformingAction = false;
                return SwitchState(aiCharacter, aiCharacter.combbatStance);
            }
            else
            {
                if (!teleportStarted)
                {
                    teleportStarted = true;
                    teleportDelayTimer = teleportDestinationReachedDelay;
                    //aiCharacter.navMeshAgent.enabled = false;
                    //aiCharacter.GetComponent<CharacterController>().enabled = false;
                    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(teleportStartAnimation, true, false);
                    characterManager.soundManager.PlaySoundFX(electrifiedAudio);
                    characterManager.aIBossUIManager.ActivateAttackIndicator(AttackIndicatorType.RED_INDICATED, 0.5f, 0.25f);
                    IxeleceMaterialManagement.Instance.SetIxeleceTeleportMaterial();
                    IxeleceMaterialManagement.Instance.FadeTeleportMaterials(2f, 1f, 0f);
                }
                else if (teleportDelayTimer >= 0 && !aiCharacter.isPerformingAction)
                {
                    if (!spawnedDamageIndicator)
                    {
                        spawnedDamageIndicator = true;

                        WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(characterManager.NetworkBehaviourId, (int)PooledObjectType.DamageIndicator, 0, teleportDestination, Quaternion.identity, damageIndicatorSize, null, true, true, teleportDestinationReachedDelay, 2.5f);

                        //damageIndicatorPrefab = Instantiate(damageIndicatorPrefab, teleportDestination, Quaternion.identity);
                        //damageIndicatorPrefab.GetComponent<GroundIndicator>().SetIndicatorSize(damageIndicatorSize);
                        //damageIndicatorCollider = damageIndicatorPrefab.GetComponentInChildren<DamageCollider>();
                        //damageIndicatorCollider.groupOfAttack = CharacterGroup.Team02;
                        //damageIndicatorCollider.DisableDamageCollider();
                    }

                    teleportDelayTimer -= Time.deltaTime;
                    aiCharacter.transform.position = new Vector3(10,-20,100);
                    aiCharacter.transform.rotation = new Quaternion(0,180,0,0);
                }
                else if(teleportDelayTimer <= 0 && teleportStarted && !teleportingBack)
                {
                    teleportingBack = true;
                    aiCharacter.transform.position = teleportDestination;
                    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(teleportStartAnimation, true, false);
                    IxeleceMaterialManagement.Instance.FadeTeleportMaterials(1f, 0f, 1f);
                    //damageIndicatorCollider.EnableDamageCollider();
                    characterManager.soundManager.PlaySoundFX(electrifiedAudio);
                }
                else if (!aiCharacter.isPerformingAction && teleportingBack)
                {
                    //damageIndicatorCollider.DisableDamageCollider();
                    //Destroy(damageIndicatorPrefab);
                    IxeleceMaterialManagement.Instance.RevertIxeleceMaterial();
                    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Idle", false, false, true, true);

                    characterManager.isPerformingAction = false;
                    teleportFinished = true;
                    //aiCharacter.navMeshAgent.enabled = true;
                    //aiCharacter.GetComponent<CharacterController>().enabled = true;
                    return SwitchState(aiCharacter, aiCharacter.combbatStance);
                }
                return this;
            }
        }

        protected override void ResetStateFlags(AICharacterManager aICharacterManager)
        {
            base.ResetStateFlags(aICharacterManager);

            aICharacterManager.isPerformingAction = false;
            aICharacterManager.characterLocomotionManager.canRotate = true;

            teleportFinished = false;
            teleportStarted = false;
            teleportingBack = false;
            spawnedDamageIndicator = false;
        }
    }
}
