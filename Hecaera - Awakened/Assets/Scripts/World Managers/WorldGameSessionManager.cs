using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
}
