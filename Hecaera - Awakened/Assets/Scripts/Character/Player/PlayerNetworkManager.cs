using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Linq;

namespace Erikduss
{
    public class PlayerNetworkManager : CharacterNetworkManager
    {
        PlayerManager player;

        public NetworkVariable<FixedString64Bytes> characterName = new NetworkVariable<FixedString64Bytes>("Character", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("Equipment")]
        public NetworkVariable<int> currentWeaponBeingUsed = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> currentRightHandWeaponID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> currentLeftHandWeaponID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<bool> isUsingRightHand = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> isUsingLeftHand = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<int> playerMaterialID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); //Only the server assigns colors.
        public NetworkVariable<Color> playerCustomMaterialColor = new NetworkVariable<Color>(Color.magenta, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        public void SetCharacterActionHand(bool rightHandedAction)
        {
            if (rightHandedAction)
            {
                isUsingLeftHand.Value = false;
                isUsingRightHand.Value = true;
            }
            else
            {
                isUsingLeftHand.Value = true;
                isUsingRightHand.Value = false;
            }
        }

        public void SetNewMaxHealthValue(int oldVitality, int newVitality)
        {
            maxHealth.Value = player.playerStatsManager.CalculateHealthBasedOnVitalityLevel(newVitality);
            PlayerUIManager.instance.playerUIHudManager.SetMaxHealthValue(maxHealth.Value);
            currentHealth.Value = maxHealth.Value;
        }

        public void SetNewMaxStaminaValue(int oldEndurance, int newEndurance)
        {
            maxStamina.Value = player.playerStatsManager.CalculateStaminaBasedOnEnduranceLevel(newEndurance);
            PlayerUIManager.instance.playerUIHudManager.SetMaxStaminaValue(maxStamina.Value);
            currentStamina.Value = maxStamina.Value;
        }

        public void OnMaterialIDChange(int oldID, int newID)
        {
            PlayerMaterialManagement.Instance.SetMaterial(player, newID, playerCustomMaterialColor.Value, false);
        }

        public void OnMaterialColorChange(Color oldColor, Color newColor)
        {
            PlayerMaterialManagement.Instance.SetMaterial(player, playerMaterialID.Value, newColor, true);
        }

        public void OnCurrentRightHandWeaponIDChange(int oldID, int newID)
        {
            WeaponItem newWeapon = Instantiate(WorldItemDatabase.Instance.GetWeaponByID(newID));
            player.playerInventoryManager.currentRightHandWeapon = newWeapon;
            player.playerEquipmentManager.LoadRightWeapon();
        }

        public void OnCurrentLeftHandWeaponIDChange(int oldID, int newID)
        {
            WeaponItem newWeapon = Instantiate(WorldItemDatabase.Instance.GetWeaponByID(newID));
            player.playerInventoryManager.currentLeftHandWeapon = newWeapon;
            player.playerEquipmentManager.LoadLeftWeapon();
        }

        public void OnCurrentCurrentWeaponBeingUsedIDChange(int oldID, int newID)
        {
            WeaponItem newWeapon = Instantiate(WorldItemDatabase.Instance.GetWeaponByID(newID));
            player.playerCombatManager.currentWeaponBeingUsed = newWeapon;
        }

        //Item Action
        [ServerRpc]
        public void NotifyTheServerOfWeaponActionServerRpc(ulong clientID, int actionID, int weaponID)
        {
            if (IsServer)
            {
                NotifyTheServerOfWeaponActionClientRpc(clientID, actionID, weaponID);
            }
        }

        [ClientRpc]
        private void NotifyTheServerOfWeaponActionClientRpc(ulong clientID, int actionID, int weaponID)
        {
            //we do not want to play the action again for the character that called the action
            if (clientID != NetworkManager.Singleton.LocalClientId)
            {
                PerformWeaponBasedAction(actionID, weaponID);
            }
        }

        //Requireownership to false is required to allow the server to force teleport a player.
        [ServerRpc(RequireOwnership = false)]
        public void NotifyTheServerOfTeleportActionServerRpc(ulong clientID, int teleportLocationID, int encounterID, bool preventTeleport, bool setMaxTeleportValue, bool overrideTeleportLocation = false)
        {
            if (IsServer)
            {
                TeleportActionForAllClientsClientRpc(clientID, teleportLocationID, encounterID, preventTeleport, setMaxTeleportValue, overrideTeleportLocation);
            }
        }

        [ClientRpc]
        public void TeleportActionForAllClientsClientRpc(ulong clientID, int teleportLocationID, int encounterID, bool preventTeleport, bool setMaxTeleportValue, bool overrideTeleportLocation = false)
        {
            if (IsOwner)
            {
                if (!preventTeleport)
                {
                    if (!overrideTeleportLocation)
                        transform.position = WorldBossEncounterManager.Instance.bossEncounter.Where(a => a.encounterBossID == encounterID).FirstOrDefault().playerSpawnLocations[teleportLocationID].position;
                    else
                        transform.position = Vector3.zero;
                }
                if (setMaxTeleportValue) WorldBossEncounterManager.Instance.SetMaxSpawnAmount();
            }
        }

        private void PerformWeaponBasedAction(int actionID, int weaponID)
        {
            WeaponItemAction weaponAction = WorldActionManager.Instance.GetWeaponItemActionByID(actionID);

            if (weaponAction != null)
            {
                weaponAction.AttemptToPerformAction(player, WorldItemDatabase.Instance.GetWeaponByID(weaponID));
            }
            else
            {
                Debug.LogError("WEAPON ACTION ID OUT OF BOUNDS, CANNOT PERFORM ACTION " + actionID + " OF WEAPON " + weaponID);
            }
        }
    }
}
