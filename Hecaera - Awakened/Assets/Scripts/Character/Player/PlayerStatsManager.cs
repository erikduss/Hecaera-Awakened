using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerStatsManager : CharacterStatsManager
{
    PlayerManager player;

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
        //Enable if stamina is low (can only do 1 action, this should be a set number that can be changed if needed.)
        //PlayerUIManager.instance.playerUIHudManager.
        //Disable if player can do 2 actions

        //WE DO NOT WANT TO REGENERATE STAMINA WHILE USING IT.

        base.RegenerateStamina();
    }
}
