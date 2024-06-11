using Erikduss;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AssistMeOrbLogic : SyncedObject, IDamageable
{
    public NetworkVariable<float> currentObjectHealth = new NetworkVariable<float>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> objectMaxHealth = new NetworkVariable<float>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private float elapsedTime;
    private bool dealtDamage = false;

    public GameObject visualDamageObject;

    protected override void Update()
    {
        //Projectiles are being handles by the server only!, assign its network position to the position of our transform.
        if (NetworkManager.Singleton.IsServer)
        {
            networkPosition.Value = transform.position;
            networkRotation.Value = transform.rotation;
        }
        //if the character is being controlled from elsewhere, then assign its position here locally.
        else
        {
            //Instantly move the transform is its far away (happens while initiating from the object pool)
            if (Vector3.Distance(transform.position, networkPosition.Value) > 1f)
            {
                transform.position = networkPosition.Value;
            }

            //Position
            transform.position = Vector3.SmoothDamp
                (transform.position,
                networkPosition.Value,
                ref networkPositionVelocity,
                networkPositionSmoothTime);

            //Rotation
            transform.rotation = Quaternion.Slerp
                (transform.rotation,
                networkRotation.Value,
                networkPositionSmoothTime);
        }


        //check if the orb is destroyed.
        if(currentObjectHealth.Value <= 0)
        {
            DestroyOrb();
        }

        //logic for detonation of the orb.
        if (objectEnabled.Value)
        {
            if (activeTime <= 0 && startedTimer)
            {
                //if the max active time is reached. we want to detonate
                //ReturnThisObjectToPool();

                if (currentObjectHealth.Value > 0 && !dealtDamage) DetonateOrb();
            }
            else if (!startedTimer)
            {
                StartReturnTimer();
            }
            else
            {
                activeTime -= Time.deltaTime;
            }
        }
    }

    protected override void OnObjectEnabledChange(bool oldID, bool newID)
    {
        base.OnObjectEnabledChange(oldID, newID);

        if (NetworkManager.Singleton.IsServer)
        {
            currentObjectHealth.Value = objectMaxHealth.Value;
        }
    }

    protected override void ReturnThisObjectToPool()
    {
        if (!dealtDamage && currentObjectHealth.Value > 0) DetonateOrb();

        elapsedTime = 0;
        dealtDamage = false;

        if (NetworkManager.Singleton.IsServer)
        {
            currentObjectHealth.Value = objectMaxHealth.Value;
        }
        
        base.ReturnThisObjectToPool();
    }

    public void DetonateOrb()
    {
        //show visual
        dealtDamage = true;

        elapsedTime = 0;

        GameObject.Instantiate(visualDamageObject, transform.position, Quaternion.identity);

        if (!NetworkManager.Singleton.IsServer) return;

        foreach (PlayerManager player in WorldGameSessionManager.Instance.players)
        {
            //can it be dodged?
            //if (damageTarget != null && !damageTarget.characterNetworkManager.isInvincible.Value)
            //{
            //    contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            //    DamageTarget(damageTarget);
            //}

            float damage = 75f;

            //if this is an attack from group 2, the multiplier is equal to the boss empower multiplier. Otherwise its 1 and wont do anything.
            float multiplier = WorldBossEncounterManager.Instance.bossEmpowerDamageMultiplier.Value;
            Vector3 contactPoint = player.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeDamageEffect);
            damageEffect.characterCausingDamage = null;
            damageEffect.physicalDamage = damage * multiplier;
            damageEffect.magicDamage = 0 * multiplier;
            damageEffect.fireDamage = 0 * multiplier;
            damageEffect.holyDamage = 0 * multiplier;
            damageEffect.contactPoint = contactPoint;

            damageEffect.playDamageAnimation = true;
            damageEffect.willPlayDamageSFX = true;

            player.characterEffectsManager.ProcessInstantEffect(damageEffect, true);
        }

        ReturnThisObjectToPool();
    }

    public void DestroyOrb()
    {
        elapsedTime = 0;

        if (NetworkManager.Singleton.IsServer)
        {
            ReturnThisObjectToPool();
        }
    }

    public void TakeDamage(float damage)
    {
        if(NetworkManager.Singleton.IsServer)
        {
            float newValue = currentObjectHealth.Value -= damage;

            if (newValue < 0) newValue = 0;

            currentObjectHealth.Value = newValue;
        }
    }
}
