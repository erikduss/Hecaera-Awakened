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

    [HideInInspector] public PlayerUIHudManager playerUIHudManager;
    [HideInInspector] public PlayerUIPopUpManager playerUIPopUpManager;

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

        playerUIHudManager = GetComponentInChildren<PlayerUIHudManager>();
        playerUIPopUpManager = GetComponentInChildren<PlayerUIPopUpManager>();
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
            TitleScreenManager.Instance.JoinGame();
        }
    }
}
