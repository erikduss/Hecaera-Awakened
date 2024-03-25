using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class SunBeamLogic : MonoBehaviour
    {
        public GameObject objectToFollow;
        public AICharacterManager casterCharacter;

        private float maxLifeTime = 2.75f;
        private float currentLifeTime = 0;

        [SerializeField] private GameObject sunbeamChildGameobject;

        private void Update()
        {
            if (sunbeamChildGameobject.activeSelf)
            {
                currentLifeTime += Time.deltaTime;

                if (currentLifeTime > maxLifeTime)
                {
                    DeativateSunbeam();
                }
            }

            transform.position = new Vector3(objectToFollow.transform.position.x, objectToFollow.transform.position.y - 1f, objectToFollow.transform.position.z);
            transform.rotation = casterCharacter.transform.rotation;
        }

        public void ActivateSunbeam()
        {
            casterCharacter.aICharacterCombatManager.attackRotationSpeed = casterCharacter.aICharacterCombatManager.attackRotationSpeed / 2f;
            sunbeamChildGameobject.SetActive(true);
        }

        public void DeativateSunbeam()
        {
            casterCharacter.aICharacterCombatManager.attackRotationSpeed = casterCharacter.aICharacterCombatManager.attackRotationSpeed * 2f;
            sunbeamChildGameobject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
