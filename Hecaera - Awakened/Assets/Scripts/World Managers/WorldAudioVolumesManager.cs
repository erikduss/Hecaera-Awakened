using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class WorldAudioVolumesManager : MonoBehaviour
    {
        public static WorldAudioVolumesManager Instance;

        [Header("Audio Sources")]
        public List<AudioSource> musicAudioSources = new List<AudioSource>();
        public List<AudioSource> SFXAudioSources = new List<AudioSource>();
        public List<AudioSource> dialogAudioSources = new List<AudioSource>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddAudioSource(AudioSource audio, AudioSourceType audioType)
        {
            switch (audioType)
            {
                case AudioSourceType.MUSIC:
                    musicAudioSources.Add(audio);
                    SetAudioSourceVolume(audio, SettingsMenuManager.Instance.settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue);
                    break;
                case AudioSourceType.SFX:
                    SFXAudioSources.Add(audio);
                    SetAudioSourceVolume(audio, SettingsMenuManager.Instance.settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue);
                    break;
                case AudioSourceType.DIALOG:
                    dialogAudioSources.Add(audio);
                    SetAudioSourceVolume(audio, SettingsMenuManager.Instance.settingsGraphicsMenu.dialogVolumeOption.currentSubOption.integerValue);
                    break;
                default:
                    SFXAudioSources.Add(audio);
                    SetAudioSourceVolume(audio, SettingsMenuManager.Instance.settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue);
                    break;
            }
        }

        public void LoadAudioFromSavedSettingsData()
        {
            //NOTE IF EDITING THIS: Make sure main volume settings are applied to all the audio. DONT APPLY THE SAME VOLUME SETTING TWICE.
            //Main volume affects all the audio, it needs to be taking into concideration while setting audio volumes.
            SetMusicAudioSourcesVolumes(SettingsMenuManager.Instance.settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue,
                SettingsMenuManager.Instance.settingsGraphicsMenu.mainVolumeOption.currentSubOption.integerValue);

            SetSFXAudioSourcesVolumes(SettingsMenuManager.Instance.settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue,
                SettingsMenuManager.Instance.settingsGraphicsMenu.mainVolumeOption.currentSubOption.integerValue);

            SetDialogAudioSourcesVolumes(SettingsMenuManager.Instance.settingsGraphicsMenu.dialogVolumeOption.currentSubOption.integerValue,
                SettingsMenuManager.Instance.settingsGraphicsMenu.mainVolumeOption.currentSubOption.integerValue);
        }

        private void SetAudioSourceVolume(AudioSource source, float volume)
        {
            float calculatedVolume = volume * ((float)SettingsMenuManager.Instance.settingsGraphicsMenu.mainVolumeOption.currentSubOption.integerValue / 100);
            float fixedVolume = calculatedVolume / 100;

            source.volume = fixedVolume;
        }

        public void SetMusicAudioSourcesVolumes(float volume, float mainVolume)
        {
            float calculatedVolume = volume * (mainVolume / 100);
            float fixedVolume = calculatedVolume / 100;

            foreach (AudioSource source in musicAudioSources)
            {
                if (source != null)
                    source.volume = fixedVolume;
            }
        }

        public void SetSFXAudioSourcesVolumes(float volume, float mainVolume)
        {
            float calculatedVolume = volume * (mainVolume / 100);
            float fixedVolume = calculatedVolume / 100;

            foreach (AudioSource source in SFXAudioSources)
            {
                if (source != null)
                    source.volume = fixedVolume;
            }
        }

        public void SetDialogAudioSourcesVolumes(float volume, float mainVolume)
        {
            float calculatedVolume = volume * (mainVolume / 100);
            float fixedVolume = calculatedVolume / 100;

            foreach (AudioSource source in dialogAudioSources)
            {
                if (source != null)
                    source.volume = fixedVolume;
            }
        }
    }
}
