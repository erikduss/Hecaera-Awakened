using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AICharacterManager : CharacterManager
{
    public AICharacterNetworkManager aICharacterNetworkManager;
    public AICharacterCombatManager aICharacterCombatManager;
    public AICharacterLocomotionManager aICharacterLocomotionManager;

    [Header("NavMesh Agent")]
    public NavMeshAgent navMeshAgent;

    [Header("Current State")]
    [SerializeField] AIState currentState;

    [Header("States")]
    public IdleState idle;
    public PursueTargetState pursueTarget;

    protected override void Awake()
    {
        base.Awake();
        aICharacterCombatManager = GetComponent<AICharacterCombatManager>();
        aICharacterNetworkManager = GetComponent<AICharacterNetworkManager>();
        aICharacterLocomotionManager = GetComponent<AICharacterLocomotionManager>();

        navMeshAgent = GetComponentInChildren<NavMeshAgent>();

        //use a copy so the original is not modified.
        idle = Instantiate(idle);
        pursueTarget = Instantiate(pursueTarget);

        currentState = idle;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        ProcessStateMachine();
    }

    private void ProcessStateMachine()
    {
        //logic of states is done per state. If this returns null it means it should stay on the same state.
        AIState nextState = currentState?.Tick(this);

        if(nextState != null)
        {
            currentState = nextState;
        }

        //reset the position and rotation after every state machine tick. (so it doesnt double apply the rotation)
        navMeshAgent.transform.localPosition = Vector3.zero;
        navMeshAgent.transform.localRotation = Quaternion.identity;

        if (navMeshAgent.enabled)
        {
            Vector3 agentDestination = navMeshAgent.destination;
            float remainingDistance = Vector3.Distance(agentDestination, transform.position);

            if(remainingDistance > navMeshAgent.stoppingDistance)
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
