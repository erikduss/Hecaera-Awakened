using AkshayDhotre.GraphicSettingsMenu;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class SavedSettingsManager : MonoBehaviour
    {
        //THIS SCRIPT IS NO LONGER USED

        public static SavedSettingsManager instance;

        //public SettingsSaveData LoadedSettingsData { private set { loadedSettingsData = value; } get { return loadedSettingsData; } }
        //private SettingsSaveData loadedSettingsData;

        //public GraphicMenuManager settingsGraphicsMenu;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                //Always make sure to load in the settings
                //LoadSaveSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            //if(settingsGraphicsMenu == null)
            //{
            //    settingsGraphicsMenu = GameObject.FindGameObjectWithTag("GraphicMenuManager").GetComponent<GraphicMenuManager>();
            //}

            //if(settingsGraphicsMenu != null)
            //{
            //    WorldAudioVolumesManager.Instance.LoadAudioFromSavedSettingsData();
            //}
        }

        public void SaveSettings(SettingsSaveData saveData)
        {
            //PlayerPrefs.SetFloat("MainVolume", saveData.mainVolume);
            //PlayerPrefs.SetFloat("MusicVolume", saveData.musicVolume);
            //PlayerPrefs.SetFloat("SFXVolume", saveData.SFXVolume);
            //PlayerPrefs.SetFloat("DialogVolume", saveData.dialogVolume);
            //PlayerPrefs.SetFloat("HorizontalSensitivity", saveData.horizontalSensitivity);
            //PlayerPrefs.SetFloat("VerticalSensitivity", saveData.verticalSensitivity);

            //PlayerPrefs.Save();

            //LoadedSettingsData.mainVolume = saveData.mainVolume;
            //LoadedSettingsData.musicVolume = saveData.musicVolume;
            //LoadedSettingsData.SFXVolume = saveData.SFXVolume;
            //LoadedSettingsData.dialogVolume = saveData.dialogVolume;
            //loadedSettingsData.horizontalSensitivity = saveData.horizontalSensitivity;
            //loadedSettingsData.verticalSensitivity = saveData.verticalSensitivity;
        }

        /*public SettingsSaveData LoadSaveSettings()
        {
            //Create a new save data with values that are saved in the playerprefs (or the default in settinsSaveData)
            loadedSettingsData = new SettingsSaveData();

            if (PlayerPrefs.HasKey("MainVolume"))
            {
                loadedSettingsData.mainVolume = PlayerPrefs.GetFloat("MainVolume");
            }

            if (PlayerPrefs.HasKey("MusicVolume"))
            {
                loadedSettingsData.musicVolume = PlayerPrefs.GetFloat("MusicVolume");
            }

            if (PlayerPrefs.HasKey("SFXVolume"))
            {
                loadedSettingsData.SFXVolume = PlayerPrefs.GetFloat("SFXVolume");
            }

            if (PlayerPrefs.HasKey("DialogVolume"))
            {
                loadedSettingsData.dialogVolume = PlayerPrefs.GetFloat("DialogVolume");
            }

            if (PlayerPrefs.HasKey("HorizontalSensitivity"))
            {
                loadedSettingsData.horizontalSensitivity = PlayerPrefs.GetFloat("HorizontalSensitivity");
            }

            if (PlayerPrefs.HasKey("VerticalSensitivity"))
            {
                loadedSettingsData.verticalSensitivity = PlayerPrefs.GetFloat("VerticalSensitivity");
            }

            return loadedSettingsData;
        }*/
    }
}
