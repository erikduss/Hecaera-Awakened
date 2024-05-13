using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class Enums : MonoBehaviour
    {

    }

    public enum CharacterSlot
    {
        CharacterSlot_01,
        CharacterSlot_02,
        CharacterSlot_03,
        CharacterSlot_04,
        CharacterSlot_05,
        CharacterSlot_06,
        CharacterSlot_07,
        CharacterSlot_08,
        CharacterSlot_09,
        CharacterSlot_10,
        NO_SLOT
    }

    //The specific groups, used to detect who can attack who. You cannot attack someone in your own team.
    public enum CharacterGroup
    {
        Team01,
        Team02,
        NONE,
    }

    public enum EnemySpawnType
    {
        BASIC_DUMMY,
        BOSS
    }

    public enum AudioSourceType
    {
        DEFAULT,
        MUSIC,
        SFX,
        DIALOG
    }

    public enum WeaponModelSlot
    {
        RightHand,
        LeftHand
    }

    public enum AttackType
    {
        LightAttack01,
        LightAttack02,
        LightAttack03,
        HeavyAttack01,
        HeavyAttack02,
        HeavyAttack03,
        ChargedAttack01,
        ChargedAttack02,
        ChargedAttack03,
        InstantMagicAttack01,
        LightJumpAttack01,
        //Boss attacks
        SunbeamAttack,
        DeathFromAbove,
        Shockwave,
        GroundSlam,
        GetOutSlam,
        NatureFury,
        LightEmbrace,
        EmbraceCombo,
        SproutingVines,
        UthanorsWrath
    }

    public enum PooledObjectType
    {
        NONE,
        Instant_Magic_Spell,
        FireFruit,
        DamageIndicator,
        ConeDamageIndicator,
        Shockwave,
        FireFruitGroundToxin,
        SproutingVine,
        SproutingVineToxin,
        UthanorWrathPillar,
        GetOutRocks,
        LightEmbraceEffect,
        NatureFuryVisual
    }

    public enum AttackIndicatorType
    {
        NONE,
        YELLOW_NORMAL,
        RED_INDICATED,
        BLUE_CHARGE
    }
}