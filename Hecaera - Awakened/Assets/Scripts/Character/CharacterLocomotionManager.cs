using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class CharacterLocomotionManager : MonoBehaviour
    {
        CharacterManager character;

        [Header("Ground Check & Jumping")]
        [SerializeField] public float gravityForce = -5.55f;
        public float currentGravityForce;
        [SerializeField] protected LayerMask groundLayer;
        [SerializeField] protected LayerMask nonStandableSurfaceLayer;
        [SerializeField] protected float nonStandableCheckRange = 0.5f;
        [SerializeField] float groundCheckSphereRadius = 1.0f;
        [SerializeField] protected Vector3 yVelocity; //The force at which our character will be pulled up or down (jumping or falling)
        [SerializeField] protected float groundedYVelocity = -20; //the force at which our character is sticking to the ground
        [SerializeField] protected float fallStartYvelocity = -5; //The force at which the fall will start (rises the longer they fall)
        protected bool fallingVelocityHasBeenSet = false;
        protected float inAirTimer = 0;

        [Header("Flags")]
        public bool isRolling = false;
        public bool canRotate = true;
        public bool canMove = true;
        public bool isGrounded = true;

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();
        }

        protected virtual void Update()
        {
            HandleGroundCheck();

            if (character.characterLocomotionManager.isGrounded)
            {
                //prevent sliding after landing.
                if (yVelocity.x > 0 || yVelocity.z > 0)
                {
                    yVelocity.x = 0;
                    yVelocity.z = 0;
                }

                //if we are not attemping to jump or move upward
                if (yVelocity.y < 0)
                {
                    inAirTimer = 0;
                    fallingVelocityHasBeenSet = false;
                    yVelocity.y = groundedYVelocity;
                    yVelocity.x = 0;
                    yVelocity.z = 0;
                    currentGravityForce = gravityForce; //reset the gravity force after a jump attack.
                }
            }
            else
            {
                //if we are NOT jumping and our falling velocity has NOT been set yet.
                if (!character.characterNetworkManager.isJumping.Value && !fallingVelocityHasBeenSet)
                {
                    fallingVelocityHasBeenSet = true;
                    yVelocity.y = fallStartYvelocity;
                }

                RaycastHit hit;
                if (Physics.SphereCast(transform.position, nonStandableCheckRange, transform.TransformDirection(Vector3.down), out hit, nonStandableSurfaceLayer))
                {
                    //only react to a collider that is not your own
                    if (hit.collider.attachedRigidbody != character.characterController.attachedRigidbody)
                    {
                        Debug.Log("STADING ON NON STANDABLE SURFACE!");
                        yVelocity += (transform.forward * 0.025f);
                    }
                }
                else
                {
                    yVelocity.x = 0;
                    yVelocity.z = 0;
                }

                inAirTimer += Time.deltaTime;

                character.animator.SetFloat("InAirTimer", inAirTimer);

                yVelocity.y += currentGravityForce * Time.deltaTime;
            }

            if (!character.characterController.enabled) return;
            //There should always be some force applied to the character!
            character.characterController.Move(yVelocity * Time.deltaTime);
        }

        protected void HandleGroundCheck()
        {
            character.characterLocomotionManager.isGrounded = Physics.CheckSphere(character.transform.position, groundCheckSphereRadius, groundLayer);
        }

        //Debug to check the ground check sphere
        protected void OnDrawGizmosSelected()
        {
            //Gizmos.DrawSphere(character.transform.position, groundCheckSphereRadius);
        }

        public void EnableCanRotate()
        {
            canRotate = true;
        }

        public void DisableCanRotate()
        {
            canRotate = false;
        }
    }
}
