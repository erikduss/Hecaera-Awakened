using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode.Transports.UTP;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
#if UNITY_EDITOR
using ParrelSync;
#endif

namespace Erikduss
{
    public class TitleScreenManager : MonoBehaviour
    {
        public static TitleScreenManager Instance;

        [Header("Audio")]
        [SerializeField] AudioSource menuMusicAudio;

        [Header("Menus")]
        [SerializeField] public GameObject titleScreenMainMenu;
        [SerializeField] GameObject titleScreenLoadMenu;
        [SerializeField] GameObject titleScreenJoinMenu;

        [Header("Buttons")]
        [SerializeField] public Button pressToStartButton;
        [SerializeField] public Button mainMenuNewGameButton;
        [SerializeField] Button loadMenureturnButton;
        [SerializeField] Button joinMenureturnButton;
        [SerializeField] Button mainMenuLoadGameButton;
        [SerializeField] Button noCharacterSlotsOkayButton;
        [SerializeField] Button deleteCharacterPopUpConfirmButton;

        [Header("Pop Ups")]
        [SerializeField] GameObject noCharacterSlotsPopUp;
        [SerializeField] GameObject deleteCharacterSlotPopUp;

        [Header("Server Info")]
        [SerializeField] TextMeshProUGUI serverConnectStatusText;
        [SerializeField] TMP_InputField joinGameServerIPText;
        [SerializeField] TMP_InputField joinGameServerPortText;

        [Header("Character Slots")]
        public CharacterSlot currentSeletedSlot = CharacterSlot.NO_SLOT;

        [Header("PhaseSkipToggles")]
        [SerializeField] Toggle phase1SkipToggle;
        [SerializeField] Toggle phase2SkipToggle;
        [SerializeField] Toggle phase3SkipToggle;
        [SerializeField] Toggle phase4SkipToggle;

        public bool continuedPastSplashScreen = false;
        [SerializeField] private SettingsMenuManager settingsMenuManager;

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

        private void Update()
        {
            if (!continuedPastSplashScreen && ConnectionManager.Instance.authenticationFinished)
            {
                if (Input.anyKey)
                {
                    continuedPastSplashScreen = true;
                    pressToStartButton.onClick.Invoke();
                }
            }
        }

        private void Start()
        {
            SetAudioFromSoundManager();
        }

        public void ExitGame()
        {
            NetworkManager.Singleton.Shutdown();
            Application.Quit();
        }

        private void SetAudioFromSoundManager()
        {
            menuMusicAudio.clip = WorldSoundFXManager.instance.menuMusicTrack;
            menuMusicAudio.Play();
        }

        public void StartNewGame()
        {
            WorldGameSessionManager.Instance.skipPhase1 = phase1SkipToggle.isOn;
            WorldGameSessionManager.Instance.skipPhase2 = phase2SkipToggle.isOn;
            WorldGameSessionManager.Instance.skipPhase3 = phase3SkipToggle.isOn;
            WorldGameSessionManager.Instance.skipPhase4 = phase4SkipToggle.isOn;

            ConnectionManager.Instance.StartLoadingIntoGameAsHost();
        }

        public void JoinGame()
        {
            //we restart as client
            ConnectionManager.Instance.StartLoadingIntoGameAsClient(joinGameServerIPText.text);
        }

        public void OpenJoinGameMenu()
        {
            titleScreenMainMenu.SetActive(false);
            titleScreenJoinMenu.SetActive(true);

            joinMenureturnButton.Select();

            //CustomArgumentParrelsync.Instance.RetryProfileSwitch();
        }

        public void CloseJoinGameMenu()
        {
            titleScreenJoinMenu.SetActive(false);
            titleScreenMainMenu.SetActive(true);

            mainMenuLoadGameButton.Select();
        }

        public void OpenLoadGameMenu()
        {
            titleScreenMainMenu.SetActive(false);
            titleScreenLoadMenu.SetActive(true);

            loadMenureturnButton.Select();
        }

        public void CloseLoadGameMenu()
        {
            titleScreenLoadMenu.SetActive(false);
            titleScreenMainMenu.SetActive(true);

            mainMenuNewGameButton.Select();
        }

        public void DisplayNoFreeCharacterSlotsPopUp()
        {
            noCharacterSlotsPopUp.SetActive(true);
            noCharacterSlotsOkayButton.Select();
        }

        public void CloseNoFreeCharacterSlotsPopUp()
        {
            noCharacterSlotsPopUp.SetActive(false);
            mainMenuNewGameButton.Select();
        }

        public void SelectCharacterSlot(CharacterSlot characterSlot)
        {
            currentSeletedSlot = characterSlot;
        }

        public void SelectNoSlot()
        {
            currentSeletedSlot = CharacterSlot.NO_SLOT;
        }

        public void AttemptToDeleteCharacterSlot()
        {
            if (currentSeletedSlot != CharacterSlot.NO_SLOT)
            {
                deleteCharacterSlotPopUp.SetActive(true);
                deleteCharacterPopUpConfirmButton.Select();
            }
        }

        public void DeleteCharacterSlot()
        {
            deleteCharacterSlotPopUp.SetActive(false);
            WorldSaveGameManager.instance.DeleteGame(currentSeletedSlot);
            //disable and enable again to refresh slots
            titleScreenLoadMenu.SetActive(false);
            titleScreenLoadMenu.SetActive(true);

            loadMenureturnButton.Select();
        }

        public void CloseDeleteCharacterPopUp()
        {
            deleteCharacterSlotPopUp.SetActive(false);
            loadMenureturnButton.Select();
        }

        public void LaunchExperimentalScene()
        {
            SceneManager.LoadScene("LoadingToExperimental");
        }
    }
}
