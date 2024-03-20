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

            if (renderer == null ) 
            {
                MeshRenderer ren = GetComponent<MeshRenderer>();

                if ( ren != null)
                {
                    IxeleceMaterialManagement.Instance.leftOverRenderers.Add(ren);
                    return;
                }

                Debug.LogError("MESH RENDERER NOT FOUND");
                return;
            }

            if (hasMainBodyMaterial) IxeleceMaterialManagement.Instance.mainBodyRenderers.Add(renderer);
            else IxeleceMaterialManagement.Instance.propsRenderers.Add(renderer);
        }
    }
}
