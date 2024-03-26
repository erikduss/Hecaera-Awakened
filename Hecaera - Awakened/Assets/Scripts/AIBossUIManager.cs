using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Erikduss
{
    public class AIBossUIManager : CharacterUIManager
    {
        [Header("Properties")]
        [SerializeField] private static float defaultDuration = 1f;

        [Header("UI")]
        [SerializeField] private GameObject attackIndicatorParent;
        [SerializeField] private Image attackIndicatorInnerImage;
        [SerializeField] private Image attackIndicatorOuterImage;

        [Header("Attack Indicator Colors")]
        [ColorUsage(true,true)]
        [SerializeField] private Color normalAttackColor = Color.yellow;
        [ColorUsage(true, true)]
        [SerializeField] private Color indicatorAttackColor = Color.red;
        [ColorUsage(true, true)]
        [SerializeField] private Color chargeAttackColor = Color.blue;
        [ColorUsage(true, true)]
        private Color currentlyUsedColor;

        public void ActivateAttackIndicator(AttackIndicatorType attackType, float fadeInTime, float timeAtFullOpacity = 0.5f)
        {
            attackIndicatorParent.SetActive(true);
            StartCoroutine(FadeIndicatorOpacity(0,1,fadeInTime, attackType));

            StartCoroutine(DeactivateAttackIndicator(attackType, fadeInTime, fadeInTime+timeAtFullOpacity));
        }

        private IEnumerator DeactivateAttackIndicator(AttackIndicatorType attackType, float duration, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            StartCoroutine(FadeIndicatorOpacity(1, 0, duration, attackType));

            yield return new WaitForSeconds(duration);
            attackIndicatorParent.SetActive(false);
        }

        private IEnumerator FadeIndicatorOpacity(float startAlpha, float endAlpha, float time, AttackIndicatorType attackType)
        {
            if (time > 0f)
            {
                switch (attackType)
                {
                    case AttackIndicatorType.YELLOW_NORMAL:
                            currentlyUsedColor = normalAttackColor;
                        break;
                    case AttackIndicatorType.RED_INDICATED:
                            currentlyUsedColor = indicatorAttackColor;
                        break;
                    case AttackIndicatorType.BLUE_CHARGE:
                            currentlyUsedColor = chargeAttackColor;
                        break;
                    default:
                            currentlyUsedColor = normalAttackColor;
                        break;
                }

                currentlyUsedColor = new Color(currentlyUsedColor.r, currentlyUsedColor.g, currentlyUsedColor.b, startAlpha);
                //Set the color right away;
                attackIndicatorInnerImage.color = currentlyUsedColor;
                attackIndicatorOuterImage.color = currentlyUsedColor;

                float timer = 0;

                while (timer < time)
                {
                    timer += Time.deltaTime;
                    float newAlpha = Mathf.Lerp(currentlyUsedColor.a, endAlpha, time * Time.deltaTime);
                    currentlyUsedColor = new Color(currentlyUsedColor.r, currentlyUsedColor.g, currentlyUsedColor.b, newAlpha);

                    attackIndicatorInnerImage.color = currentlyUsedColor;
                    attackIndicatorOuterImage.color = currentlyUsedColor;

                    yield return null;
                }
            }

            //If we have no timer we set it instantly
            currentlyUsedColor = new Color(currentlyUsedColor.r, currentlyUsedColor.g, currentlyUsedColor.b, endAlpha);
            attackIndicatorInnerImage.color = currentlyUsedColor;
            attackIndicatorOuterImage.color = currentlyUsedColor;

            yield return null;
        }
    }
}
