using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
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

        [Header("Resources")]
        public int currentHealth = 100;
        public float currentStamina;

        [Header("Stats")]
        public int vitality = 15;
        public int endurance = 10;

        [Header("Bosses")]
        public SerializableDictionary<int, bool> bossesAwakened; //int = ID of boss. 
        public SerializableDictionary<int, bool> bossesDefeated;

        public CharacterSaveData()
        {
            bossesAwakened = new SerializableDictionary<int, bool>();
            bossesDefeated = new SerializableDictionary<int, bool>();
        }
    }
}
