using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsSaveData
{
    [Header("Sensitivity")]
    public float horizontalSensitivity = 0f; //Goes between -100 and 100, -100 meaning half the sensitivity of the default. 100 meaning default + half
    public float verticalSensitivity = 0f; //Goes between -100 and 100

    [Header("Volumes")]
    public float mainVolume = 50f;
    public float musicVolume = 25f;
    public float SFXVolume = 50f;
    public float dialogVolume = 50f;
}
