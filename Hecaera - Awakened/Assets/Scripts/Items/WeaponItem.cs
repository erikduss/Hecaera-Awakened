using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : Item
{
    [Header("Weapon Model")]
    public GameObject weaponModel;

    [Header("Weapon Requirements")]
    public int strengthREQ = 0;

    [Header("Weapon Base Damage")]
    public int physicalDamage = 0;
    public int magicDamage = 0;
    public int fireDamage = 0;
    public int holyDamage = 0;
    public int lightningDamage = 0;

    [Header("Weapon Poise")]
    public float poiseDamage = 10;

    [Header("Attack Modifiers")]
    public float light_Attack_01_Modifier = 1;
    public float heavy_Attack_01_Modifier = 1.4f;
    public float charged_Heavy_Attack_01_Modifier = 2f;

    [Header("Stamina Cost Modifiers")]
    public int baseStaminaCost = 20;
    public float lightAttackStaminaCostMultiplier = 1;
    public float heavyAttackStaminaCostMultiplier = 1.4f;
    public float instantMagicAttackStaminaCostMultiplier = 1;

    [Header("Actions")]
    public WeaponItemAction oh_RB_Action; //one hand, right bumper action
    public WeaponItemAction oh_RT_Action; //One handed right trigger action (bottom bumper)

    public WeaponItemAction oh_LB_Action; //one hand, left bumper action
}
