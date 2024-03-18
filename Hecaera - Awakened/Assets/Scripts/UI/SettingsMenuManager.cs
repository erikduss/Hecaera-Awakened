using AkshayDhotre.GraphicSettingsMenu;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Erikduss
{
    public class SettingsMenuManager : MonoBehaviour
    {
        public static SettingsMenuManager Instance;

        PlayerControls playerControls;

        public GraphicMenuManager settingsGraphicsMenu;

        public GameObject settingsMenuGameObject;
        private EventSystem currentEventSystem;

        [Header("Title Screen Inputs")]
        [SerializeField] bool saveSettings = false;

        [Header("Horizontal Sensitivity Components")]
        [SerializeField] private Slider horizontalSensitivitySlider;
        [SerializeField] private TextMeshProUGUI horizontalSensitivityText;

        [Header("Vertical Sensitivity Components")]
        [SerializeField] private Slider verticalSensitivitySlider;
        [SerializeField] private TextMeshProUGUI verticalSensitivityText;

        [Header("Main Volume Components")]
        [SerializeField] private Slider mainVolumeSlider;
        [SerializeField] private TextMeshProUGUI mainVolumePercentageText;

        [Header("Music Volume Components")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumePercentageText;

        [Header("SFX Volume Components")]
        [SerializeField] private Slider SFXVolumeSlider;
        [SerializeField] private TextMeshProUGUI SFXVolumePercentageText;

        [Header("Dialog Volume Components")]
        [SerializeField] private Slider dialogVolumeSlider;
        [SerializeField] private TextMeshProUGUI dialogVolumePercentageText;

        [Header("Buttons")]
        [SerializeField] private Button returnButton;
        [SerializeField] private Button saveButton;

        private SettingsSaveData tempSettingsData;

        [Header("Button Interactions")]
        public bool north_Input = false;
        public bool south_Input = false;
        public bool west_Input = false;
        public bool east_Input = false;

        public bool back_Input = false;

        private bool eventControlsDisabled = false;

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

            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            if (settingsGraphicsMenu == null)
            {
                settingsGraphicsMenu = GameObject.FindGameObjectWithTag("GraphicMenuManager").GetComponent<GraphicMenuManager>();
            }

            currentEventSystem = EventSystem.current;

            //currentEventSystem.GetComponent<InputSystemUIInputModule>().enabled = false;

            SetAllSettingsFromLoadedSettingsData();
        }

        private void Update()
        {
            if (!settingsMenuGameObject.gameObject.activeSelf)
            {
                //if (eventControlsDisabled)
                //{
                //    //event controls need to be enabled again if they are disabled
                //    eventControlsDisabled = false;
                //    currentEventSystem.GetComponent<InputSystemUIInputModule>().enabled = true;
                //}
                //return;
            }

            if (!eventControlsDisabled)
            {
                ////event controls need to be disabled if they are enabled to prevent it applying twice.
                //eventControlsDisabled = true;
                //currentEventSystem.GetComponent<InputSystemUIInputModule>().enabled = false;
            }

            if (saveSettings)
            {
                saveSettings = false;
                SaveSettings();
            }

            if (!settingsMenuGameObject.gameObject.activeSelf) return;

            //HandleButtonNorthInput();
            //HandleButtonSouthInput();
            HandleButtonBackInput();
        }

        private void ToggleEventSystemControls()
        {
            eventControlsDisabled = true;
            currentEventSystem.GetComponent<InputSystemUIInputModule>().enabled = false;
        }

        private void HandleButtonNorthInput()
        {
            if (north_Input)
            {
                north_Input = false;

                //switch state doesnt work due to requiring a constant gameobject value.
                if(currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.resolutionOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(saveButton.gameObject);
                }
                else if (currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.screenmodeOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.resolutionOption.gameObject);
                }
                else if(currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.qualityLevelOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.screenmodeOption.gameObject);
                }
                else if(currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.mainVolumeOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.qualityLevelOption.gameObject);
                }
                else if(currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.musicVolumeOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.mainVolumeOption.gameObject);
                }
                else if(currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.sfxVolumeOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.musicVolumeOption.gameObject);
                }
                else if(currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.dialogVolumeOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.sfxVolumeOption.gameObject);
                }
                else if(currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.horizontalSensitivityOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.dialogVolumeOption.gameObject);
                }
                else if(currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.verticalSensitivityOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.horizontalSensitivityOption.gameObject);
                }
                else if (currentEventSystem.currentSelectedGameObject == returnButton.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.verticalSensitivityOption.gameObject);
                }
                else if(currentEventSystem.currentSelectedGameObject == saveButton.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(returnButton.gameObject);
                }
                else
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.resolutionOption.gameObject);
                }
            }
        }

        private void HandleButtonSouthInput()
        {
            if (south_Input)
            {
                south_Input = false;

                //switch state doesnt work due to requiring a constant gameobject value.
                if (currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.resolutionOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.screenmodeOption.gameObject);
                }
                else if (currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.screenmodeOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.qualityLevelOption.gameObject);
                }
                else if (currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.qualityLevelOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.mainVolumeOption.gameObject);
                }
                else if (currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.mainVolumeOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.musicVolumeOption.gameObject);
                }
                else if (currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.musicVolumeOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.sfxVolumeOption.gameObject);
                }
                else if (currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.sfxVolumeOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.dialogVolumeOption.gameObject);
                }
                else if (currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.dialogVolumeOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.horizontalSensitivityOption.gameObject);
                }
                else if (currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.horizontalSensitivityOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.verticalSensitivityOption.gameObject);
                }
                else if (currentEventSystem.currentSelectedGameObject == settingsGraphicsMenu.verticalSensitivityOption.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(returnButton.gameObject);
                }
                else if (currentEventSystem.currentSelectedGameObject == returnButton.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(saveButton.gameObject);
                }
                else if (currentEventSystem.currentSelectedGameObject == saveButton.gameObject)
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.resolutionOption.gameObject);
                }
                else
                {
                    currentEventSystem.SetSelectedGameObject(settingsGraphicsMenu.resolutionOption.gameObject);
                }
            }
        }

        private void HandleButtonBackInput()
        {
            if (back_Input)
            {
                back_Input = false;

                CloseSettingsMenu();
            }
        }

        private void OnEnable()
        {
            if (playerControls == null)
            {
                playerControls = new PlayerControls();
                playerControls.UI.XInteract.performed += i => saveSettings = true;
                playerControls.UI.BackInteract.performed += i => back_Input = true;

                playerControls.UI.ButtonNorth.performed += i => north_Input = true;
                playerControls.UI.ButtonWest.performed += i => west_Input = true;
                playerControls.UI.ButtonSouth.performed += i => south_Input = true;
                playerControls.UI.ButtonEast.performed += i => east_Input = true;
            }

            playerControls.Enable();
        }

        private void OnDisable()
        {
            playerControls.Disable();

            saveSettings = false;
            back_Input = false;

            north_Input = false;
            west_Input = false;
            south_Input = false;
            east_Input = false;
        }

        public void SetAllSettingsFromLoadedSettingsData()
        {
            tempSettingsData = new SettingsSaveData
            {
                mainVolume = settingsGraphicsMenu.dataToLoad.mainVolume,
                musicVolume = settingsGraphicsMenu.dataToLoad.musicVolume,
                SFXVolume = settingsGraphicsMenu.dataToLoad.sfxVolume,
                dialogVolume = settingsGraphicsMenu.dataToLoad.dialogVolume,
                horizontalSensitivity = settingsGraphicsMenu.dataToLoad.horizontalSensitivity,
                verticalSensitivity = settingsGraphicsMenu.dataToLoad.verticalSensitivity
            };

            mainVolumeSlider.value = tempSettingsData.mainVolume;
            musicVolumeSlider.value = tempSettingsData.musicVolume;
            SFXVolumeSlider.value = tempSettingsData.SFXVolume;
            dialogVolumeSlider.value = tempSettingsData.dialogVolume;
            horizontalSensitivitySlider.value = tempSettingsData.horizontalSensitivity;
            verticalSensitivitySlider.value = tempSettingsData.verticalSensitivity;

            WorldAudioVolumesManager.Instance.SetMusicAudioSourcesVolumes(musicVolumeSlider.value, mainVolumeSlider.value);
            WorldAudioVolumesManager.Instance.SetSFXAudioSourcesVolumes(SFXVolumeSlider.value, mainVolumeSlider.value);
            WorldAudioVolumesManager.Instance.SetDialogAudioSourcesVolumes(dialogVolumeSlider.value, mainVolumeSlider.value);

            PlayerCamera.instance.SetNewSensitivityFromSaveValues(horizontalSensitivitySlider.value, verticalSensitivitySlider.value);
        }

        public void SaveSettings()
        {
            settingsGraphicsMenu.Save();
            //SavedSettingsManager.instance.SaveSettings(tempSettingsData);
        }

        public void CloseSettingsMenu()
        {
            //check if settings are changed (different from save file)
            if (
                settingsGraphicsMenu.dataToLoad.mainVolume != settingsGraphicsMenu.mainVolumeOption.currentSubOption.integerValue
                || settingsGraphicsMenu.dataToLoad.musicVolume != settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue
                || settingsGraphicsMenu.dataToLoad.sfxVolume != settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue
                || settingsGraphicsMenu.dataToLoad.dialogVolume != settingsGraphicsMenu.dialogVolumeOption.currentSubOption.integerValue
                || settingsGraphicsMenu.dataToLoad.horizontalSensitivity != settingsGraphicsMenu.horizontalSensitivityOption.currentSubOption.integerValue
                || settingsGraphicsMenu.dataToLoad.verticalSensitivity != settingsGraphicsMenu.verticalSensitivityOption.currentSubOption.integerValue)
            {
                PlayerUIManager.instance.playerUIPopUpManager.DisplayAbandonChangedSettingsPopUp();
            }
            else
            {
                if (SceneManager.GetActiveScene().buildIndex == 0)
                    PlayerUIManager.instance.playerUIPopUpManager.CloseSettingsMenu(false);
                else
                    PlayerUIManager.instance.playerUIPopUpManager.CloseSettingsMenu(true);
            }
        }

        //not used
        public void ReturnToButtonPanelInGame()
        {
            //check if settings are changed (different from save file)
            if (
                tempSettingsData.mainVolume != settingsGraphicsMenu.mainVolumeOption.currentSubOption.integerValue
                || tempSettingsData.musicVolume != settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue
                || tempSettingsData.SFXVolume != settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue
                || tempSettingsData.dialogVolume != settingsGraphicsMenu.dialogVolumeOption.currentSubOption.integerValue
                || tempSettingsData.horizontalSensitivity != settingsGraphicsMenu.horizontalSensitivityOption.currentSubOption.integerValue
                || tempSettingsData.verticalSensitivity != settingsGraphicsMenu.verticalSensitivityOption.currentSubOption.integerValue)
            {
                PlayerUIManager.instance.playerUIPopUpManager.DisplayAbandonChangedSettingsPopUp();
            }
            else
                PlayerUIManager.instance.playerUIPopUpManager.CloseSettingsMenu(true);
        }

        public void UpdateMainVolumeText()
        {
            mainVolumePercentageText.text = mainVolumeSlider.value + "%";
            tempSettingsData.mainVolume = (int)mainVolumeSlider.value;

            settingsGraphicsMenu.mainVolumeOption.SetCurrentsuboptionByValue(((int)mainVolumeSlider.value));

            WorldAudioVolumesManager.Instance.SetMusicAudioSourcesVolumes(musicVolumeSlider.value, mainVolumeSlider.value);
            WorldAudioVolumesManager.Instance.SetSFXAudioSourcesVolumes(SFXVolumeSlider.value, mainVolumeSlider.value);
            WorldAudioVolumesManager.Instance.SetDialogAudioSourcesVolumes(dialogVolumeSlider.value, mainVolumeSlider.value);
        }
        public void UpdateMusicVolumeText()
        {
            musicVolumePercentageText.text = musicVolumeSlider.value + "%";
            tempSettingsData.musicVolume = (int)musicVolumeSlider.value;

            settingsGraphicsMenu.musicVolumeOption.SetCurrentsuboptionByValue(((int)musicVolumeSlider.value));

            WorldAudioVolumesManager.Instance.SetMusicAudioSourcesVolumes(musicVolumeSlider.value, mainVolumeSlider.value);
        }
        public void UpdateSFXVolumeText()
        {
            SFXVolumePercentageText.text = SFXVolumeSlider.value + "%";
            tempSettingsData.SFXVolume = (int)SFXVolumeSlider.value;

            settingsGraphicsMenu.sfxVolumeOption.SetCurrentsuboptionByValue(((int)SFXVolumeSlider.value));

            WorldAudioVolumesManager.Instance.SetSFXAudioSourcesVolumes(SFXVolumeSlider.value, mainVolumeSlider.value);
        }
        public void UpdateDialogVolumeText()
        {
            dialogVolumePercentageText.text = dialogVolumeSlider.value + "%";
            tempSettingsData.dialogVolume = (int)dialogVolumeSlider.value;

            settingsGraphicsMenu.dialogVolumeOption.SetCurrentsuboptionByValue(((int)dialogVolumeSlider.value));

            WorldAudioVolumesManager.Instance.SetDialogAudioSourcesVolumes(dialogVolumeSlider.value, mainVolumeSlider.value);
        }

        public void UpdateHorizontalSensitivityText()
        {
            if (horizontalSensitivitySlider.value == 0) horizontalSensitivityText.text = "Default";
            else
                horizontalSensitivityText.text = horizontalSensitivitySlider.value.ToString();

            tempSettingsData.horizontalSensitivity = (int)horizontalSensitivitySlider.value;

            settingsGraphicsMenu.horizontalSensitivityOption.SetCurrentsuboptionByValue(((int)horizontalSensitivitySlider.value));

            PlayerCamera.instance.SetNewSensitivityFromSaveValues(horizontalSensitivitySlider.value, verticalSensitivitySlider.value);
        }

        public void UpdateVerticalSensitivityText()
        {
            if (verticalSensitivitySlider.value == 0) verticalSensitivityText.text = "Default";
            else
                verticalSensitivityText.text = verticalSensitivitySlider.value.ToString();

            tempSettingsData.verticalSensitivity = (int)verticalSensitivitySlider.value;

            settingsGraphicsMenu.verticalSensitivityOption.SetCurrentsuboptionByValue(((int)verticalSensitivitySlider.value));

            PlayerCamera.instance.SetNewSensitivityFromSaveValues(horizontalSensitivitySlider.value, verticalSensitivitySlider.value);
        }
    }
}
