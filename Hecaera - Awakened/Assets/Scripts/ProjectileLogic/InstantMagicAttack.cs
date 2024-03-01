using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantMagicAttack : Projectile
{
    [SerializeField] private float projectileSpeed = 2f;
    [SerializeField] private float downwardsVelocity = 1f;

    protected override void Update()
    {
        base.Update();

        if (objectEnabled.Value)
        {
            rb.velocity = transform.forward * projectileSpeed;
        }
            
    }
}
