using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Erikduss
{
    public class GroundIndicator : NetworkBehaviour
    {
        public NetworkVariable<bool> objectEnabled = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> networkSize = new NetworkVariable<float>(2f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<bool> damageColliderEnabled = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [SerializeField] protected GameObject objectToScale;

        [SerializeField] public float indicatorSize = 1f;

        [SerializeField] protected List<DecalProjector> indicatorRenderers = new List<DecalProjector>();

        protected NetworkObject netObj;
        public DamageCollider damageCollider;

        protected Projectile currentlyAttachedProjectile;
        public PooledObjectType indicatorReturnType = PooledObjectType.DamageIndicator;

        protected virtual void Start()
        {
            netObj = GetComponent<NetworkObject>();

            damageCollider = GetComponentInChildren<DamageCollider>();
        }

        protected virtual void Update()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                networkPosition.Value = transform.position;
                networkRotation.Value = transform.rotation;
            }
            else
            {
                transform.position = networkPosition.Value;
                transform.rotation = networkRotation.Value;
                SetIndicatorSize(networkSize.Value);
            }

            if (!IsServer) return;

            if (currentlyAttachedProjectile != null)
            {
                if (!objectEnabled.Value) objectEnabled.Value = true;

                float distance = Vector3.Distance(gameObject.transform.position, currentlyAttachedProjectile.transform.position);

                if (distance < 2f)
                {
                    ReturnThisProjectileToPool();
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!NetworkManager.Singleton.IsServer)
            {
                if (transform.position != networkPosition.Value)
                {
                    transform.position = networkPosition.Value;
                }

                if (transform.rotation != networkRotation.Value)
                {
                    transform.rotation = networkRotation.Value;
                }

                //if we're active but the network variable says we shouldnt be active. Disable.
                if (gameObject.activeSelf && !objectEnabled.Value)
                {
                    gameObject.SetActive(false);
                }

                objectEnabled.OnValueChanged += OnObjectEnabledChange;
            }

            damageColliderEnabled.OnValueChanged += OnDamageColliderEnabledChange;
            OnDamageColliderEnabledChange(false, damageColliderEnabled.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            damageColliderEnabled.OnValueChanged -= OnDamageColliderEnabledChange;

            if (!NetworkManager.Singleton.IsServer)
            {
                objectEnabled.OnValueChanged -= OnObjectEnabledChange;
                return;
            }
        }

        public void OnObjectEnabledChange(bool oldID, bool newID)
        {
            gameObject.SetActive(newID);

            if (!NetworkManager.Singleton.IsServer)
                return;
        }

        public virtual void SetIndicatorSize(float size)
        {
            objectToScale.transform.localScale = new Vector3(size, size, size);
        }

        public virtual void OnDamageColliderEnabledChange(bool oldID, bool newID)
        {
            if (newID) damageCollider.EnableDamageCollider();
            else
                damageCollider.DisableDamageCollider();

            if (!NetworkManager.Singleton.IsServer)
                return;
        }

        public virtual void StartFadeingIndicator(float size, Projectile attachedProjectile, bool enableDamageCollider, float damageColliderEnableDelay, float colliderActiveTime)
        {
            currentlyAttachedProjectile = attachedProjectile;
            networkSize.Value = size;
            SetIndicatorSize(size);
            objectEnabled.Value = true;
            StartCoroutine(FadeInIndicators(.5f, 0, 1));

            if (enableDamageCollider)
            {
                StartCoroutine(EnableDamageColliderWithDelay(damageColliderEnableDelay, colliderActiveTime));
            }
        }

        protected virtual IEnumerator EnableDamageColliderWithDelay(float delay, float colliderActiveTime)
        {
            yield return new WaitForSeconds(delay);
            damageColliderEnabled.Value = true;
            damageCollider.EnableDamageCollider();
            yield return new WaitForSeconds(colliderActiveTime);
            damageColliderEnabled.Value = false;
            damageCollider.DisableDamageCollider();
            ReturnThisProjectileToPool();
        }

        protected virtual IEnumerator FadeInIndicators(float duration, float startAlpha, float endAlpha)
        {
            if (duration > 0f)
            {
                foreach (var projector in indicatorRenderers)
                {
                    projector.fadeFactor = startAlpha;
                }

                float timer = 0;

                while (timer < duration)
                {
                    timer += Time.deltaTime;

                    foreach (var projector in indicatorRenderers)
                    {
                        float newAlpha = Mathf.Lerp(projector.fadeFactor, endAlpha, duration * Time.deltaTime);
                        projector.fadeFactor = newAlpha;
                    }

                    yield return null;
                }
            }

            foreach (var projector in indicatorRenderers)
            {
                projector.fadeFactor = endAlpha;
            }

            yield return new WaitForSeconds(5f); //max time for an indicator to stay around
            ReturnThisProjectileToPool();
        }

        public virtual void ReturnThisProjectileToPool()
        {
            if(currentlyAttachedProjectile != null)
            {
                if (currentlyAttachedProjectile.spawnObjectOnCollision)
                {
                    if(currentlyAttachedProjectile.objectToSpawn == PooledObjectType.FireFruitGroundToxin)
                    {
                        //player has 0.5 seconds to get away from the toxin before taking damage.
                        WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)currentlyAttachedProjectile.objectToSpawn, 0, gameObject.transform.position, gameObject.transform.rotation, networkSize.Value * 1.5f, null, true, true, 0.5f, 7.5f);
                    }
                    else if(currentlyAttachedProjectile.objectToSpawn == PooledObjectType.SproutingVineToxin)
                    {
                        //has a spawn delay to make it fair, if you get hit by the vines you will be stunned for a second.
                        WorldGroundIndicatorManager.Instance.NotifyTheServerOfSpawnActionServerRpc(NetworkObjectId, (int)currentlyAttachedProjectile.objectToSpawn, 0.75f, gameObject.transform.position, gameObject.transform.rotation, networkSize.Value * 1.5f, null, true, true, 0.5f, 7.5f);
                    }
                }
            }

            //projectileCollider.DisableDamageCollider();
            //ResetProjectileOwner();
            //startedTimer = false;
            //objectEnabled.Value = false;
            currentlyAttachedProjectile = null;
            damageColliderEnabled.Value = false;
            damageCollider.DisableDamageCollider();
            objectEnabled.Value = false;
            WorldNetworkObjectPoolManager.Instance.m_PooledObjects[WorldNetworkObjectPoolManager.Instance.GetGameObjectWithPoolType(indicatorReturnType)].Release(netObj);
        }
    }
}
