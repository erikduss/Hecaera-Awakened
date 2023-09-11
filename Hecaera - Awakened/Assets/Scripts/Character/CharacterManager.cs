using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class CharacterManager : NetworkBehaviour
{
    [HideInInspector] public CharacterController characterController;
    [HideInInspector] public Animator animator;

    [HideInInspector] public CharacterNetworkManager characterNetworkManager;

    protected virtual void Awake()
    {
        DontDestroyOnLoad(this);

        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        characterNetworkManager = GetComponent<CharacterNetworkManager>();
    }

    protected virtual void Update()
    {
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
}
