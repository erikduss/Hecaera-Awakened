using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class AIIxeleceSoundFXManager : CharacterSoundFXManager
    {
        public void PlayIxeleceScream()
        {
            PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(WorldSoundFXManager.instance.ixeleceScreams));
        }

        public void PlayIxeleceAttackVoice()
        {
            PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(WorldSoundFXManager.instance.ixeleceScreams));
        }
    }
}
