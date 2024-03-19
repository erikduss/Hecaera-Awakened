using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    [CreateAssetMenu(menuName = "A.I/States/Boss Sleep")]
    public class BossSleepState : AIState
    {
        public bool setPosition = false;
        [SerializeField] Vector3 sleepingPosition;

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if(setPosition)
                aiCharacter.transform.position = sleepingPosition;

            return base.Tick(aiCharacter);        
        }
    }
}
