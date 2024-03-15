using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class SettingsSaveData
    {
        [Header("Sensitivity")]
        public int horizontalSensitivity = 0; //Goes between -100 and 100, -100 meaning half the sensitivity of the default. 100 meaning default + half
        public int verticalSensitivity = 0; //Goes between -100 and 100

        [Header("Volumes")]
        public int mainVolume = 50;
        public int musicVolume = 25;
        public int SFXVolume = 50;
        public int dialogVolume = 50;
    }
}
