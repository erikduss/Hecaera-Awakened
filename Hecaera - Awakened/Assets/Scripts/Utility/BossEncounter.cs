using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    [Serializable]
    public class BossEncounter
    {
        public int encounterBossID;
        public List<Transform> playerSpawnLocations = new List<Transform>();
    }
}
