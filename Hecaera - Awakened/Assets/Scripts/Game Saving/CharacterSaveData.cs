using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterSaveData
{
    [Header("Character Name")]
    public string characterName;

    [Header("Time Played")]
    public float secondsPlayed;

    [Header("World Coordinated")]
    public float xPosition;
    public float yPosition;
    public float zPosition;
}
