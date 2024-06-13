using Erikduss;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class PlayOnEmotionsLogic : Projectile
    {
        //this object does not use damage checking from projectile damnage collider
        public NetworkVariable<bool> isDamaging = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private Collider checkCollider;

        public PlayerManager attachedPlayer;
        private int amountOfNearbyPlayers = 0;

        public float startUpTime = 2f;
        private float startUpTimer = 0f;

        private float damageDelayAfterHit = 0.2f;
        private float damageTimer = 0f;

        public GameObject damageActiveObject;
        public GameObject damageInactiveObject;

        private void Awake()
        {
            if (checkCollider == null)
            {
                checkCollider = GetComponent<Collider>();
            }

            checkCollider.enabled = false; //damage collider should only be enabled during attack animations.
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            isDamaging.OnValueChanged += OnIsDamagingValueChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            isDamaging.OnValueChanged -= OnIsDamagingValueChanged;
        }

        protected override void Update()
        {
            if (IsServer)
            {
                transform.position = attachedPlayer.transform.position;
            }

            base.Update();

            if (!IsServer) return;

            if(startUpTimer < startUpTime)
            {
                startUpTimer += Time.deltaTime;
                return;
            }

            if(projectileType == PooledObjectType.EmotionSorrow)
            {
                //startup time is over, we damage now
                if (amountOfNearbyPlayers <= 0)
                {
                    if (!isDamaging.Value) isDamaging.Value = true;

                    if (damageTimer <= 0)
                        DamageAttachedCharacter();
                    else
                        damageTimer -= Time.deltaTime;
                }
                else
                {
                    if (isDamaging.Value) isDamaging.Value = false;
                }
            }
            else if(projectileType == PooledObjectType.EmotionHatred)
            {
                //startup time is over, we damage now
                if (amountOfNearbyPlayers > 0)
                {
                    if (!isDamaging.Value) isDamaging.Value = true;

                    if (damageTimer <= 0)
                        DamageAttachedCharacter();
                    else
                        damageTimer -= Time.deltaTime;
                }
                else
                {
                    if (isDamaging.Value) isDamaging.Value = false;
                }
            }
        }

        private void DamageAttachedCharacter()
        {
            damageTimer = damageDelayAfterHit;

            TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeDamageEffect);
            damageEffect.physicalDamage = 1f;
            damageEffect.magicDamage = 0;
            damageEffect.fireDamage = 0;
            damageEffect.holyDamage = 0;
            damageEffect.contactPoint = Vector3.zero;
            damageEffect.playDamageAnimation = false;
            damageEffect.willPlayDamageSFX = false;

            Debug.Log("Dealing: " + damageEffect.physicalDamage + " Damage");

            //damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);

            if (IsServer)
            {
                attachedPlayer.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                    attachedPlayer.NetworkObjectId,
                    projectileOwnerNetworkID.Value,
                    damageEffect.physicalDamage,
                    damageEffect.magicDamage,
                    damageEffect.fireDamage,
                    damageEffect.holyDamage,
                    damageEffect.poiseDamage,
                    damageEffect.angleHitFrom,
                    damageEffect.contactPoint.x,
                    damageEffect.contactPoint.y,
                    damageEffect.contactPoint.z,
                    false,
                    false);
            }
        }

        protected override void OnObjectEnabledChange(bool oldID, bool newID)
        {
            startUpTimer = 0f;

            //only clear this when disabling.
            if (!newID)
                attachedPlayer = null;

            if(IsServer)
                isDamaging.Value = false;

            base.OnObjectEnabledChange(oldID, newID);
        }

        private void OnIsDamagingValueChanged(bool oldVal, bool newVal)
        {
            //change visual
            if (newVal)
            {
                damageActiveObject.SetActive(true);
                damageInactiveObject.SetActive(false);
            }
            else
            {
                damageActiveObject.SetActive(false);
                damageInactiveObject.SetActive(true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerManager damageTarget = other.GetComponentInParent<PlayerManager>();

            if (damageTarget != null)
            {
                //dont count yourself
                if (damageTarget == attachedPlayer)
                    return;

                amountOfNearbyPlayers++;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerManager damageTarget = other.GetComponentInParent<PlayerManager>();

            if (damageTarget != null)
            {
                //dont count yourself
                if (damageTarget == attachedPlayer)
                    return;

                amountOfNearbyPlayers--;
            }
        }
    }
}
