using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetAndDestroyAllDontDestroys : MonoBehaviour
{
    public bool startedDestroying = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyEverything());
    }

    // Update is called once per frame
    void Update()
    {
        if (!startedDestroying)
        {
            startedDestroying = true;
            StartCoroutine(DestroyEverything());
        }
    }

    private IEnumerator DestroyEverything()
    {
        startedDestroying = true;

        Destroy(WorldActionManager.Instance.gameObject);
        //world connection manager
        Destroy(ConnectionManager.Instance.gameObject);
        //world network manager
        Destroy(WorldGameSessionManager.Instance.gameObject);
        //player ui manager
        Destroy(PlayerUIManager.instance.gameObject);
        //player input manager
        Destroy(PlayerInputManager.instance.gameObject);
        //player camera
        Destroy(PlayerCamera.instance.gameObject);
        //world souns fx manager
        Destroy(WorldSoundFXManager.instance.gameObject);
        //world item database
        Destroy(WorldItemDatabase.Instance.gameObject);
        //world projectile manager
        Destroy(WorldProjectilesManager.Instance.gameObject);
        //world action manager
        Destroy(WorldActionManager.Instance.gameObject);
        //player material manager
        Destroy(PlayerMaterialManagement.Instance.gameObject);
        //this
        Destroy(WorldSaveGameManager.instance.gameObject);

        yield return new WaitForSeconds(1);

        if (WorldActionManager.Instance == null && ConnectionManager.Instance == null && WorldGameSessionManager.Instance == null
            && PlayerUIManager.instance == null && PlayerInputManager.instance == null && PlayerCamera.instance == null
            && WorldSoundFXManager.instance == null && WorldItemDatabase.Instance == null && WorldProjectilesManager.Instance == null
            && WorldActionManager.Instance == null && PlayerMaterialManagement.Instance == null && WorldSaveGameManager.instance == null)
        {
            SceneManager.LoadScene(0); //go back to splash screen
        }
        else
        {
            startedDestroying = false;
            yield return null;
        }
    }
}
