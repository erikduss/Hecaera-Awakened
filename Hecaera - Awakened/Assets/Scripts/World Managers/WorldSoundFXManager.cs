using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Erikduss
{
    public class WorldSoundFXManager : MonoBehaviour
    {
        public static WorldSoundFXManager instance;

        [Header("World Music")]
        public AudioSource bossIntroMusicSource;
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
        public AudioClip ixeleceBossFightIntroMusic;
        public AudioClip ixeleceBossFightPhase1Music;

        [Header("Ixelece Voice")]
        public AudioClip[] ixeleceScreams;

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

        public void PlayBossTrack(AudioClip introTrack, AudioClip loopTrack)
        {
            if(introTrack != null)
            {
                bossIntroMusicSource.clip = introTrack;
                bossIntroMusicSource.loop = false;
                bossIntroMusicSource.Play();

                worldMusicSource.clip = loopTrack;
                worldMusicSource.loop = true;
                bossIntroMusicSource.PlayDelayed(bossIntroMusicSource.clip.length);
            }
            else
            {
                worldMusicSource.clip = loopTrack;
                worldMusicSource.loop = true;
                worldMusicSource.Play();
            }
        }

        public void StopBossTrack()
        {
            float calculatedVolumeMultiplier = (float)SettingsMenuManager.Instance.settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue / 100 
                * ((float)SettingsMenuManager.Instance.settingsGraphicsMenu.mainVolumeOption.currentSubOption.integerValue / 100);

            StartCoroutine(FadeOutBossMusicThenStop(calculatedVolumeMultiplier));
        }

        private IEnumerator FadeOutBossMusicThenStop(float multiplier)
        {
            while(worldMusicSource.volume > 0)
            {
                worldMusicSource.volume -= ((float)1 * multiplier) * Time.deltaTime;
                bossIntroMusicSource.volume -= ((float)1 * multiplier) * Time.deltaTime;
                yield return null;
            }

            bossIntroMusicSource.Stop();
            worldMusicSource.Stop();

            WorldAudioVolumesManager.Instance.LoadAudioFromSavedSettingsData();
        }

        public AudioClip ChooseRandomSFXFromArray(AudioClip[] array)
        {
            int index = Random.Range(0, array.Length);
            return array[index];
        }
    }
}
