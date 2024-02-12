using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AICharacterCombatManager : CharacterCombatManager
{
    [Header("Detection")]
    [SerializeField] float detectionRadius = 15;
    [SerializeField] float minimumDetectionAngle = -35;
    [SerializeField] float maximumDetectionAngle = 35;

    public void FindATargetViaLineOfSight(AICharacterManager aiCharacter)
    {
        if (currentTarget != null)
            return;

        Collider[] colliders = Physics.OverlapSphere(aiCharacter.transform.position, detectionRadius, WorldUtilityManager.Instance.GetCharacterLayers());

        for(int i = 0; i < colliders.Length; i++)
        {
            CharacterManager targetCharacter = colliders[i].transform.GetComponent<CharacterManager>();

            if (targetCharacter == null) continue;

            if(targetCharacter == aiCharacter) continue;

            if (targetCharacter.characterNetworkManager.isDead.Value) continue;

            //can I attack this character?
            if(WorldUtilityManager.Instance.CanIDamageThisTarget(aiCharacter.characterGroup, targetCharacter.characterGroup))
            {
                //is the target in our viewable angle?
                Vector3 targetDirection = targetCharacter.transform.position - aiCharacter.transform.position;
                float viewableAngle = Vector3.Angle(targetDirection, aiCharacter.transform.forward);

                if(viewableAngle > minimumDetectionAngle && viewableAngle < maximumDetectionAngle)
                {
                    //check for environment blocking view
                    if(Physics.Linecast(aiCharacter.characterCombatManager.lockOnTransform.position, targetCharacter.characterCombatManager.lockOnTransform.position, WorldUtilityManager.Instance.GetEnvironmentLayers()))
                    {
                        Debug.DrawLine(aiCharacter.characterCombatManager.lockOnTransform.position, targetCharacter.characterCombatManager.lockOnTransform.position);
                        Debug.Log("Blocked");
                    }
                    else
                    {
                        aiCharacter.characterCombatManager.SetTarget(targetCharacter);
                    }
                }
            }
        }
    }
}
