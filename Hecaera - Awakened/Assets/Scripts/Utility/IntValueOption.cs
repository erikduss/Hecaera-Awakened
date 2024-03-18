using AkshayDhotre.GraphicSettingsMenu;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Erikduss
{
    public class IntValueOption : Option
    {
        [SerializeField] private string valueName;
        [SerializeField] private int maxValue;
        [SerializeField] private int minValue;

        [SerializeField] private Slider valueSlider;

        protected override void Awake()
        {
            //Initialize();
        }

        public void Initialize()
        {
            GenerateSuboptions();

            base.canCycleBack = false;

            //If currentSuboption is null, assign the first element from the sub option list
            if (currentSubOption.name == "" && subOptionList.Count > 0)
            {
                currentSubOptionIndex = 0;
                currentSubOption = subOptionList[0];
                UpdateSuboptionText();
            }
        }

        public override void Apply()
        {
            //GraphicSettingHelperMethods.ChangeQualitySettings(currentSubOption.integerValue);
        }

        /// <summary>
        /// Get the available quality levels from the QualitySettings and create a suboption list from it
        /// </summary>
        private void GenerateSuboptions()
        {
            //Unity stores the quality level from Edit/ProjectSettings/Quality in QualitySettings.names according to thier integer value

            for (int i = minValue; i < maxValue + 1; i++)
            {
                SubOption t = new SubOption();
                t.name = valueName + "_" + i.ToString();
                t.indexInList = i;
                t.integerValue = i;
                subOptionList.Add(t);
            }
        }

        public override void SelectNextSubOption()
        {
            //Prevent cycle if disabled
            if (currentSubOptionIndex >= (subOptionList.Count - 1))
            {
                if (!canCycleBack) return;
            }

            currentSubOptionIndex = GetNextValue(0,0);

            int fixedIndex = currentSubOptionIndex + Mathf.Abs(minValue); //The index needs to consider there can be minus values but not minus indexes.

            currentSubOption = subOptionList[fixedIndex];
            UpdateSuboptionText();

            valueSlider.value = currentSubOption.integerValue;
        }
        public override void SelectPreviousSubOption()
        {
            //Prevent cycle if disabled
            if (currentSubOptionIndex <= minValue)
            {
                if (!canCycleBack) return;
            }

            currentSubOptionIndex = GetPreviousValue(0,0);

            int fixedIndex = currentSubOptionIndex + Mathf.Abs(minValue); //The index needs to consider there can be minus values but not minus indexes.

            currentSubOption = subOptionList[fixedIndex];
            UpdateSuboptionText();

            valueSlider.value = currentSubOption.integerValue;
        }

        protected override int GetNextValue(int currentVal, int maxVal)
        {
            //Dont move further or cycle back when reaching the max
            if (currentSubOptionIndex >= maxValue)
            {
                return currentSubOptionIndex;
            }
            else
            {
                return currentSubOptionIndex + 1;
            }
        }

        protected override int GetPreviousValue(int currentVal, int maxVal)
        {
            //Done move back or cycle forward if reaching the minimum.
            if (currentSubOptionIndex == minValue)
                return minValue;

            return currentSubOptionIndex - 1;
        }

    /// <summary>
    /// Goes through the list of the suboptions and then finds the suboption which has value equal to the input value
    /// and assigns that sub option as the current sub option
    /// </summary>
    /// <param name="v"></param>
    public void SetCurrentsuboptionByValue(int v)
        {
            if (subOptionList.Count > 0)
            {
                foreach (var t in subOptionList)
                {
                    if (t.integerValue == v)
                    {
                        currentSubOption = t;
                        currentSubOptionIndex = t.indexInList;
                        UpdateSuboptionText();
                        return;
                    }
                }

                //If no item is found then we use the fall back option
                Debug.LogWarning("Suboption with value : " + v + " ,not found in :" + gameObject.name + ",using fallback option instead");
                currentSubOption = fallBackOption;
                currentSubOptionIndex = fallBackOption.indexInList;
            }
            else
            {
                Debug.LogError("No items in suboption list : " + gameObject.name);
            }
        }
    }
}
