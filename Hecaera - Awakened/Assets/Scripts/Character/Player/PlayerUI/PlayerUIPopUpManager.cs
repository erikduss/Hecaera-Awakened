using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUIPopUpManager : MonoBehaviour
{
    [Header("YOU DIED Pop Up")]
    [SerializeField] GameObject youDiedPopUpGameObject;
    [SerializeField] TextMeshProUGUI youDiedPopUpBackgroundText;
    [SerializeField] TextMeshProUGUI youDiedPopUpText;
    [SerializeField] CanvasGroup youDiedPopUpCanvasGroup;

    [Header("Menu Buttons Pop Up")]
    public bool buttonsMenuIsOpen = false;
    [SerializeField] GameObject menuButtonsPopUpGameObject;
    [SerializeField] Button returnToGameButton;
    [SerializeField] GameObject buttonsLayoutGroupGameObject;

    [Header("Settings Ingame Menu")]
    [SerializeField] SettingsMenuManager settingsMenuManager;
    [SerializeField] GameObject settingsPopUpGameObject;
    [SerializeField] GameObject abandonChangedSettingsPopUp;
    [SerializeField] Button abandonChangedSettingsConfirmButton;
    [SerializeField] Button returnToButtonsMenuButton;

    private void Start()
    {
        if (menuButtonsPopUpGameObject.activeSelf)
        {
            menuButtonsPopUpGameObject.SetActive(false);
            buttonsMenuIsOpen = false;
        }
    }

    public void SendYouDiedPopUp()
    {
        youDiedPopUpGameObject.SetActive(true);
        youDiedPopUpBackgroundText.characterSpacing = 0;
        StartCoroutine(StretchPopUpTextOverTime(youDiedPopUpBackgroundText, 8, 19f));
        StartCoroutine(FadeInPopUpOverTimer(youDiedPopUpCanvasGroup, 5));
        StartCoroutine(WaitThenFadeOutPopUpOverTime(youDiedPopUpCanvasGroup, 2, 5));
    }

    private IEnumerator StretchPopUpTextOverTime(TextMeshProUGUI text, float duration, float stretchAmount)
    {
        if (duration > 0f)
        {
            text.characterSpacing = 0;
            float timer = 0;
            yield return null;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                text.characterSpacing = Mathf.Lerp(text.characterSpacing, stretchAmount, duration * (Time.deltaTime / 20));
                yield return null;
            }
        }
    }

    private IEnumerator FadeInPopUpOverTimer(CanvasGroup canvas, float duration)
    {
        if(duration > 0f)
        {
            canvas.alpha = 0;
            float timer = 0;

            yield return null;

            while(timer < duration)
            {
                timer += Time.deltaTime;
                canvas.alpha = Mathf.Lerp(canvas.alpha, 1, duration * Time.deltaTime);
                yield return null;
            }
        }

        canvas.alpha = 1;

        yield return null;
    }

    private IEnumerator WaitThenFadeOutPopUpOverTime(CanvasGroup canvas, float duration, float delay)
    {
        if (duration > 0f)
        {
            while (delay > 0f)
            {
                delay -= Time.deltaTime;
                yield return null;
            }

            canvas.alpha = 1;
            float timer = 0;

            yield return null;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                canvas.alpha = Mathf.Lerp(canvas.alpha, 0, duration * Time.deltaTime);
                yield return null;
            }
        }

        canvas.alpha = 0;

        yield return null;
    }

    public void MenuButtonsActiveToggle()
    {
        if (menuButtonsPopUpGameObject.activeSelf)
        {
            //First check if options menu is open
            if (settingsPopUpGameObject.activeSelf)
            {
                //Make sure to revert changed settings
                RevertSettingsChanges();
            }

            menuButtonsPopUpGameObject.SetActive(false);
            buttonsMenuIsOpen = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            menuButtonsPopUpGameObject.SetActive(true);
            buttonsMenuIsOpen = true;
            returnToGameButton.Select(); //Always start at the select button.
        }
    }

    public void OpenSettingsIngameMenu()
    {
        buttonsLayoutGroupGameObject.SetActive(false);
        settingsPopUpGameObject.SetActive(true);
        returnToButtonsMenuButton.Select();
    }

    public void CloseSettingsIngameMenu()
    {
        buttonsLayoutGroupGameObject.SetActive(true);
        settingsPopUpGameObject.SetActive(false);
        returnToGameButton.Select();
    }

    public void DisplayAbandonChangedSettingsPopUp()
    {
        abandonChangedSettingsPopUp.SetActive(true);
        abandonChangedSettingsConfirmButton.Select();
    }

    public void CloseAbandonChangedSettingsPopUp()
    {
        abandonChangedSettingsPopUp.SetActive(false);
        returnToButtonsMenuButton.Select();
    }

    public void RevertSettingsChanges()
    {
        abandonChangedSettingsPopUp.SetActive(false);
        //reset settings!
        settingsMenuManager.SetAllSettingsFromLoadedSettingsData();

        CloseSettingsIngameMenu();
    }

    public void ReturnToLobby()
    {
        //Close menu, otherwise it can still be interacted with when loading back to lobby scene.
        MenuButtonsActiveToggle();

        NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene("LoadingToMainMenu");
        //SceneManager.LoadScene("Scene_Main_Menu_01");

        //ExitGame(); //dont have a way to reset things yet.
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        Application.Quit();
    }
}
