using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Erikduss
{
    public class UI_StatBar : MonoBehaviour
    {
        private Slider slider;
        private RectTransform rectTransform;

        [Header("Bar Options")]
        [SerializeField] protected bool scaleBarLengthWithStats = true;
        [SerializeField] protected float widthScaleMultiplier = 1f;

        protected virtual void Awake()
        {
            slider = GetComponent<Slider>();
            rectTransform = GetComponent<RectTransform>();
        }

        public virtual void SetStat(int newValue)
        {
            slider.value = newValue;
        }

        public virtual void SetMaxStat(int maxValue)
        {
            slider.maxValue = maxValue;
            slider.value = maxValue;

            if (scaleBarLengthWithStats)
            {
                float newBarSizeValue = maxValue * widthScaleMultiplier;

                //make sure the bar doesnt become huge.
                if (newBarSizeValue > 1000) newBarSizeValue = 1000;

                rectTransform.sizeDelta = new Vector2(newBarSizeValue, rectTransform.sizeDelta.y);
                //Resets the position of the bars based on their layout settings.
                PlayerUIManager.instance.playerUIHudManager.RefreshHUD();
            }
        }
    }
}
