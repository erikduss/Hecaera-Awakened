using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAudioVolumesManager : MonoBehaviour
{
    public static WorldAudioVolumesManager Instance;

    [Header("Audio Sources")]
    public List<AudioSource> musicAudioSources = new List<AudioSource>();
    public List<AudioSource> SFXAudioSources = new List<AudioSource>();
    public List<AudioSource> dialogAudioSources = new List<AudioSource>();

    private void Awake()
    {
        if(Instance == null)
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
                SetAudioSourceVolume(audio, SavedSettingsManager.instance.settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue);
                break;
            case AudioSourceType.SFX: 
                SFXAudioSources.Add(audio);
                SetAudioSourceVolume(audio, SavedSettingsManager.instance.settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue);
                break;
            case AudioSourceType.DIALOG: 
                dialogAudioSources.Add(audio);
                SetAudioSourceVolume(audio, SavedSettingsManager.instance.settingsGraphicsMenu.dialogVolumeOption.currentSubOption.integerValue);
                break;
            default:
                SFXAudioSources.Add(audio);
                SetAudioSourceVolume(audio, SavedSettingsManager.instance.settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue);
                break;
        }
    }

    public void LoadAudioFromSavedSettingsData()
    {
        Debug.Log("Using Values (1)");
        SetMusicAudioSourcesVolumes(SavedSettingsManager.instance.settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue, 
            SavedSettingsManager.instance.settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue);
        SetSFXAudioSourcesVolumes(SavedSettingsManager.instance.settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue, 
            SavedSettingsManager.instance.settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue);
        SetDialogAudioSourcesVolumes(SavedSettingsManager.instance.settingsGraphicsMenu.dialogVolumeOption.currentSubOption.integerValue, 
            SavedSettingsManager.instance.settingsGraphicsMenu.dialogVolumeOption.currentSubOption.integerValue);
    }

    private void SetAudioSourceVolume(AudioSource source, float volume)
    {
        float calculatedVolume = volume * (SavedSettingsManager.instance.settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue / 100);
        float fixedVolume = calculatedVolume / 100;

        source.volume = fixedVolume;
    }

    public void SetMusicAudioSourcesVolumes(float volume, float mainVolume)
    {
        float calculatedVolume = volume * (mainVolume / 100);
        float fixedVolume = calculatedVolume / 100;

        foreach(AudioSource source in musicAudioSources)
        {
            if(source != null)
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
