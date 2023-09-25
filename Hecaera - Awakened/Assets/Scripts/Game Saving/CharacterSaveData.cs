using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterSaveData
{
    [Header("SCENE INDEX")]
    public int sceneIndex = 6; //6 is default at the moment (test scene)

    [Header("Character Name")]
    public string characterName = "Character";

    [Header("Time Played")]
    public float secondsPlayed;

    [Header("World Coordinated")]
    public float xPosition;
    public float yPosition;
    public float zPosition;
}
