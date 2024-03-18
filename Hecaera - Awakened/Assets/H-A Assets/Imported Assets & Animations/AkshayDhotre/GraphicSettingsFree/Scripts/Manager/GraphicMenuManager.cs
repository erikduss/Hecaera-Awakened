using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Erikduss;

namespace AkshayDhotre.GraphicSettingsMenu
{
    [RequireComponent(typeof(GraphicSettingSaveManager))]
    /// <summary>
    /// Handles the toggling of menu and the saving/loading and applying of the graphic settings.
    /// </summary>
    public class GraphicMenuManager : MonoBehaviour
    {
        //Reference to the options in the scene
        [Header("Video Settings")]
        public ResolutionOption resolutionOption;
        public ScreenmodeOption screenmodeOption;
        public QualityLevelOption qualityLevelOption;

        [Header("Volume Settings")]
        public IntValueOption mainVolumeOption;
        public IntValueOption musicVolumeOption;
        public IntValueOption sfxVolumeOption;
        public IntValueOption dialogVolumeOption;

        [Header("Sensitivity Settings")]
        public IntValueOption horizontalSensitivityOption;
        public IntValueOption verticalSensitivityOption;

        [Tooltip("The button on keyboard which when pressed will apply the graphic settings")]
        public KeyCode keyboardApplySettingsKey = KeyCode.Return;

        private GraphicSettingDataContainer dataToSave = new GraphicSettingDataContainer();//Data to be saved will be stored in this 
        public GraphicSettingDataContainer dataToLoad = new GraphicSettingDataContainer();//Data will be loaded into this 

        private GraphicSettingSaveManager graphicSettingSaveManager;

        private void Awake()
        {
            resolutionOption.Initialize();
            screenmodeOption.Initialize();
            qualityLevelOption.Initialize();

            mainVolumeOption.Initialize();
            musicVolumeOption.Initialize();
            sfxVolumeOption.Initialize();
            dialogVolumeOption.Initialize();

            horizontalSensitivityOption.Initialize();
            verticalSensitivityOption.Initialize();
        }

        private void Start()
        {
            graphicSettingSaveManager = GetComponent<GraphicSettingSaveManager>();

            //It is necessary to load the data in Start() rather than in Awake() because we generate the resolution suboption list
            //and the quality level suboption list in the awake function. So if we call this function in awake and apply the settings
            //the fallback suboption settings will be applied.

            SetUpAllSettings();
        }

        public void SetUpAllSettings()
        {
            if (graphicSettingSaveManager.FileExists())
            {
                Load();
                UpdateUIFromLoadedData();
                ApplySettings();
                WorldAudioVolumesManager.Instance.LoadAudioFromSavedSettingsData();
            }
            else
            {
                Debug.Log("New Save file Created!");
                SetDefaultSettings();
                Save();

                ForceLoadSaveFile();
            }

            SettingsMenuManager.Instance.SetAllSettingsFromLoadedSettingsData();
        }

        /// <summary>
        /// Called when the UI apply button is pressed
        /// </summary>
        public void OnApplyButtonPress()
        {
            ApplySettings();
        }

        /// <summary>
        /// Applies the settings and saves the new settings
        /// </summary>
        private void ApplySettings()
        {
            resolutionOption.Apply();
            screenmodeOption.Apply();
            qualityLevelOption.Apply();

            mainVolumeOption.Apply();
            musicVolumeOption.Apply();
            sfxVolumeOption.Apply();
            dialogVolumeOption.Apply();

            horizontalSensitivityOption.Apply();
            verticalSensitivityOption.Apply();

            Save();
        }

        public void SetDefaultSettings()
        {
            //lets not mess with video settings to prevent weird issues.
            //dataToSave.screenHeight = (int)resolutionOption.currentSubOption.vector2Value.y;
            //dataToSave.screenWidth = (int)resolutionOption.currentSubOption.vector2Value.x;
            //dataToSave.screenMode = screenmodeOption.currentSubOption.integerValue;
            //dataToSave.qualityLevel = qualityLevelOption.currentSubOption.integerValue;

            mainVolumeOption.SetCurrentsuboptionByValue(graphicSettingSaveManager.defaultDataContainer.mainVolume);
            musicVolumeOption.SetCurrentsuboptionByValue(graphicSettingSaveManager.defaultDataContainer.musicVolume);
            sfxVolumeOption.SetCurrentsuboptionByValue(graphicSettingSaveManager.defaultDataContainer.sfxVolume);
            dialogVolumeOption.SetCurrentsuboptionByValue(graphicSettingSaveManager.defaultDataContainer.dialogVolume);

            horizontalSensitivityOption.SetCurrentsuboptionByValue(graphicSettingSaveManager.defaultDataContainer.horizontalSensitivity);
            verticalSensitivityOption.SetCurrentsuboptionByValue(graphicSettingSaveManager.defaultDataContainer.verticalSensitivity);
        }

