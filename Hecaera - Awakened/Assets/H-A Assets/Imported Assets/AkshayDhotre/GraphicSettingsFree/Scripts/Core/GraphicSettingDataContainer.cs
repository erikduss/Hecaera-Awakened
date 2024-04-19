using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

namespace AkshayDhotre.GraphicSettingsMenu
{
    
    /// <summary>
    ///A container for storing the graphic settings values
    /// </summary>
    [System.Serializable]
    [XmlRoot("Settings")]//Data will appear under the "Settings" element in the xml file
    public class GraphicSettingDataContainer
    {
        public int screenWidth = 0;
        public int screenHeight = 0;

        //FullScreenMode is a enum : https://docs.unity3d.com/ScriptReference/FullScreenMode.html
        public int screenMode = -1;

        public int qualityLevel = -1;

        public int mainVolume = 25;
        public int musicVolume = 25;
        public int sfxVolume = 50;
        public int dialogVolume = 50;

        public int horizontalSensitivity = 0;
        public int verticalSensitivity = 0;
    }
}

