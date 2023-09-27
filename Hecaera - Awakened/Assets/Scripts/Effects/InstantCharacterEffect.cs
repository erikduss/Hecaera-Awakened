using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantCharacterEffect : ScriptableObject
{
    [Header("EffectI ID")]
    public int instantEffectID;

    public virtual void ProcessEffect(CharacterManager character)
    {

    }
}
