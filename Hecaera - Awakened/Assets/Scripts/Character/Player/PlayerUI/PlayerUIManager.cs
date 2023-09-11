using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerUIManager : MonoBehaviour
{
    private static PlayerUIManager _instance;
    public static PlayerUIManager instance { get { return _instance; } }

    [Header("NETWORK JOIN")]
    [SerializeField] bool startGameAsClient;

    private void Awake()
    {
        if (instance == null)
        {
            _instance = this;
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

    private void Update()
    {
        if (startGameAsClient)
        {
            startGameAsClient = false;
            //we must first shut down becaus we started as a host during the title screen.
            NetworkManager.Singleton.Shutdown();
            //we restart as client
            NetworkManager.Singleton.StartClient();
        }
    }
}
