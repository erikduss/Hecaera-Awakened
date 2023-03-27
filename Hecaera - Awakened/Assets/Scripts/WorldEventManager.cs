using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike
{
    public class WorldEventManager : MonoBehaviour
    {
        public List<FogWall> fogWalls;

        public bool bossFightIsActive; //is currently fighting boss
        public bool bossHasBeenAwakened; //woke the boss/watched the cutscene (died during the fight for example)
        public bool bossHasBeenDefeated; //boss has been defeated

        private void Awake()
        {
        }

        public void ActivateBossFight()
        {
            bossFightIsActive = true;
            bossHasBeenAwakened = true;
            
            foreach(FogWall wall in fogWalls)
            {
                wall.ActivateFogWall();
            }
        }

        public void BossHasBeenDefeated()
        {
            bossHasBeenDefeated = true;
            bossFightIsActive = false;

            foreach (FogWall wall in fogWalls)
            {
                wall.DeactivateFogWall();
            }
        }
    }
}
