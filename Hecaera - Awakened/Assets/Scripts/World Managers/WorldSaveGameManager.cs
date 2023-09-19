using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldSaveGameManager : MonoBehaviour
{
    private static WorldSaveGameManager _instance;
    public static WorldSaveGameManager instance { get { return _instance; } }

    [SerializeField] private PlayerManager player;

    [Header("Save/Load")]
    [SerializeField] bool saveGame;
    [SerializeField] bool loadGame;

    [Header("World Scene Index")]
    [SerializeField] int worldSceneIndex = 1;

    [Header("Save Data Writer")]
    private SaveFileDataWriter saveFileDataWriter;

    [Header("Current Character Data")]
    public CharacterSlot currentCharacterSlotBeingUsed;
    public CharacterSaveData currentCharacterData;
    private string saveFileName;

    [Header("Character Slots")]
    public CharacterSaveData characterSlot01;
    public CharacterSaveData characterSlot02;
    public CharacterSaveData characterSlot03;
    public CharacterSaveData characterSlot04;
    public CharacterSaveData characterSlot05;
    public CharacterSaveData characterSlot06;
    public CharacterSaveData characterSlot07;
    public CharacterSaveData characterSlot08;
    public CharacterSaveData characterSlot09;
    public CharacterSaveData characterSlot10;

    private void Awake()
    {
        //THERE CAN ONLY BE ONE INSTANCE OF THIS SCRIPT AT ONE TIME
        if(instance == null)
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
        LoadAllCharacterProfiles();
    }

    private void Update()
    {
        if (saveGame)
        {
            saveGame = false;
            SaveGame();
        }

        if (loadGame)
        {
            loadGame = false;
            LoadGame();
        }
    }

    public string DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot characterSlot)
    {
        string fileName = "";

        switch(characterSlot)
        {
            case CharacterSlot.CharacterSlot_01:
                fileName = "CharacterSlow_01";
                break; 
            case CharacterSlot.CharacterSlot_02:
                fileName = "CharacterSlow_02";
                break; 
            case CharacterSlot.CharacterSlot_03:
                fileName = "CharacterSlow_03";
                break;
            case CharacterSlot.CharacterSlot_04:
                fileName = "CharacterSlow_04";
                break;
            case CharacterSlot.CharacterSlot_05:
                fileName = "CharacterSlow_05";
                break;
            case CharacterSlot.CharacterSlot_06:
                fileName = "CharacterSlow_06";
                break;
            case CharacterSlot.CharacterSlot_07:
                fileName = "CharacterSlow_07";
                break;
            case CharacterSlot.CharacterSlot_08:
                fileName = "CharacterSlow_08";
                break;
            case CharacterSlot.CharacterSlot_09:
                fileName = "CharacterSlow_09";
                break;
            case CharacterSlot.CharacterSlot_10:
                fileName = "CharacterSlow_10";
                break;
        }

        return fileName;
    }

    public void CreateNewGame()
    {
        //Create a new file, with a file name based on which slot we use.
        saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);

        currentCharacterData = new CharacterSaveData();
    }

    public void LoadGame()
    {
        //Load a previous file, with a file name depending on the slot.
        saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);

        saveFileDataWriter = new SaveFileDataWriter();
        //Generally works on multiple machine types.
        saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        saveFileDataWriter.saveFileName = saveFileName;
        currentCharacterData = saveFileDataWriter.LoadSaveFile();

        StartCoroutine(LoadWorldScene());
    }

    public void SaveGame()
    {
        //Save the current  file
        saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);

        saveFileDataWriter = new SaveFileDataWriter();
        //Generally works on multiple machine types.
        saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        saveFileDataWriter.saveFileName = saveFileName;

        player.SaveGameDataToCurrentCharacterData(ref currentCharacterData);

        //Overrides if already exists
        saveFileDataWriter.CreateNewCharacterSaveFile(currentCharacterData);
    }

    //load all character profiles on device when starting game
    private void LoadAllCharacterProfiles()
    {
        saveFileDataWriter = new SaveFileDataWriter();
        saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;

        saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_01);
        characterSlot01 = saveFileDataWriter.LoadSaveFile();

        saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_02);
        characterSlot02 = saveFileDataWriter.LoadSaveFile();

        saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_03);
        characterSlot03 = saveFileDataWriter.LoadSaveFile();

        saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_04);
        characterSlot04 = saveFileDataWriter.LoadSaveFile();

        saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_05);
        characterSlot05 = saveFileDataWriter.LoadSaveFile();

        saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_06);
        characterSlot06 = saveFileDataWriter.LoadSaveFile();

        saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_07);
        characterSlot07 = saveFileDataWriter.LoadSaveFile();

        saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_08);
        characterSlot08 = saveFileDataWriter.LoadSaveFile();

        saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_09);
        characterSlot09 = saveFileDataWriter.LoadSaveFile();

        saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_10);
        characterSlot10 = saveFileDataWriter.LoadSaveFile();
    }

    public IEnumerator LoadWorldScene()
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(worldSceneIndex);

        yield return null;
    }

    public int GetWorldSceneIndex()
    {
        return worldSceneIndex;
    }
}
