using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class CircleGroundIndicator : GroundIndicator
    {
        [SerializeField] private List<MeshRenderer> indicatorRenderers = new List<MeshRenderer>();

        private NetworkObject netObj;
        private DamageCollider damageCollider;

        private Projectile currentlyAttachedProjectile;

        private void Start()
        {
            netObj = GetComponent<NetworkObject>();

            damageCollider = GetComponentInChildren<DamageCollider>();

            foreach (var renderer in indicatorRenderers)
            {
                renderer.material = Instantiate(renderer.material); //make a copy to make sure every indicator only modifies temporary materials
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            damageColliderEnabled.OnValueChanged += OnDamageColliderEnabledChange;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            damageColliderEnabled.OnValueChanged -= OnDamageColliderEnabledChange;
        }

        protected override void Update()
        {
            base.Update();

            if (!IsServer) return;

            if (currentlyAttachedProjectile != null)
            {
                if(!objectEnabled.Value) objectEnabled.Value = true;

                float distance = Vector3.Distance(gameObject.transform.position, currentlyAttachedProjectile.transform.position);

                if (distance < 2f)
                {
                    ReturnThisProjectileToPool();
                }
            }
        }

        public void StartFadeingIndicator(float size, Projectile attachedProjectile, bool enableDamageCollider, float damageColliderEnableDelay, float colliderActiveTime)
        {
            currentlyAttachedProjectile = attachedProjectile;
            networkSize.Value = size;
            SetIndicatorSize(size);
            objectEnabled.Value = true;
            StartCoroutine(FadeInIndicators(.5f, 0, 1));

            if(enableDamageCollider)
            {
                StartCoroutine(EnableDamageColliderWithDelay(damageColliderEnableDelay, colliderActiveTime));
            }
        }

        private IEnumerator EnableDamageColliderWithDelay(float delay, float colliderActiveTime)
        {
            yield return new WaitForSeconds (delay);
            Debug.Log("Damage collider active");
            damageColliderEnabled.Value = true;
            damageCollider.EnableDamageCollider();
            yield return new WaitForSeconds(colliderActiveTime);
            Debug.Log("Damage collider disabled");
            damageColliderEnabled.Value = false;
            damageCollider.DisableDamageCollider();
            ReturnThisProjectileToPool();
        }

        private IEnumerator FadeInIndicators(float duration, float startAlpha, float endAlpha)
        {
            if (duration > 0f)
            {
                foreach (var renderer in indicatorRenderers)
                {
                    renderer.material.SetFloat("_CutoutAlphaMul", startAlpha);
                    //renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, startAlpha);
                }
                
                float timer = 0;

                while (timer < duration)
                {
                    timer += Time.deltaTime;

                    foreach (var renderer in indicatorRenderers)
                    {
                        float newAlpha = Mathf.Lerp(renderer.material.GetFloat("_CutoutAlphaMul"), endAlpha, duration * Time.deltaTime);
                        renderer.material.SetFloat("_CutoutAlphaMul", newAlpha);
                        //renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, newAlpha);
                    }

                    yield return null;
                }
            }

            foreach (var renderer in indicatorRenderers)
            {
                renderer.material.SetFloat("_CutoutAlphaMul", endAlpha);
                //renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, endAlpha);
            }

            yield return new WaitForSeconds(5f); //max time for an indicator to stay around
            ReturnThisProjectileToPool();
        }

        public void OnDamageColliderEnabledChange(bool oldID, bool newID)
        {
            if (newID) damageCollider.EnableDamageCollider();
            else
                damageCollider.DisableDamageCollider();

            if (!NetworkManager.Singleton.IsServer)
                return;
        }

        public void ReturnThisProjectileToPool()
        {
            //projectileCollider.DisableDamageCollider();
            //ResetProjectileOwner();
            //startedTimer = false;
            //objectEnabled.Value = false;
            currentlyAttachedProjectile = null;
            damageColliderEnabled.Value = false;
            damageCollider.DisableDamageCollider();
            objectEnabled.Value = false;
            WorldNetworkObjectPoolManager.Instance.m_PooledObjects[WorldNetworkObjectPoolManager.Instance.GetGameObjectWithPoolType(PooledObjectType.DamageIndicator)].Release(netObj);
        }
    }
}
