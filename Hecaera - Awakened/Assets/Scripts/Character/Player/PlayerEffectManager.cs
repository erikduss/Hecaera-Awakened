using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class PlayerEffectManager : CharacterEffectsManager
    {
        [Header("Dedug Delete Later")]
        [SerializeField] InstantCharacterEffect effectToTest;
        [SerializeField] bool processEffect = false;

        private void Update()
        {
            if (processEffect)
            {
                processEffect = false;
                //Using an instantiate, if not using instantiate it will change values in the inspector instead, destroying all the base values.
                InstantCharacterEffect effect = Instantiate(effectToTest);
                ProcessInstantEffect(effect);
            }
        }
    }
}
