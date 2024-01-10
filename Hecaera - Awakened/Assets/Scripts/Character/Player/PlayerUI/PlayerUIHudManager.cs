using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIHudManager : MonoBehaviour
{
    [SerializeField] UI_StatBar healthBar;
    [SerializeField] UI_StatBar staminaBar;
    [SerializeField] public TextMeshProUGUI joinCodeText;

    public bool fadingAlphaOfStaminaPanel = false;
    [SerializeField] GameObject stamina_Low_Panel;
    [SerializeField] Image stamina_Low_Panel_Image;

    private void Start()
    {
        stamina_Low_Panel.SetActive(false);
    }

    public IEnumerator FadeAlphaOfStaminaPanel()
    {
        //If the toggle returns true, fade in the Image
        if (fadingAlphaOfStaminaPanel == true)
        {
            stamina_Low_Panel.SetActive(true);

            //Fully fade in Image with the duration of 1 second
            stamina_Low_Panel_Image.CrossFadeAlpha(0.25f, 1.0f, false);
        }
        //If the toggle is false, fade out to nothing (0) the Image with a duration of 2
        if (fadingAlphaOfStaminaPanel == false)
        {
            stamina_Low_Panel_Image.CrossFadeAlpha(0, 1.0f, false);

            yield return new WaitForSeconds(1.0f);

            stamina_Low_Panel.SetActive(false);
        }
    }

    public void RefreshHUD()
    {
        healthBar.gameObject.SetActive(false);
        healthBar.gameObject.SetActive(true);
        staminaBar.gameObject.SetActive(false);
        staminaBar.gameObject.SetActive(true);
    }

    public void SetNewHealthValue(int oldValue, int newValue)
    {
        healthBar.SetStat(newValue);
    }

    public void SetMaxHealthValue(int maxHealth)
    {
        healthBar.SetMaxStat(maxHealth);
    }

    public void SetNewStaminaValue(float oldValue, float newValue)
    {
        staminaBar.SetStat(Mathf.RoundToInt(newValue));
    }

    public void SetMaxStaminaValue(int maxStamina)
    {
        staminaBar.SetMaxStat(maxStamina);
    }
}
