using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class IxeleceSkinRenderLink : MonoBehaviour
    {
        [SerializeField] bool hasMainBodyMaterial = false;

        private void Awake()
        {
            SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();

            AIIxeleceCharacterManager characterToLinkTo = transform.GetComponentInParent<AIIxeleceCharacterManager>();

            if (renderer == null ) 
            {
                MeshRenderer ren = GetComponent<MeshRenderer>();

                if ( ren != null)
                {
                    characterToLinkTo.ixeleceMaterialManagement.leftOverRenderers.Add(ren);
                    return;
                }

                Debug.LogError("MESH RENDERER NOT FOUND");
                return;
            }

            if (hasMainBodyMaterial) characterToLinkTo.ixeleceMaterialManagement.mainBodyRenderers.Add(renderer);
            else characterToLinkTo.ixeleceMaterialManagement.propsRenderers.Add(renderer);
        }
    }
}
