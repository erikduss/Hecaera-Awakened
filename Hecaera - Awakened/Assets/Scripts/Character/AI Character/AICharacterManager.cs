using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Erikduss
{
    public class AICharacterManager : CharacterManager
    {
        [Header("Character Name")]
        public string characterName = "";

        public AICharacterNetworkManager aICharacterNetworkManager;
        public AICharacterCombatManager aICharacterCombatManager;
        public AICharacterLocomotionManager aICharacterLocomotionManager;

        [Header("NavMesh Agent")]
        public NavMeshAgent navMeshAgent;

        [Header("Current State")]
        [SerializeField] protected AIState currentState;

        [Header("States")]
        public IdleState idle;
        public PursueTargetState pursueTarget;
        public CombatStanceState combbatStance;
        public AttackState attack;

        protected override void Awake()
        {
            base.Awake();
            aICharacterCombatManager = GetComponent<AICharacterCombatManager>();
            aICharacterNetworkManager = GetComponent<AICharacterNetworkManager>();
            aICharacterLocomotionManager = GetComponent<AICharacterLocomotionManager>();

            navMeshAgent = GetComponentInChildren<NavMeshAgent>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkDespawn();

            if (IsOwner)
            {
                //use a copy so the original is not modified.
                idle = Instantiate(idle);
                pursueTarget = Instantiate(pursueTarget);
                combbatStance = Instantiate(combbatStance);
                attack = Instantiate(attack);

                currentState = idle;
            }

            aICharacterNetworkManager.currentHealth.OnValueChanged += aICharacterNetworkManager.CheckHP;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            aICharacterNetworkManager.currentHealth.OnValueChanged -= aICharacterNetworkManager.CheckHP;
        }

        protected override void Update()
        {
            base.Update();

            aICharacterCombatManager.HandleActionRecovery(this);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (IsOwner)
            {
                ProcessStateMachine();
            }
        }

        private void ProcessStateMachine()
        {
            //logic of states is done per state. If this returns null it means it should stay on the same state.
            AIState nextState = currentState?.Tick(this);

            if (nextState != null)
            {
                currentState = nextState;
            }

            //reset the position and rotation after every state machine tick. (so it doesnt double apply the rotation)
            navMeshAgent.transform.localPosition = Vector3.zero;
            navMeshAgent.transform.localRotation = Quaternion.identity;

            if (aICharacterCombatManager.currentTarget != null)
            {
                aICharacterCombatManager.targetsDirection = aICharacterCombatManager.currentTarget.transform.position - transform.position;
                aICharacterCombatManager.viewableAngle = WorldUtilityManager.Instance.GetAngleOfTarget(transform, aICharacterCombatManager.targetsDirection);
                aICharacterCombatManager.distanceFromTarget = Vector3.Distance(transform.position, aICharacterCombatManager.currentTarget.transform.position);
            }

            if (navMeshAgent.enabled)
            {
                Vector3 agentDestination = navMeshAgent.destination;
                float remainingDistance = Vector3.Distance(agentDestination, transform.position);

                if (remainingDistance > navMeshAgent.stoppingDistance)
                {
                    aICharacterNetworkManager.isMoving.Value = true;
                }
                else
                {
                    aICharacterNetworkManager.isMoving.Value = false;
                }
            }
            else
            {
                aICharacterNetworkManager.isMoving.Value = false;
            }
        }
    }
}
