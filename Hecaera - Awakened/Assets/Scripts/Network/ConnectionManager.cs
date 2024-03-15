using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;
using System;
#if UNITY_EDITOR
using ParrelSync;
#endif
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;

namespace Erikduss
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance;

        private UnityTransport networkTransport;

        public bool authenticationFinished = false;
        public bool connectedToServer = false;
        private bool finishedCreatingServerRoom = false;

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

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            networkTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            networkTransport.OnTransportEvent += OnTransportEvent;

            AuthenticatePlayer();
        }

        private void Update()
        {
            if (finishedCreatingServerRoom)
            {
                finishedCreatingServerRoom = false;
                WorldSaveGameManager.instance.AttemptToCreateNewGame();
            }
        }

        private async void AuthenticatePlayer()
        {
            //in case we go back to the menu, we want to instantly continue since we are already authenticated.
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    TitleScreenManager.Instance.continuedPastSplashScreen = true;
                    authenticationFinished = true;
                    TitleScreenManager.Instance.pressToStartButton.onClick.Invoke();

                    return;
                }
            }

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

        public void StartLoadingIntoGameAsHost()
        {
            SceneManager.LoadScene("LoadingToGameScene", LoadSceneMode.Single);
            StartNetworkAsHost();
        }

        public void StartLoadingIntoGameAsClient(string roomCode)
        {
            SceneManager.LoadScene("LoadingToGameScene", LoadSceneMode.Single);
            StartCoroutine(JoiningGame(roomCode));
        }

        public void StartNetworkAsHost()
        {
            try
            {
                TitleScreenManager.Instance.continuedPastSplashScreen = true;

                UnityTransport networkTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                networkTransport.SetConnectionData("192.168.2.1", 12567, "0.0.0.0");
                //192.168.2.1
                //Available ports: 9000, 1511, 12567 (2456-2458, Valheim Server), (19132-19133, Minecraft bedrock), 25565 minecraft java

                WorldGameSessionManager.Instance.SetApprovalCheckCallback();

                StartCoroutine(ConfigureTransportAndStartNgoAsHost());
                //NetworkManager.Singleton.StartHost();
            }
            catch
            {
                //we fail to connect, we go back.
                NetworkManager.Singleton.SceneManager.LoadScene("Scene_Main_Menu_01", LoadSceneMode.Single);
            }
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

            finishedCreatingServerRoom = true;
            yield return null;
        }

        private async void GetRelayRoomKey()
        {
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(WorldGameSessionManager.AllocationInstance.AllocationId);
            Debug.Log("Join Code: " + joinCode);
            PlayerUIManager.instance.playerUIHudManager.joinCodeText.text = "Join Code: " + joinCode;
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

        public IEnumerator JoiningGame(string joinCode)
        {
            //serverConnectStatusText.text = string.Empty;

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
            var clientRelayUtilityTask = JoinRelayServerFromJoinCode(joinCode);

            PlayerUIManager.instance.playerUIHudManager.joinCodeText.text = "Join Code: " + joinCode;

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
        }

        private void OnTransportEvent(Unity.Netcode.NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
        {
            if (eventType == NetworkEvent.TransportFailure)
            {
                Debug.Log("Transport Failure!");
                //serverConnectStatusText.text = "Network Transport Failure";
            }
            if (eventType == NetworkEvent.Disconnect)
            {
                Debug.Log("DISCONNECT!");
                //serverConnectStatusText.text = "Network Disconnected";
            }
            if (eventType == NetworkEvent.Connect) connectedToServer = true;
        }
    }
}
