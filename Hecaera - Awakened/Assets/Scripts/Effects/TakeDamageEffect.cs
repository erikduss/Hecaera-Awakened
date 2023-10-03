using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take Damage")]
public class TakeDamageEffect : InstantCharacterEffect
{
    [Header("Character Causing Damage")]
    public CharacterManager characterCausingDamage; //if the character is caused by another characters attack

    [Header("Damage")]
    public float physicalDamage = 0;
    public float magicDamage = 0;
    public float fireDamage = 0;
    public float lightningDamage = 0;
    public float holyDamage = 0;

    [Header("Final Damage")]
    private int finalDamageDealt = 0; //The damage the character takes after all calculations.

    [Header("Poise")]
    public float poiseDamage = 0;
    public bool poiseBroken = false; //If character's poise is broken, they will be stunned.

    [Header("Animation")]
    public bool playDamageAnimation = true;
    public bool manuallySelectDamageAnimation = false;
    public string damageAnimation;

    [Header("Sound FX")]
    public bool willPlayDamageSFX = true;
    public AudioClip elementalDamageSoundFX; //additional sound effect on top of regular SFX

    [Header("Direction Damage Taken From")]
    public float angleHitFrom;
    public Vector3 contactPoint; //Used to determine where the blood fx will appear.

    public override void ProcessEffect(CharacterManager character)
    {
        base.ProcessEffect(character);

        //If the character is dead, cant take damage.
        if (character.characterNetworkManager.isDead.Value)
            return;

        CalculateDamage(character);
    }

    private void CalculateDamage(CharacterManager character)
    {
        if (!character.IsOwner)
            return;

        if(characterCausingDamage != null)
        {
            //check for damage modifiers from the attacker (buffs, etc)
        }

        finalDamageDealt = Mathf.RoundToInt(physicalDamage + magicDamage + fireDamage + lightningDamage + holyDamage);

        if(finalDamageDealt <= 0)
        {
            finalDamageDealt = 1;
        }

        character.characterNetworkManager.currentHealth.Value -= finalDamageDealt;


    }
}