        private void ForceLoadSaveFile()
        {
            Load();
            UpdateUIFromLoadedData();
            ApplySettings();
            WorldAudioVolumesManager.Instance.LoadAudioFromSavedSettingsData();
        }

        /// <summary>
        /// Get the values from the option, assign them in the GraphicSettingDataContainer and saves the data into a XML file
        /// </summary>
        public void Save()
        {
            //Assign values to dataToSave
            dataToSave.screenHeight = (int)resolutionOption.currentSubOption.vector2Value.y;
            dataToSave.screenWidth = (int)resolutionOption.currentSubOption.vector2Value.x;
            dataToSave.screenMode = screenmodeOption.currentSubOption.integerValue;
            dataToSave.qualityLevel = qualityLevelOption.currentSubOption.integerValue;

            dataToSave.mainVolume = mainVolumeOption.currentSubOption.integerValue;
            dataToSave.musicVolume = musicVolumeOption.currentSubOption.integerValue;
            dataToSave.sfxVolume = sfxVolumeOption.currentSubOption.integerValue;
            dataToSave.dialogVolume = dialogVolumeOption.currentSubOption.integerValue;

            dataToSave.horizontalSensitivity = horizontalSensitivityOption.currentSubOption.integerValue;
            dataToSave.verticalSensitivity = verticalSensitivityOption.currentSubOption.integerValue;

            graphicSettingSaveManager.SaveSettings(dataToSave);

            //Also update the loaded data.
            dataToLoad.screenHeight = (int)resolutionOption.currentSubOption.vector2Value.y;
            dataToLoad.screenWidth = (int)resolutionOption.currentSubOption.vector2Value.x;
            dataToLoad.screenMode = screenmodeOption.currentSubOption.integerValue;
            dataToLoad.qualityLevel = qualityLevelOption.currentSubOption.integerValue;

            dataToLoad.mainVolume = mainVolumeOption.currentSubOption.integerValue;
            dataToLoad.musicVolume = musicVolumeOption.currentSubOption.integerValue;
            dataToLoad.sfxVolume = sfxVolumeOption.currentSubOption.integerValue;
            dataToLoad.dialogVolume = dialogVolumeOption.currentSubOption.integerValue;

            dataToLoad.horizontalSensitivity = horizontalSensitivityOption.currentSubOption.integerValue;
            dataToLoad.verticalSensitivity = verticalSensitivityOption.currentSubOption.integerValue;
        }

        /// <summary>
        /// Load the settings in dataToLoad(graphicSettingsDataContainer)
        /// </summary>
        public void Load()
        {
            graphicSettingSaveManager.LoadSettings(out dataToLoad);
        }

        /// <summary>
        /// Updates the UI suboption text and also sets the currentSubOption equal to the value from the loaded data
        /// </summary>
        private void UpdateUIFromLoadedData()
        {
            //so that the player will see the current settings on the menu
            resolutionOption.SetCurrentsuboptionByValue(new Vector2(dataToLoad.screenWidth, dataToLoad.screenHeight));
            screenmodeOption.SetCurrentsuboptionByValue(dataToLoad.screenMode);
            qualityLevelOption.SetCurrentsuboptionByValue(dataToLoad.qualityLevel);

            mainVolumeOption.SetCurrentsuboptionByValue(dataToLoad.mainVolume);
            musicVolumeOption.SetCurrentsuboptionByValue(dataToLoad.musicVolume);
            sfxVolumeOption.SetCurrentsuboptionByValue(dataToLoad.sfxVolume);
            dialogVolumeOption.SetCurrentsuboptionByValue(dataToLoad.dialogVolume);

            horizontalSensitivityOption.SetCurrentsuboptionByValue(dataToLoad.horizontalSensitivity);
            verticalSensitivityOption.SetCurrentsuboptionByValue(dataToLoad.verticalSensitivity);
        }
    }
}

