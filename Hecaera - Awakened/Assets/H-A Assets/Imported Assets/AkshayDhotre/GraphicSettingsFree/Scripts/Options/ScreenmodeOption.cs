using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AkshayDhotre.GraphicSettingsMenu
{
    public class ScreenmodeOption : Option
    {
        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            //The suboption list is created in the editor for this option
            if (currentSubOption.name == "" && subOptionList.Count > 0)
            {
                currentSubOptionIndex = 0;
                currentSubOption = subOptionList[currentSubOptionIndex];
                UpdateSuboptionText();
            }
        }

        /// <summary>
        /// Applies the screenmode settings
        /// </summary>
        public override void Apply()
        {
            GraphicSettingHelperMethods.ChangeScreenMode(currentSubOption.integerValue);
        }

        /// <summary>
        /// Goes through the list of the resolution and then finds the suboption which has value equal to the input value
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

