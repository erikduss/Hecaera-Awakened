using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class CharacterManager : NetworkBehaviour
{
    [HideInInspector] public CharacterController characterController;
    [HideInInspector] public Animator animator;

    [HideInInspector] public CharacterNetworkManager characterNetworkManager;
    [HideInInspector] public CharacterEffectsManager characterEffectsManager;
    [HideInInspector] public CharacterAnimatorManager characterAnimatorManager;

    [Header("Flags")]
    public bool isPerformingAction = false;
    public bool isJumping = false;
    public bool isGrounded = true;
    public bool applyRootMotion = false;
    public bool canRotate = true;
    public bool canMove = true;

    protected virtual void Awake()
    {
        DontDestroyOnLoad(this);

        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        characterNetworkManager = GetComponent<CharacterNetworkManager>();
        characterEffectsManager = GetComponent<CharacterEffectsManager>();
        characterAnimatorManager = GetComponent<CharacterAnimatorManager>();
    }

    protected virtual void Update()
    {
        animator.SetBool("IsGrounded", isGrounded);

        //if this character is being controller from our side, then assign its network position to the position of our transform.
        if (IsOwner)
        {
            characterNetworkManager.networkPosition.Value = transform.position;
            characterNetworkManager.networkRotation.Value = transform.rotation;
        }
        //if the character is being controlled from elsewhere, then assign its position here locally.
        else
        {
            //Position
            transform.position = Vector3.SmoothDamp
                (transform.position, 
                characterNetworkManager.networkPosition.Value, 
                ref characterNetworkManager.networkPositionVelocity, 
                characterNetworkManager.networkPositionSmoothTime);

            //Rotation
            transform.rotation = Quaternion.Slerp
                (transform.rotation, 
                characterNetworkManager.networkRotation.Value, 
                characterNetworkManager.networkPositionSmoothTime);
        }
    }

    protected virtual void LateUpdate()
    {

    }

    public virtual IEnumerator ProcessDeathEvent(bool manuallySelectDeathAnimation = false)
    {
        if (IsOwner)
        {
            characterNetworkManager.currentHealth.Value = 0;
            characterNetworkManager.isDead.Value = true;

            if (!manuallySelectDeathAnimation)
            {
                characterAnimatorManager.PlayTargetActionAnimation("Dead_01", true);
            }
        }

        yield return new WaitForSeconds(5);
    }

    public virtual void ReviveCharacter()
    {

    }
}
