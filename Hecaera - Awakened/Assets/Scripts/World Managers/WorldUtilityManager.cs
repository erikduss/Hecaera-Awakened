using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUtilityManager : MonoBehaviour
{
    public static WorldUtilityManager Instance;

    [Header("Layers")]
    [SerializeField] LayerMask characterLayers;
    [SerializeField] LayerMask environmentLayers;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public LayerMask GetCharacterLayers()
    {
        return characterLayers;
    }

    public LayerMask GetEnvironmentLayers()
    {
        return environmentLayers;
    }

    public bool CanIDamageThisTarget(CharacterGroup attackingCharacter, CharacterGroup targetCharacter)
    {
        if(attackingCharacter == CharacterGroup.Team01)
        {
            switch(targetCharacter)
            {
                case CharacterGroup.Team01: return false;
                case CharacterGroup.Team02: return true;
                default: return false;
            }
        }
        else if(attackingCharacter == CharacterGroup.Team02)
        {
            switch(targetCharacter)
            {
                case CharacterGroup.Team01: return true;
                case CharacterGroup.Team02: return false;
                default: return false;
            }
        }

        return false;
    }
}
