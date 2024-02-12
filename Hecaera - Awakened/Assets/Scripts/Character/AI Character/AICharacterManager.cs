using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICharacterManager : CharacterManager
{
    public AICharacterCombatManager aICharacterCombatManager;

    [Header("Current State")]
    [SerializeField] AIState currentState;

    protected override void Awake()
    {
        base.Awake();
        aICharacterCombatManager = GetComponent<AICharacterCombatManager>();
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
    }
}
