using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenSettingsMenuManager : MonoBehaviour
{
    PlayerControls playerControls;

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

    private SettingsSaveData tempSettingsData;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        SetAllSettingsFromLoadedSettingsData();
    }

    private void Update()
    {
        if (saveSettings)
        {
            saveSettings = false;
            SaveSettings();
        }
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.UI.XInteract.performed += i => saveSettings = true;
        }

        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void SetAllSettingsFromLoadedSettingsData()
    {
        tempSettingsData = new SettingsSaveData
        {
            mainVolume = SavedSettingsManager.instance.settingsGraphicsMenu.mainVolumeOption.currentSubOption.integerValue,
            musicVolume = SavedSettingsManager.instance.settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue,
            SFXVolume = SavedSettingsManager.instance.settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue,
            dialogVolume = SavedSettingsManager.instance.settingsGraphicsMenu.dialogVolumeOption.currentSubOption.integerValue,
            horizontalSensitivity = SavedSettingsManager.instance.settingsGraphicsMenu.horizontalSensitivityOption.currentSubOption.integerValue,
            verticalSensitivity = SavedSettingsManager.instance.settingsGraphicsMenu.verticalSensitivityOption.currentSubOption.integerValue
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
        SavedSettingsManager.instance.SaveSettings(tempSettingsData);
    }

    public void ReturnToMainMenu()
    {
        //check if settings are changed (different from save file)
        if(
            tempSettingsData.mainVolume != SavedSettingsManager.instance.settingsGraphicsMenu.mainVolumeOption.currentSubOption.integerValue
            || tempSettingsData.musicVolume != SavedSettingsManager.instance.settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue
            || tempSettingsData.SFXVolume != SavedSettingsManager.instance.settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue
            || tempSettingsData.dialogVolume != SavedSettingsManager.instance.settingsGraphicsMenu.dialogVolumeOption.currentSubOption.integerValue
            || tempSettingsData.horizontalSensitivity != SavedSettingsManager.instance.settingsGraphicsMenu.horizontalSensitivityOption.currentSubOption.integerValue
            || tempSettingsData.verticalSensitivity != SavedSettingsManager.instance.settingsGraphicsMenu.verticalSensitivityOption.currentSubOption.integerValue)    
        {
            TitleScreenManager.Instance.DisplayAbandonChangedSettingsPopUp();
        }
        else
            TitleScreenManager.Instance.CloseSettingsMenu();
    }

    public void ReturnToButtonPanelInGame()
    {
        //check if settings are changed (different from save file)
        if (
            tempSettingsData.mainVolume != SavedSettingsManager.instance.settingsGraphicsMenu.mainVolumeOption.currentSubOption.integerValue
            || tempSettingsData.musicVolume != SavedSettingsManager.instance.settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue
            || tempSettingsData.SFXVolume != SavedSettingsManager.instance.settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue
            || tempSettingsData.dialogVolume != SavedSettingsManager.instance.settingsGraphicsMenu.dialogVolumeOption.currentSubOption.integerValue
            || tempSettingsData.horizontalSensitivity != SavedSettingsManager.instance.settingsGraphicsMenu.horizontalSensitivityOption.currentSubOption.integerValue
            || tempSettingsData.verticalSensitivity != SavedSettingsManager.instance.settingsGraphicsMenu.verticalSensitivityOption.currentSubOption.integerValue)
        {
            PlayerUIManager.instance.playerUIPopUpManager.DisplayAbandonChangedSettingsPopUp();
        }
        else
            PlayerUIManager.instance.playerUIPopUpManager.CloseSettingsIngameMenu();
    }

    public void UpdateMainVolumeText()
    {
        mainVolumePercentageText.text = mainVolumeSlider.value + "%";
        tempSettingsData.mainVolume = (int)mainVolumeSlider.value;

        SavedSettingsManager.instance.settingsGraphicsMenu.mainVolumeOption.SetCurrentsuboptionByValue(((int)mainVolumeSlider.value));

        WorldAudioVolumesManager.Instance.SetMusicAudioSourcesVolumes(musicVolumeSlider.value, mainVolumeSlider.value);
        WorldAudioVolumesManager.Instance.SetSFXAudioSourcesVolumes(SFXVolumeSlider.value, mainVolumeSlider.value);
        WorldAudioVolumesManager.Instance.SetDialogAudioSourcesVolumes(dialogVolumeSlider.value, mainVolumeSlider.value);
    }
    public void UpdateMusicVolumeText()
    {
        musicVolumePercentageText.text = musicVolumeSlider.value + "%";
        tempSettingsData.musicVolume = (int)musicVolumeSlider.value;

        SavedSettingsManager.instance.settingsGraphicsMenu.musicVolumeOption.SetCurrentsuboptionByValue(((int)musicVolumeSlider.value));

        WorldAudioVolumesManager.Instance.SetMusicAudioSourcesVolumes(musicVolumeSlider.value, mainVolumeSlider.value);
    }
    public void UpdateSFXVolumeText()
    {
        SFXVolumePercentageText.text = SFXVolumeSlider.value + "%";
        tempSettingsData.SFXVolume = (int)SFXVolumeSlider.value;

        SavedSettingsManager.instance.settingsGraphicsMenu.sfxVolumeOption.SetCurrentsuboptionByValue(((int)SFXVolumeSlider.value));

        WorldAudioVolumesManager.Instance.SetSFXAudioSourcesVolumes(SFXVolumeSlider.value, mainVolumeSlider.value);
    }
    public void UpdateDialogVolumeText()
    {
        dialogVolumePercentageText.text = dialogVolumeSlider.value + "%";
        tempSettingsData.dialogVolume = (int)dialogVolumeSlider.value;

        SavedSettingsManager.instance.settingsGraphicsMenu.dialogVolumeOption.SetCurrentsuboptionByValue(((int)dialogVolumeSlider.value));

        WorldAudioVolumesManager.Instance.SetDialogAudioSourcesVolumes(dialogVolumeSlider.value, mainVolumeSlider.value);
    }

    public void UpdateHorizontalSensitivityText()
    {
        if (horizontalSensitivitySlider.value == 0) horizontalSensitivityText.text = "Default";
        else
            horizontalSensitivityText.text = horizontalSensitivitySlider.value.ToString();

        tempSettingsData.horizontalSensitivity = (int)horizontalSensitivitySlider.value;

        SavedSettingsManager.instance.settingsGraphicsMenu.horizontalSensitivityOption.SetCurrentsuboptionByValue(((int)horizontalSensitivitySlider.value));

        PlayerCamera.instance.SetNewSensitivityFromSaveValues(horizontalSensitivitySlider.value, verticalSensitivitySlider.value);
    }

    public void UpdateVerticalSensitivityText()
    {
        if (verticalSensitivitySlider.value == 0) verticalSensitivityText.text = "Default";
        else
            verticalSensitivityText.text = verticalSensitivitySlider.value.ToString();

        tempSettingsData.verticalSensitivity = (int)verticalSensitivitySlider.value;

        SavedSettingsManager.instance.settingsGraphicsMenu.verticalSensitivityOption.SetCurrentsuboptionByValue(((int)verticalSensitivitySlider.value));

        PlayerCamera.instance.SetNewSensitivityFromSaveValues(horizontalSensitivitySlider.value, verticalSensitivitySlider.value);
    }
}
