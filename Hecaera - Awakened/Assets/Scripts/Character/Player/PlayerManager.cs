using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace Erikduss
{
    public class PlayerManager : CharacterManager
    {
        public bool UseTHAnimations = false;

        [Header("DEBUG MENU")]
        [SerializeField] bool respawnCharacter = false;
        [SerializeField] bool switchRightWeapon = false;

        [HideInInspector] public PlayerAnimatorManager playerAnimatorManager;
        [HideInInspector] public PlayerLocomotionManager playerLocomotionManager;
        [HideInInspector] public PlayerNetworkManager playerNetworkManager;
        [HideInInspector] public PlayerStatsManager playerStatsManager;
        [HideInInspector] public PlayerInventoryManager playerInventoryManager;
        [HideInInspector] public PlayerEquipmentManager playerEquipmentManager;
        [HideInInspector] public PlayerCombatManager playerCombatManager;
        [HideInInspector] public PlayerSoundFXManager playerSoundFXManager;

        protected override void Awake()
        {
            base.Awake();

            playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
            playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
            playerNetworkManager = GetComponent<PlayerNetworkManager>();
            playerStatsManager = GetComponent<PlayerStatsManager>();
            playerInventoryManager = GetComponent<PlayerInventoryManager>();
            playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
            playerCombatManager = GetComponent<PlayerCombatManager>();
            playerSoundFXManager = GetComponent<PlayerSoundFXManager>();
        }

        protected override void Update()
        {
            base.Update();

            //if we do not own this gameobject, we do not control or edit it.
            if (!IsOwner)
            {
                return;
            }

            //Handle all of our character's movement.
            playerLocomotionManager.HandleAllMovement();

            playerStatsManager.RegenerateStamina();


            DebugMenu();
        }

        protected override void LateUpdate()
        {
            if (!IsOwner)
            {
                return;
            }

            base.LateUpdate();

            PlayerCamera.instance.HandleAllCameraActions();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;

            //IF THIS IS THE PLAYER OBJECT OWNED BY THIS CLIENT
            if (IsOwner)
            {
                PlayerCamera.instance.player = this;
                PlayerInputManager.instance.player = this;
                WorldSaveGameManager.instance.player = this;

                playerNetworkManager.vitality.OnValueChanged += playerNetworkManager.SetNewMaxHealthValue;
                playerNetworkManager.endurance.OnValueChanged += playerNetworkManager.SetNewMaxStaminaValue;

                playerNetworkManager.currentHealth.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewHealthValue;
                playerNetworkManager.currentStamina.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewStaminaValue;
                playerNetworkManager.currentStamina.OnValueChanged += playerStatsManager.ResetStaminaRegenTimer;
            }

            //Stats
            playerNetworkManager.currentHealth.OnValueChanged += playerNetworkManager.CheckHP;

            //lock on
            playerNetworkManager.isLockedOn.OnValueChanged += playerNetworkManager.OnIsLockedOnChanged;
            playerNetworkManager.currentTargetNetworkObjectID.OnValueChanged += playerNetworkManager.OnLockOnTargetIDChange;

            //Equipment
            playerNetworkManager.currentRightHandWeaponID.OnValueChanged += playerNetworkManager.OnCurrentRightHandWeaponIDChange;
            playerNetworkManager.currentLeftHandWeaponID.OnValueChanged += playerNetworkManager.OnCurrentLeftHandWeaponIDChange;
            playerNetworkManager.currentWeaponBeingUsed.OnValueChanged += playerNetworkManager.OnCurrentCurrentWeaponBeingUsedIDChange;

            //Flags
            playerNetworkManager.isChargingAttack.OnValueChanged += playerNetworkManager.OnIsChargingAttackChanged;

            //Player colors
            playerNetworkManager.playerMaterialID.OnValueChanged += playerNetworkManager.OnMaterialIDChange;
            playerNetworkManager.playerCustomMaterialColor.OnValueChanged += playerNetworkManager.OnMaterialColorChange;

            if (IsServer)
            {
                //Set the color of the additional player's model.
                int nextIndex = PlayerMaterialManagement.Instance.lastSelectedMaterialIndex++;
                Color backupColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                bool useCustomColor = false;

                if (nextIndex > PlayerMaterialManagement.Instance.additionalPlayerMaterials.Count)
                {
                    useCustomColor = true;
                    playerNetworkManager.playerMaterialID.Value = -1;
                    playerNetworkManager.playerCustomMaterialColor.Value = backupColor;
                }
                else
                    playerNetworkManager.playerMaterialID.Value = nextIndex;

                PlayerMaterialManagement.Instance.SetMaterial(this, nextIndex, backupColor, useCustomColor);
            }
            else
            {
                playerNetworkManager.OnMaterialIDChange(0, playerNetworkManager.playerMaterialID.Value);
            }

            //Upon connecting, if we are the owner of this character but not the server. Reload our character data
            //Server doesnt need to reload due to the character not being deleted.
            if (IsOwner && !IsServer)
            {
                WorldSaveGameManager.instance.currentCharacterData.currentHealth = playerStatsManager.CalculateHealthBasedOnVitalityLevel(WorldSaveGameManager.instance.currentCharacterData.vitality);
                LoadGameDataFromCurrentCharacterData(ref WorldSaveGameManager.instance.currentCharacterData);

                //Possibly fixes issue of client starting without stats.
                WorldSaveGameManager.instance.LoadGame();
            }
        }

        public override void OnNetworkDespawn()
        {
            //Unsubscribe from ALL events to prevent memmory leaks!
            base.OnNetworkDespawn();

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;

            //IF THIS IS THE PLAYER OBJECT OWNED BY THIS CLIENT
            if (IsOwner)
            {
                playerNetworkManager.vitality.OnValueChanged -= playerNetworkManager.SetNewMaxHealthValue;
                playerNetworkManager.endurance.OnValueChanged -= playerNetworkManager.SetNewMaxStaminaValue;

                playerNetworkManager.currentHealth.OnValueChanged -= PlayerUIManager.instance.playerUIHudManager.SetNewHealthValue;
                playerNetworkManager.currentStamina.OnValueChanged -= PlayerUIManager.instance.playerUIHudManager.SetNewStaminaValue;
                playerNetworkManager.currentStamina.OnValueChanged -= playerStatsManager.ResetStaminaRegenTimer;
            }

            //Stats
            playerNetworkManager.currentHealth.OnValueChanged -= playerNetworkManager.CheckHP;

            //lock on
            playerNetworkManager.isLockedOn.OnValueChanged -= playerNetworkManager.OnIsLockedOnChanged;
            playerNetworkManager.currentTargetNetworkObjectID.OnValueChanged -= playerNetworkManager.OnLockOnTargetIDChange;

            //Equipment
            playerNetworkManager.currentRightHandWeaponID.OnValueChanged -= playerNetworkManager.OnCurrentRightHandWeaponIDChange;
            playerNetworkManager.currentLeftHandWeaponID.OnValueChanged -= playerNetworkManager.OnCurrentLeftHandWeaponIDChange;
            playerNetworkManager.currentWeaponBeingUsed.OnValueChanged -= playerNetworkManager.OnCurrentCurrentWeaponBeingUsedIDChange;

            //Flags
            playerNetworkManager.isChargingAttack.OnValueChanged -= playerNetworkManager.OnIsChargingAttackChanged;

            //Player colors
            playerNetworkManager.playerMaterialID.OnValueChanged -= playerNetworkManager.OnMaterialIDChange;
            playerNetworkManager.playerCustomMaterialColor.OnValueChanged -= playerNetworkManager.OnMaterialColorChange;
        }

        private void OnClientConnectedCallback(ulong clientID)
        {
            WorldGameSessionManager.Instance.AddPlayerToActivePlayersList(this);

            //if we are the host we already have all the players and dont need to load them.
            if (!IsServer && IsOwner)
            {
                foreach (PlayerManager player in WorldGameSessionManager.Instance.players)
                {
                    if (player != this)
                    {
                        player.LoadOtherPlayerCharacterWhenJoiningServer();
                    }
                    else //We still need to set our own color
                    {
                        bool useCustomColor = false;
                        //if (playerNetworkManager.playerMaterialID.Value == -1) useCustomColor = true;

                        PlayerMaterialManagement.Instance.SetMaterial(this, playerNetworkManager.playerMaterialID.Value, playerNetworkManager.playerCustomMaterialColor.Value, useCustomColor);
                    }
                }
            }
        }

        public override IEnumerator ProcessDeathEvent(bool manuallySelectDeathAnimation = false)
        {
            if (IsOwner)
            {
                PlayerUIManager.instance.playerUIPopUpManager.SendYouDiedPopUp();
            }
            else
            {
                if (WorldGameSessionManager.Instance.PVPEnabled)
                {
                    //we killed the other player, to keep it fair. Heal other player too.
                    WorldGameSessionManager.Instance.HealLocalPlayerToFull();
                }
            }

            yield return base.ProcessDeathEvent(manuallySelectDeathAnimation);

            if (WorldBossEncounterManager.Instance.DoWeStillHaveRespawnsAvailable())
            {
                //only the server can execute this. But still needed everywhere to keep track of respawn charges.
                WorldBossEncounterManager.Instance.RespawnPlayerAtEnouncterSpawnPoint(this, 0);

                ReviveCharacter();
            }
            else 
            {
                WorldBossEncounterManager.Instance.CheckIfEncounterNeedsToBeRestarted();

                //we only want to switch our camera if we are the owner.
                if(IsOwner)
                    WorldBossEncounterManager.Instance.FollowOtherPlayerWithCamera();

                //we are dead, we dont have any more respawns. Enrage the boss.
                WorldBossEncounterManager.Instance.UpdateDeadPlayersList();
            }
        }

        public override void ReviveCharacter()
        {
            base.ReviveCharacter(); //needs this to disable and enable the collider on both client and server side.

            if (IsOwner)
            {
                playerNetworkManager.currentHealth.Value = playerNetworkManager.maxHealth.Value;
                playerNetworkManager.currentStamina.Value = playerNetworkManager.maxStamina.Value;
                playerNetworkManager.isDead.Value = false;
                playerNetworkManager.isLockedOn.Value = false;

                playerAnimatorManager.PlayTargetActionAnimation("Empty", false);
            }
        }

        public void SaveGameDataToCurrentCharacterData(ref CharacterSaveData currentCharacterData)
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            if (currentIndex != 0 && currentIndex != 5)
                currentCharacterData.sceneIndex = SceneManager.GetActiveScene().buildIndex;

            currentCharacterData.characterName = playerNetworkManager.characterName.Value.ToString();
            currentCharacterData.yPosition = transform.position.y;
            currentCharacterData.zPosition = transform.position.z;
            currentCharacterData.xPosition = transform.position.x;

            currentCharacterData.currentHealth = playerNetworkManager.currentHealth.Value;
            currentCharacterData.currentStamina = playerNetworkManager.currentStamina.Value;

            currentCharacterData.vitality = playerNetworkManager.vitality.Value;
            currentCharacterData.endurance = playerNetworkManager.endurance.Value;

        }

        public void LoadGameDataFromCurrentCharacterData(ref CharacterSaveData currentCharacterData)
        {
            playerNetworkManager.characterName.Value = currentCharacterData.characterName;
            Vector3 myPosition = new Vector3(currentCharacterData.xPosition, currentCharacterData.yPosition, currentCharacterData.zPosition);
            transform.position = myPosition;

            playerNetworkManager.vitality.Value = currentCharacterData.vitality;
            playerNetworkManager.endurance.Value = currentCharacterData.endurance;

            playerNetworkManager.maxHealth.Value = playerStatsManager.CalculateHealthBasedOnVitalityLevel(playerNetworkManager.vitality.Value);
            playerNetworkManager.maxStamina.Value = playerStatsManager.CalculateStaminaBasedOnEnduranceLevel(playerNetworkManager.endurance.Value);
            playerNetworkManager.currentHealth.Value = currentCharacterData.currentHealth;
            playerNetworkManager.currentStamina.Value = currentCharacterData.currentStamina;
            PlayerUIManager.instance.playerUIHudManager.SetMaxStaminaValue(playerNetworkManager.maxStamina.Value);
        }

        public void LoadOtherPlayerCharacterWhenJoiningServer()
        {
            //sync weapons
            playerNetworkManager.OnCurrentRightHandWeaponIDChange(0, playerNetworkManager.currentRightHandWeaponID.Value);
            playerNetworkManager.OnCurrentLeftHandWeaponIDChange(0, playerNetworkManager.currentLeftHandWeaponID.Value);

            //SET THE CORRECT COLORS OF PLAYERS
            if (!IsServer && !IsHost) //The server has default colors.
            {
                bool useCustomColor = false;
                //if (playerNetworkManager.playerMaterialID.Value == -1) useCustomColor = true;

                playerNetworkManager.OnMaterialIDChange(0, playerNetworkManager.playerMaterialID.Value);

                //PlayerMaterialManagement.Instance.SetMaterial(this, playerNetworkManager.playerMaterialID.Value, playerNetworkManager.playerCustomMaterialColor.Value, useCustomColor);
            }

            //Set lock on target if locked on.
            if (playerNetworkManager.isLockedOn.Value)
            {
                playerNetworkManager.OnLockOnTargetIDChange(0, playerNetworkManager.currentTargetNetworkObjectID.Value);
            }
        }

        public bool AmITheHost()
        {
            return IsServer;
        }

        private void DebugMenu()
        {
            if (respawnCharacter)
            {
                respawnCharacter = false;
                ReviveCharacter();
            }

            if (switchRightWeapon)
            {
                switchRightWeapon = false;
                playerEquipmentManager.SwitchRightWeapon();
            }
        }
    }
}
