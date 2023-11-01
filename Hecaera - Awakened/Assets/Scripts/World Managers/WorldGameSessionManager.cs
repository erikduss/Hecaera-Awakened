using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class WorldGameSessionManager : MonoBehaviour
{
    public static WorldGameSessionManager Instance;

    [Header("Active Players In Session")]
    public List<PlayerManager> players = new List<PlayerManager>();

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

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SetApprovalCheckCallback()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCheck;
    }

    public void AddPlayerToActivePlayersList(PlayerManager player)
    {
        if(!players.Contains(player))
        {
            players.Add(player);
        }

        //Check the list for null slots
        for(int i = players.Count-1; i > -1; i--)
        {
            if (players[i] == null)
            {
                players.RemoveAt(i);
            }
        }
    }

    public void RemovePlayerFromActivePlayersList(PlayerManager player)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
        }

        //Check the list for null slots
        for (int i = players.Count - 1; i > -1; i--)
        {
            if (players[i] == null)
            {
                players.RemoveAt(i);
            }
        }
    }

    public void HealLocalPlayerToFull()
    {
        PlayerManager localPlayer = players.Where(a => a.IsLocalPlayer).FirstOrDefault();
        localPlayer.playerNetworkManager.currentHealth.Value = localPlayer.playerNetworkManager.maxHealth.Value;
        localPlayer.playerNetworkManager.currentStamina.Value = localPlayer.playerNetworkManager.maxHealth.Value;
    }

    private void ConnectionApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log("Approving connection request!");

        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;

        // Your approval logic determines the following values
        response.Approved = true;
        response.CreatePlayerObject = true;

        // The Prefab hash value of the NetworkPrefab, if null the default NetworkManager player Prefab is used
        response.PlayerPrefabHash = null;

        // Position to spawn the player object (if null it uses default of Vector3.zero)
        response.Position = Vector3.zero;

        // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        response.Rotation = Quaternion.identity;

        // If additional approval steps are needed, set this to true until the additional steps are complete
        // once it transitions from true to false the connection approval response will be processed.
        response.Pending = false;
    }
}
