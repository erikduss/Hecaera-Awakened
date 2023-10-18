using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] bool lock_On_Input;

    [Header("Player action input")]
    [SerializeField] bool dodge_Input = false;
    [SerializeField] bool sprint_Input = false;
    [SerializeField] bool jump_Input = false;
    [SerializeField] bool RB_Input = false;

    private void Awake()
    {
        if(instance == null)
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
        if(playerControls != null)
        {
            playerControls.Disable();
        }
    }

    private void OnSceneChange(Scene oldScene, Scene newScene)
    {
        //If we are loading into our world scene, enable our player controls.
        if(newScene.buildIndex == WorldSaveGameManager.instance.GetWorldSceneIndex())
        {
            instance.enabled = true;

            if (playerControls != null)
            {
                playerControls.Enable();
            }
        }
        //otherwise we must be in the main menu, disable player controls.
        //This is so our player cant move around if we enter things like a character creation menu etc.
        else
        {
            instance.enabled = false;

            if (playerControls != null)
            {
                playerControls.Disable();
            }
        }
    }

    private void OnEnable()
    {
        if(playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.PlayerMovement.Movement.performed += i => movement_Input = i.ReadValue<Vector2>();
            playerControls.PlayerCamera.Movement.performed += i => camera_Input = i.ReadValue<Vector2>();
            playerControls.PlayerActions.Dodge.performed += i => dodge_Input = true;
            playerControls.PlayerActions.Jump.performed += i => jump_Input = true;
            playerControls.PlayerActions.RB.performed += i => RB_Input = true;

            playerControls.PlayerActions.LockOn.performed += i => lock_On_Input = true;

            //Holding the input sets the bool to true. Releasing it sets it to false.
            playerControls.PlayerActions.Sprint.performed += i => sprint_Input = true;
            playerControls.PlayerActions.Sprint.canceled += i => sprint_Input = false;
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
        HandleLockOnInput();
        HandlePlayerMovementInput();
        HandleCameraMovementInput();
        HandleDodgeInput();
        HandleSprintingInput();
        HandleJumpInput();
        HandleRBInput();
    }

    private void HandleLockOnInput()
    {
        if (player.playerNetworkManager.isLockedOn.Value)
        {
            if(player.playerCombatManager.currentTarget == null)
            {
                return;
            }

            if (player.playerCombatManager.currentTarget.characterNetworkManager.isDead.Value)
            {
                player.playerNetworkManager.isLockedOn.Value = false;
            }

            //attempt to find new target.
        }

        if (lock_On_Input && player.playerNetworkManager.isLockedOn.Value)
        {
            lock_On_Input = false;
            //disable lock on
            return;
        }

        if (lock_On_Input && !player.playerNetworkManager.isLockedOn.Value)
        {
            lock_On_Input = false;
            //enable lock on

            PlayerCamera.instance.HandleLocatingLockOnTargets();
        }
    }

    private void HandlePlayerMovementInput()
    {
        vertical_Input = movement_Input.y;
        horizontal_Input = movement_Input.x;

        //RETURNS THE ABSOLUTE NUMBER, (meaning number without the negative sign, so its always positive)
        moveAmount = Mathf.Clamp01(Mathf.Abs(vertical_Input) + Mathf.Abs(horizontal_Input));

        //Clamp the value so they are always 0, 0.5 or 1
        if(moveAmount <= 0.5 && moveAmount > 0)
        {
            moveAmount = 0.5f;
        }
        else if(moveAmount > 0.5 && moveAmount <= 1)
        {
            moveAmount = 1f;
        }

        //Passing 0 on the horizontal because we only want non strafing movement
        //Strafing is only used for locking on.
        if (player == null)
            return;
        player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount, player.playerNetworkManager.isSprinting.Value);
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

            player.playerNetworkManager.SetCharacterActionHand(true);

            player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentRightHandWeapon.oh_RB_Action, player.playerInventoryManager.currentRightHandWeapon);
        }
    }
}
