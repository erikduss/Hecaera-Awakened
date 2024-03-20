using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Erikduss
{
    public class CharacterSoundFXManager : MonoBehaviour
    {
        private AudioSource audioSource;
        private bool startedGame = false;

        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            WorldAudioVolumesManager.Instance.AddAudioSource(audioSource, AudioSourceType.SFX);
        }

        public void PlaySoundFX(AudioClip soundFX, bool randomizePitch = true, float pitchRandom = 0.1f)
        {
            audioSource.PlayOneShot(soundFX);

            //Resets pitch
            audioSource.pitch = 1;

            if (randomizePitch)
            {
                audioSource.pitch += Random.Range(-pitchRandom, pitchRandom);
            }
        }

        public void PlayFootstepSoundFX()
        {
            if (!startedGame)
                if (SceneManager.GetActiveScene().buildIndex == 0) return;

            startedGame = true;
            audioSource.PlayOneShot(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(WorldSoundFXManager.instance.footstepSFX));
        }

        public void PlayRollSoundFX()
        {
            audioSource.PlayOneShot(WorldSoundFXManager.instance.rollSFX);
        }
    }
}
