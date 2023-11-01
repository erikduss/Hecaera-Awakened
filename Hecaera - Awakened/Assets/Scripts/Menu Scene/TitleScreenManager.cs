using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode.Transports.UTP;
using System;

public class TitleScreenManager : MonoBehaviour
{
    public static TitleScreenManager Instance;

    [Header("Audio")]
    [SerializeField] AudioSource menuMusicAudio;

    [Header("Menus")]
    [SerializeField] GameObject titleScreenMainMenu;
    [SerializeField] GameObject titleScreenLoadMenu;
    [SerializeField] GameObject titleScreenJoinMenu;
    [SerializeField] GameObject titleScreenSettingsMenu;

    [Header("Buttons")]
    [SerializeField] Button pressToStartButton;
    [SerializeField] Button mainMenuNewGameButton;
    [SerializeField] Button loadMenureturnButton;
    [SerializeField] Button joinMenureturnButton;
    [SerializeField] Button mainMenuLoadGameButton;
    [SerializeField] Button noCharacterSlotsOkayButton;
    [SerializeField] Button deleteCharacterPopUpConfirmButton;
    [SerializeField] Button returnFromSettingsButton;
    [SerializeField] Button abandonChangedSettingsConfirmButton;

    [Header("Pop Ups")]
    [SerializeField] GameObject noCharacterSlotsPopUp;
    [SerializeField] GameObject deleteCharacterSlotPopUp;
    [SerializeField] GameObject abandonChangedSettingsPopUp;

    [Header("Server Info")]
    [SerializeField] TextMeshProUGUI serverConnectStatusText;
    [SerializeField] TMP_InputField joinGameServerIPText;
    [SerializeField] TMP_InputField joinGameServerPortText;

    [Header("Character Slots")]
    public CharacterSlot currentSeletedSlot = CharacterSlot.NO_SLOT;

    public bool continuedPastSplashScreen = false;
    [SerializeField] private TitleScreenSettingsMenuManager settingsMenuManager;

    private UnityTransport networkTransport;

    public bool connectedToServer = false;

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

    private void Update()
    {
        if (!continuedPastSplashScreen)
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

        networkTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        networkTransport.OnTransportEvent += OnTransportEvent;
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

    public void StartNetworkAsHost()
    {
        continuedPastSplashScreen = true;

        UnityTransport networkTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        networkTransport.SetConnectionData("192.168.2.1", 12567, "0.0.0.0");
        //192.168.2.1
        //Available ports: 9000, 1511, 12567 (2456-2458, Valheim Server), (19132-19133, Minecraft bedrock), 25565 minecraft java

        WorldGameSessionManager.Instance.SetApprovalCheckCallback();

        NetworkManager.Singleton.StartHost();
    }

    public void StartNewGame()
    {
        WorldSaveGameManager.instance.AttemptToCreateNewGame();
    }

    public void JoinGame()
    {
        //we restart as client
        StartCoroutine(JoiningGame());
    }

    private IEnumerator JoiningGame()
    {
        serverConnectStatusText.text = string.Empty;

        if(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            //we must first shut down becaus we started as a host during the title screen.
            NetworkManager.Singleton.Shutdown();
        }

        while (NetworkManager.Singleton.ShutdownInProgress)
        {
            yield return null;
        }

        bool usingCustomServerData = false;

        //If alternate IP has been assigned.
        if (joinGameServerIPText.text.Length > 0)
        {
            usingCustomServerData = true;
            networkTransport.ConnectionData.Address = joinGameServerIPText.text;
            
            if(joinGameServerPortText.text.Length > 0)
            {
                networkTransport.ConnectionData.Port = ushort.Parse(joinGameServerPortText.text);
            }

            Debug.Log(networkTransport.ConnectionData.Address + " _: " + networkTransport.ConnectionData.Port);
        }
        else
        {
            usingCustomServerData = false;
            //networkTransport.ConnectionData.Address = "127.0.0.1";
            //networkTransport.ConnectionData.Port = 7777;

            networkTransport.SetConnectionData("86.84.11.223", 12567);
        }

        WorldGameSessionManager.Instance.SetApprovalCheckCallback();

        bool success = NetworkManager.Singleton.StartClient();

        yield return new WaitForSeconds(2.5f);
        if (!connectedToServer)
        {
            if (usingCustomServerData)
            {
                serverConnectStatusText.text = "Failed To Connect to custom server";
            }
            else
                serverConnectStatusText.text = "Failed To Connect to server";

            Debug.Log("FAILED TO CONNECT TO: " + networkTransport.ConnectionData.Address + ":" + networkTransport.ConnectionData.Port);
        }
        
        yield return null;
    }

    private void OnTransportEvent(Unity.Netcode.NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
    {
        if (eventType == NetworkEvent.TransportFailure) 
        {
            Debug.Log("Transport Failure!");
            serverConnectStatusText.text = "Network Transport Failure"; 
        }
        if (eventType == NetworkEvent.Disconnect)
        {
            Debug.Log("DISCONNECT!");
            serverConnectStatusText.text = "Network Disconnected";
        }
        if(eventType == NetworkEvent.Connect) connectedToServer = true;
    }

    public void OpenJoinGameMenu()
    {
        titleScreenMainMenu.SetActive(false);
        titleScreenJoinMenu.SetActive(true);

        joinMenureturnButton.Select();
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

    public void OpenSettingsMenu()
    {
        titleScreenMainMenu.SetActive(false);
        titleScreenSettingsMenu.SetActive(true);

        returnFromSettingsButton.Select();
    }

    public void CloseSettingsMenu()
    {
        titleScreenSettingsMenu.SetActive(false);
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
        if(currentSeletedSlot != CharacterSlot.NO_SLOT)
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

    public void RevertSettingsChanges()
    {
        abandonChangedSettingsPopUp.SetActive(false);
        //reset settings!
        settingsMenuManager.SetAllSettingsFromLoadedSettingsData();

        CloseSettingsMenu();
    }

    public void DisplayAbandonChangedSettingsPopUp()
    {
        abandonChangedSettingsPopUp.SetActive(true);
        abandonChangedSettingsConfirmButton.Select();
    }

    public void CloseAbandonChangedSettingsPopUp()
    {
        abandonChangedSettingsPopUp.SetActive(false);
        returnFromSettingsButton.Select();
    }
}
