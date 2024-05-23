using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Erikduss
{
    public class PlayerInputManager : MonoBehaviour
    {
        public static PlayerInputManager instance;
        public PlayerManager player;
        PlayerControls playerControls;

        [Header("Movement input")]
        [SerializeField] Vector2 movement_Input;
        public float vertical_Input;
        public float horizontal_Input;
        public float moveAmount;

        [Header("Camera movement input")]
        [SerializeField] Vector2 camera_Input;
        public float cameraVertical_Input;
        public float cameraHorizontal_Input;

        [Header("Lock On Input")]
        [SerializeField] bool lockOn_Input;
        [SerializeField] bool lockOn_Left_Input;
        [SerializeField] bool lockOn_Right_Input;
        private Coroutine lockOnCoroutine;

        [Header("Player action input")]
        public bool dodge_Input = false;
        [SerializeField] bool sprint_Input = false;
        [SerializeField] bool jump_Input = false;
        public bool useItem_Input = false;

        [Header("Bumper Inputs")]
        [SerializeField] bool RB_Input = false;
        [SerializeField] bool LB_Input = false;

        [Header("Trigger Inputs")]
        [SerializeField] bool RT_Input = false;
        [SerializeField] bool Hold_RT_Input = false;

        [Header("Qued Inputs")]
        private bool input_Que_Is_Active = false;
        [SerializeField] float que_Input_Timer = 0;
        [SerializeField] float default_Que_Input_Time = 0.35f;
        [SerializeField] bool que_RB_Input = false;
        [SerializeField] bool que_RT_Input = false;
        [SerializeField] bool que_LB_Input = false;

        [Header("Menu Input")]
        [SerializeField] bool openMenuButton_Input = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            //when the scene changes, run this logic.
            SceneManager.activeSceneChanged += OnSceneChange;

            instance.enabled = false;
            if (playerControls != null)
            {
                playerControls.Disable();
            }
        }

        private void OnSceneChange(Scene oldScene, Scene newScene)
        {
            //If we are loading into our world scene, enable our player controls.
            if (newScene.buildIndex == WorldSaveGameManager.instance.GetWorldSceneIndex())
            {
                instance.enabled = true;

                PlayerUIManager.instance.playerUIHudManager.SetHUDPanelStatus(true);

                if (playerControls != null)
                {
                    playerControls.Enable();
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            //otherwise we must be in the main menu, disable player controls.
            //This is so our player cant move around if we enter things like a character creation menu etc.
            else
            {
                instance.enabled = false;

                PlayerUIManager.instance.playerUIHudManager.SetHUDPanelStatus(false);

                if (playerControls != null)
                {
                    playerControls.Disable();
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        private void OnEnable()
        {
            if (playerControls == null)
            {
                playerControls = new PlayerControls();

                playerControls.PlayerMovement.Movement.performed += i => movement_Input = i.ReadValue<Vector2>();
                playerControls.PlayerCamera.Movement.performed += i => camera_Input = i.ReadValue<Vector2>();
                playerControls.PlayerActions.Dodge.performed += i => dodge_Input = true;
                playerControls.PlayerActions.Jump.performed += i => jump_Input = true;
                playerControls.PlayerActions.UseItem.performed += i => useItem_Input = true;

                //Bumpers (top (main) bumper of controller)
                playerControls.PlayerActions.RB.performed += i => RB_Input = true;
                playerControls.PlayerActions.LB.performed += i => LB_Input = true;

                //Triggers (bottom bumper of controller)
                playerControls.PlayerActions.RT.performed += i => RT_Input = true;
                playerControls.PlayerActions.HoldRT.performed += i => Hold_RT_Input = true;
                playerControls.PlayerActions.HoldRT.canceled += i => Hold_RT_Input = false; //if trigger released.

                //Lock on
                playerControls.PlayerActions.LockOn.performed += i => lockOn_Input = true;
                playerControls.PlayerActions.SeekLeftLockOnTarget.performed += i => lockOn_Left_Input = true;
                playerControls.PlayerActions.SeekRightLockOnTarget.performed += i => lockOn_Right_Input = true;

                //Holding the input sets the bool to true. Releasing it sets it to false.
                playerControls.PlayerActions.Sprint.performed += i => sprint_Input = true;
                playerControls.PlayerActions.Sprint.canceled += i => sprint_Input = false;

                //Menu Input
                playerControls.PlayerActions.OpenButtonsMenu.performed += i => openMenuButton_Input = true;

                //Qued Inputs
                playerControls.PlayerActions.QueRB.performed += i => QueInput(ref que_RB_Input);
                playerControls.PlayerActions.QueRT.performed += i => QueInput(ref que_RT_Input);
                playerControls.PlayerActions.QueLB.performed += i => QueInput(ref que_LB_Input);
            }

            playerControls.Enable();
        }

        private void OnDestroy()
        {
            //if we destroy this object, unsubscribe from this event
            SceneManager.activeSceneChanged -= OnSceneChange;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (enabled)
            {
                if (focus)
                {
                    playerControls.Enable();
                }
                else
                {
                    playerControls.Disable();
                }
            }
        }

        private void Update()
        {
            HandleAllInputs();
        }

        private void HandleAllInputs()
        {
            HandleOpenMenuButtonInput();

            //If the menu is open, we do not want to be able to move around.
            if (PlayerUIManager.instance.playerUIPopUpManager.buttonsMenuIsOpen)
                return;
            else if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            HandleLockOnInput();
            HandleLockOnSwitchTargetInput();
            HandlePlayerMovementInput();
            HandleCameraMovementInput();
            HandleDodgeInput();
            HandleUseItemInput();
            HandleSprintingInput();
            HandleJumpInput();
            HandleRBInput();
            HandleRTInput();
            HandleHoldRTInput();
            HandleLBInput();
            HandleQuedInputs();
        }

        private void ResetInputBoolsWhileMenuIsOpen()
        {
            dodge_Input = false;
            useItem_Input = false;
            jump_Input = false;

            RB_Input = false;
            LB_Input = false;

            RT_Input = false;
            Hold_RT_Input = false;

            lockOn_Input = false;
            lockOn_Left_Input = false;
            lockOn_Right_Input = false;

            sprint_Input = false;
        }

        private void HandleOpenMenuButtonInput()
        {
            if (openMenuButton_Input)
            {
                openMenuButton_Input = false;

                ResetInputBoolsWhileMenuIsOpen();

                //This is a toggle, meaning that if its on it will deactivate, if its off it will activate.
                PlayerUIManager.instance.playerUIPopUpManager.MenuButtonsActiveToggle();
            }
        }

        private void HandleLockOnInput()
        {
            if (player.playerNetworkManager.isLockedOn.Value)
            {
                if (player.playerCombatManager.currentTarget == null)
                {
                    return;
                }

                if (player.playerCombatManager.currentTarget.characterNetworkManager.isDead.Value)
                {
                    player.playerNetworkManager.isLockedOn.Value = false;

                    //attempt to find new target.
                    //Make sure the coroutine doesnt run multiple at the time.
                    if (lockOnCoroutine != null)
                    {
                        StopCoroutine(lockOnCoroutine);
                    }

                    lockOnCoroutine = StartCoroutine(PlayerCamera.instance.WaitThenFindNewTarget());
                }
            }

            if (lockOn_Input && player.playerNetworkManager.isLockedOn.Value)
            {
                lockOn_Input = false;
                PlayerCamera.instance.ClearLockOnTargets();
                player.playerNetworkManager.isLockedOn.Value = false;
                //disable lock on
                return;
            }

            if (lockOn_Input && !player.playerNetworkManager.isLockedOn.Value)
            {
                lockOn_Input = false;
                //enable lock on

                PlayerCamera.instance.HandleLocatingLockOnTargets();

                if (PlayerCamera.instance.nearestLockOnTarget != null)
                {
                    player.playerCombatManager.SetTarget(PlayerCamera.instance.nearestLockOnTarget);
                    player.playerNetworkManager.isLockedOn.Value = true;
                }
            }
        }

        private void HandleLockOnSwitchTargetInput()
        {
            if (lockOn_Left_Input)
            {
                lockOn_Left_Input = false;

                if (player.playerNetworkManager.isLockedOn.Value)
                {
                    PlayerCamera.instance.HandleLocatingLockOnTargets();

                    if (PlayerCamera.instance.leftLockOnTarget != null)
                    {
                        player.playerCombatManager.SetTarget(PlayerCamera.instance.leftLockOnTarget);
                    }
                }
            }

            if (lockOn_Right_Input)
            {
                lockOn_Right_Input = false;

                if (player.playerNetworkManager.isLockedOn.Value)
                {
                    PlayerCamera.instance.HandleLocatingLockOnTargets();

                    if (PlayerCamera.instance.rightLockOnTarget != null)
                    {
                        player.playerCombatManager.SetTarget(PlayerCamera.instance.rightLockOnTarget);
                    }
                }
            }
        }

        private void HandlePlayerMovementInput()
        {
            vertical_Input = movement_Input.y;
            horizontal_Input = movement_Input.x;

            //RETURNS THE ABSOLUTE NUMBER, (meaning number without the negative sign, so its always positive)
            moveAmount = Mathf.Clamp01(Mathf.Abs(vertical_Input) + Mathf.Abs(horizontal_Input));

            //Clamp the value so they are always 0, 0.5 or 1
            if (moveAmount <= 0.5 && moveAmount > 0)
            {
                moveAmount = 0.5f;
            }
            else if (moveAmount > 0.5 && moveAmount <= 1)
            {
                moveAmount = 1f;
            }

            //Passing 0 on the horizontal because we only want non strafing movement
            //Strafing is only used for locking on.
            if (player == null)
                return;

            if (moveAmount != 0)
            {
                player.playerNetworkManager.isMoving.Value = true;
            }
            else
            {
                player.playerNetworkManager.isMoving.Value = false;
            }

            if (!player.playerNetworkManager.isLockedOn.Value || player.playerNetworkManager.isSprinting.Value)
            {
                player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount, player.playerNetworkManager.isSprinting.Value);
            }
            else
            {
                player.playerAnimatorManager.UpdateAnimatorMovementParameters(horizontal_Input, vertical_Input, player.playerNetworkManager.isSprinting.Value);
            }

        }

        private void HandleCameraMovementInput()
        {
            cameraVertical_Input = -camera_Input.y;
            cameraHorizontal_Input = camera_Input.x;
        }

        private void HandleDodgeInput()
        {
            if (dodge_Input)
            {
                dodge_Input = false;

                player.playerLocomotionManager.AttemptToPerformDodge();
            }
        }

        private void HandleUseItemInput()
        {
            if (useItem_Input)
            {
                useItem_Input = false;

                player.playerCombatManager.PerformUseItemAction(player.playerInventoryManager.currentQuickUseHealingItem);
            }
        }

        private void HandleSprintingInput()
        {
            if (sprint_Input)
            {
                player.playerLocomotionManager.HandleSprinting();
            }
            else
            {
                player.playerNetworkManager.isSprinting.Value = false;
            }
        }

        private void HandleJumpInput()
        {
            if (jump_Input)
            {
                jump_Input = false;

                player.playerLocomotionManager.AttemptToPerformJump();
            }
        }

        private void HandleRBInput()
        {
            if (RB_Input)
            {
                RB_Input = false;

                //if setting the isperforming action check, combos wont work.
                //if (player.isPerformingAction) return;

                player.playerNetworkManager.SetCharacterActionHand(true);

                player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentRightHandWeapon.oh_RB_Action, player.playerInventoryManager.currentRightHandWeapon);
            }
        }

        private void HandleLBInput()
        {
            if (LB_Input)
            {
                LB_Input = false;

                if (player.isPerformingAction) return;

                player.playerNetworkManager.SetCharacterActionHand(true);

                //Not using both hands, but using the main hand weapon attack for left bumper. In this case, magic.
                player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentRightHandWeapon.oh_LB_Action, player.playerInventoryManager.currentRightHandWeapon);
            }
        }

        private void HandleRTInput()
        {
            if (RT_Input)
            {
                RT_Input = false;

                //if (player.isPerformingAction) return;

                player.playerNetworkManager.SetCharacterActionHand(true);

                player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentRightHandWeapon.oh_RT_Action, player.playerInventoryManager.currentRightHandWeapon);
            }
        }

        private void HandleHoldRTInput()
        {
            //we only want to check for a holding action if we are already performing an action (charging up)
            if (player.isPerformingAction)
            {
                player.playerNetworkManager.isChargingAttack.Value = Hold_RT_Input;
            }
        }

        //Is called by all inputs, and sets the passed on ref to true and all the other ones to false.
        private void QueInput(ref bool quedInput) //passing a ref, we pass a specific bool and not the value of it.
        {
            //reset all qued inputs
            ResetAllQuedInputs();

            //check for ui window being open, return if true

            if(player.isPerformingAction || player.playerNetworkManager.isJumping.Value)
            {
                quedInput = true;
                que_Input_Timer = default_Que_Input_Time;
                input_Que_Is_Active = true;
            }
        }

        private void ProcessQuedInputs()
        {
            if (player.playerNetworkManager.isDead.Value)
                return;

            if (que_RB_Input)
                RB_Input = true;

            if (que_RT_Input)
                RT_Input = true;

            if (que_LB_Input)
                LB_Input = true;
        }

        private void ResetAllQuedInputs()
        {
            que_RB_Input = false;
            que_RT_Input = false;
            que_LB_Input = false;
            //que_LT_Input = false;
        }

        private void HandleQuedInputs()
        {
            if (input_Que_Is_Active)
            {
                //while the timer is running, keep attemting the input.
                if(que_Input_Timer > 0)
                {
                    que_Input_Timer -= Time.deltaTime;
                    ProcessQuedInputs();
                }
                else
                {
                    ResetAllQuedInputs();
                    input_Que_Is_Active = false;
                    que_Input_Timer = 0;
                }
            }
        }
    }
}
