using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterCombatManager : NetworkBehaviour
{
    CharacterManager character;

    [Header("Attack Target")]
    public CharacterManager currentTarget;

    [Header("Attack Type")]
    public AttackType currentAttackType;

    [Header("Lock On Transform")]
    public Transform lockOnTransform;

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

    public virtual void SetTarget(CharacterManager newTarget)
    {
        if (character.IsOwner)
        {
            if(newTarget != null)
            {
                currentTarget = newTarget;
                character.characterNetworkManager.currentTargetNetworkObjectID.Value = newTarget.GetComponent<NetworkObject>().NetworkObjectId;
            }
            else
            {
                currentTarget = null;
            }
        }
    }

    public virtual void SpawnProjectile(int type)
    {
        if (!character.IsOwner)
            return;

        WorldProjectilesManager.Instance.SpawnProjectile(type);
    }
}
