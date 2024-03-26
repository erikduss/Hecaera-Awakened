using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class FireFruitLogic : Projectile
    {
        [SerializeField] private float minProjectileSpeed = 2f;
        [SerializeField] private float maxProjectileSpeed = 30f;
        [SerializeField] private float projectileSpeed;

        private void OnEnable()
        {
            projectileSpeed = -Random.Range(minProjectileSpeed, maxProjectileSpeed);
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            if (objectEnabled.Value)
            {
                rb.velocity = transform.up * projectileSpeed;
            }
        }
    }
}
