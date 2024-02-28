using AkshayDhotre.GraphicSettingsMenu;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenuManager : MonoBehaviour
{
    public static SettingsMenuManager Instance;

    PlayerControls playerControls;

    public GraphicMenuManager settingsGraphicsMenu;

    public GameObject settingsMenuGameObject;

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
            mainVolume = settingsGraphicsMenu.mainVolumeOption.currentSubOption.integerValue,
            musicVolume = settingsGraphicsMenu.musicVolumeOption.currentSubOption.integerValue,
            SFXVolume = settingsGraphicsMenu.sfxVolumeOption.currentSubOption.integerValue,
            dialogVolume = settingsGraphicsMenu.dialogVolumeOption.currentSubOption.integerValue,
            horizontalSensitivity = settingsGraphicsMenu.horizontalSensitivityOption.currentSubOption.integerValue,
            verticalSensitivity = settingsGraphicsMenu.verticalSensitivityOption.currentSubOption.integerValue
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
        if(
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
        {
            if(SceneManager.GetActiveScene().buildIndex == 0)
                PlayerUIManager.instance.playerUIPopUpManager.CloseSettingsMenu(false);
            else
                PlayerUIManager.instance.playerUIPopUpManager.CloseSettingsMenu(true);
        }
    }

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
