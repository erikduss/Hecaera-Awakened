using AkshayDhotre.GraphicSettingsMenu;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntValueOption : Option
{
    [SerializeField] private string valueName;
    [SerializeField] private int maxValue;
    [SerializeField] private int minValue;

    private void Awake()
    {
        Initialize();
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
        
        for(int i = minValue; i< maxValue+1; i++)
        {
            SubOption t = new SubOption();
            t.name = valueName + "_" + i.ToString();
            t.indexInList = i;
            t.integerValue = i;
            subOptionList.Add(t);
        }
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
