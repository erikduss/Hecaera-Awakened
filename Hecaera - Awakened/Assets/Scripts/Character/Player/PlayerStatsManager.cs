using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Erikduss
{
    public class PlayerStatsManager : CharacterStatsManager
    {
        PlayerManager player;
        private float staminaLowWarningThreshold = 30;

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        protected override void Start()
        {
            base.Start();

            //To prevent the stats to never be calculated, calculate it here.
            CalculateHealthBasedOnVitalityLevel(player.playerNetworkManager.vitality.Value);
            CalculateStaminaBasedOnEnduranceLevel(player.playerNetworkManager.endurance.Value);
        }

        public override void RegenerateStamina()
        {
            //ONLY OWNERS VAN EDIT THEIR NETWORK VARIABLES.
            if (!player.IsOwner)
                return;

            //TODO: Show stamina low indicator in Player UI

            if (player.playerNetworkManager.currentStamina.Value <= staminaLowWarningThreshold)
            {
                //Enable if stamina is low (can only do 1 action, this should be a set number that can be changed if needed.)
                StartCoroutine(PlayerUIManager.instance.playerUIHudManager.FadeAlphaOfStaminaPanel(true));
            }
            else
            {
                if (PlayerUIManager.instance.playerUIHudManager.stamina_Low_Panel.activeInHierarchy)
                {
                    if (!PlayerUIManager.instance.playerUIHudManager.fadingAlphaOfStaminaPanel)
                    {
                        //Disable if player can do 2 actions
                        StartCoroutine(PlayerUIManager.instance.playerUIHudManager.FadeAlphaOfStaminaPanel(false));
                    }
                }
            }

            base.RegenerateStamina();
        }
    }
}
