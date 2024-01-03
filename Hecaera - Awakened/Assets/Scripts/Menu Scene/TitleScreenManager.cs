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

    private bool authenticationFinished = false;

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
        if (!continuedPastSplashScreen && authenticationFinished)
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

        AuthenticatePlayer();
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

        StartCoroutine(ConfigureTransportAndStartNgoAsHost());
        //NetworkManager.Singleton.StartHost();
    }

    private IEnumerator ConfigureTransportAndStartNgoAsHost()
    {
        var serverRelayUtilityTask = WorldGameSessionManager.AllocateRelayServerAndGetJoinCode(10);
        while (!serverRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }
        if (serverRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
            yield break;
        }

        var relayServerData = serverRelayUtilityTask.Result;

        // Display the joinCode to the user.
        GetRelayRoomKey();

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();
        yield return null;
    }

    private async void GetRelayRoomKey()
    {
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(WorldGameSessionManager.AllocationInstance.AllocationId);
        Debug.Log("Join Code: " + joinCode);
        PlayerUIManager.instance.playerUIHudManager.joinCodeText.text = "Join Code: " + joinCode;
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

    private async void AuthenticatePlayer()
    {
        #if UNITY_EDITOR
        //Is this unity editor instance opening a clone project?
        if (ClonesManager.IsClone())
        {
            Debug.Log("This is a clone project.");
            // Get the custom argument for this clone project.  
            string customArgument = ClonesManager.GetArgument();
            // Do what ever you need with the argument string.
            Debug.Log("The custom argument of this clone project is: " + customArgument);

            InitializationOptions options = new InitializationOptions();
            options.SetProfile("Clone_" + customArgument + "_Profile");

            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var playerID = AuthenticationService.Instance.PlayerId;

            authenticationFinished = true;
            //AuthenticationService.Instance.SwitchProfile("Clone_" + customArgument + "_Profile");
        }
        else
        {
            Debug.Log("This is the original project.");

            InitializationOptions options = new InitializationOptions();
            options.SetProfile("Main_Profile");

            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            var playerID = AuthenticationService.Instance.PlayerId;

            authenticationFinished = true;
        }

        Debug.Log(AuthenticationService.Instance.PlayerId);

        if (authenticationFinished) return;
        #endif

        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            var playerID = AuthenticationService.Instance.PlayerId;

            authenticationFinished = true;

            Debug.Log(playerID);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public static async Task<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
    {
        JoinAllocation allocation;
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        Debug.Log($"client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
        Debug.Log($"client: {allocation.AllocationId}");

        return new RelayServerData(allocation, "dtls");
    }

    private IEnumerator JoiningGame()
    {
        serverConnectStatusText.text = string.Empty;

        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            //we must first shut down becaus we started as a host during the title screen.
            NetworkManager.Singleton.Shutdown();
        }

        while (NetworkManager.Singleton.ShutdownInProgress)
        {
            yield return null;
        }

        // Populate RelayJoinCode beforehand through the UI
        var clientRelayUtilityTask = JoinRelayServerFromJoinCode(joinGameServerIPText.text);

        while (!clientRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }

        if (clientRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to connect to Relay Server. Exception: " + clientRelayUtilityTask.Exception.Message);
            yield break;
        }

        var relayServerData = clientRelayUtilityTask.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();
        yield return null;

        //bool usingCustomServerData = false;

        ////If alternate IP has been assigned.
        //if (joinGameServerIPText.text.Length > 0)
        //{
        //    usingCustomServerData = true;
        //    networkTransport.ConnectionData.Address = joinGameServerIPText.text;

        //    if (joinGameServerPortText.text.Length > 0)
        //    {
        //        networkTransport.ConnectionData.Port = ushort.Parse(joinGameServerPortText.text);
        //    }

        //    Debug.Log(networkTransport.ConnectionData.Address + " _: " + networkTransport.ConnectionData.Port);
        //}
        //else
        //{
        //    usingCustomServerData = false;
        //    //networkTransport.ConnectionData.Address = "127.0.0.1";
        //    //networkTransport.ConnectionData.Port = 7777;

        //    networkTransport.SetConnectionData("86.84.11.223", 12567);
        //}

        ////await AuthenticationService.Instance.SignInAnonymouslyAsync();

        //bool success = NetworkManager.Singleton.StartClient();

        //Debug.Log("Managed to start client? " + success);

        //yield return new WaitForSeconds(2.5f);
        //if (!connectedToServer)
        //{
        //    if (usingCustomServerData)
        //    {
        //        serverConnectStatusText.text = "Failed To Connect to custom server";
        //    }
        //    else
        //        serverConnectStatusText.text = "Failed To Connect to server";

        //    Debug.Log("FAILED TO CONNECT TO: " + networkTransport.ConnectionData.Address + ":" + networkTransport.ConnectionData.Port);
        //}

        //yield return null;
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
