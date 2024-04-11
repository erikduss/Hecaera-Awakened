using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class CircleGroundIndicator : GroundIndicator
    {
        [SerializeField] private List<MeshRenderer> indicatorRenderers = new List<MeshRenderer>();

        private NetworkObject netObj;

        private Projectile currentlyAttachedProjectile;

        private void Start()
        {
            netObj = GetComponent<NetworkObject>();

            foreach(var renderer in indicatorRenderers)
            {
                renderer.material = Instantiate(renderer.material); //make a copy to make sure every indicator only modifies temporary materials
            }
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

        public void StartFadeingIndicator(float size, Projectile attachedProjectile)
        {
            currentlyAttachedProjectile = attachedProjectile;
            SetIndicatorSize(size);
            StartCoroutine(FadeInIndicators(.5f, 0, 1));
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

        public void ReturnThisProjectileToPool()
        {
            //projectileCollider.DisableDamageCollider();
            //ResetProjectileOwner();
            //startedTimer = false;
            //objectEnabled.Value = false;
            currentlyAttachedProjectile = null;
            objectEnabled.Value = false;
            WorldNetworkObjectPoolManager.Instance.m_PooledObjects[WorldNetworkObjectPoolManager.Instance.GetGameObjectWithPoolType(PooledObjectType.DamageIndicator)].Release(netObj);
        }
    }
}
