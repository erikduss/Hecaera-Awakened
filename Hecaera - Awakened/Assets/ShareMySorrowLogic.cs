using Erikduss;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class ShareMySorrowLogic : Projectile
    {
        public GameObject explosionObject;

        //this object does not use damage checking from projectile damnage collider
        public NetworkVariable<bool> isDamaging = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private Collider checkCollider;

        public PlayerManager attachedPlayer;
        private List<PlayerManager> playersThatWillbeDamaged = new List<PlayerManager>();

        public float detonationTime = 2f;
        private float detonationTimer = 0f;

        public float damageDealt = 50f;
        public float damageCheckDelay = 2f;

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
            if (IsServer && !isDamaging.Value)
            {
                transform.position = attachedPlayer.transform.position;
            }

            base.Update();

            if (!IsServer) return;

            if (detonationTimer < detonationTime)
            {
                detonationTimer += Time.deltaTime;
                return;
            }

            //causes the explosion effect to appear
            if (!isDamaging.Value)
            {
                isDamaging.Value = true;
                StartCoroutine(DamageClosebyCharacters(damageCheckDelay));
            }
        }

        private IEnumerator DamageClosebyCharacters(float delay)
        {
            Vector3 explosionLocation = transform.position;

            yield return new WaitForSeconds(delay);

            //we dont need to damage anything
            if (playersThatWillbeDamaged.Count > 0)
            {
                foreach (PlayerManager player in playersThatWillbeDamaged)
                {
                    TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeDamageEffect);
                    damageEffect.physicalDamage = damageDealt;
                    damageEffect.magicDamage = 0;
                    damageEffect.fireDamage = 0;
                    damageEffect.holyDamage = 0;
                    damageEffect.contactPoint = Vector3.zero;
                    damageEffect.playDamageAnimation = true;
                    damageEffect.willPlayDamageSFX = true;

                    Debug.Log("Dealing: " + damageEffect.physicalDamage + " Damage");

                    //damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);

                    if (IsServer)
                    {
                        player.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
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
                            true,
                            true);
                    }
                }
            }

            //spawn toxin.
            //player has 1 second to get away from the toxin before taking damage.
            WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)objectToSpawn, 0, explosionLocation, Quaternion.identity, 10, null, true, true, 1f, 7.5f);
        }

        protected override void OnObjectEnabledChange(bool oldID, bool newID)
        {
            detonationTimer = 0f;

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
                explosionObject.SetActive(true);
            }
            else
            {
                explosionObject.SetActive(false);
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

                playersThatWillbeDamaged.Add(damageTarget);
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

                playersThatWillbeDamaged.Remove(damageTarget);
            }
        }
    }
}
