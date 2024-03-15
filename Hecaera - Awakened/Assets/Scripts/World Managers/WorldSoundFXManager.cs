using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class WorldSoundFXManager : MonoBehaviour
    {
        public static WorldSoundFXManager instance;

        [Header("World Music")]
        public AudioSource worldMusicSource;

        [Header("Menu Sounds")]
        public AudioClip menuMusicTrack;

        [Header("Damage Sounds")]
        public AudioClip[] physicalDamageSFX;

        [Header("Action Sounds")]
        public AudioClip rollSFX;

        [Header("Footstep Sounds")]
        public AudioClip[] footstepSFX;

        [Header("Ixelece Boss Music")]
        public AudioClip ixeleceBossFightPhase1Music;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public AudioClip ChooseRandomSFXFromArray(AudioClip[] array)
        {
            int index = Random.Range(0, array.Length);
            return array[index];
        }
    }
}
