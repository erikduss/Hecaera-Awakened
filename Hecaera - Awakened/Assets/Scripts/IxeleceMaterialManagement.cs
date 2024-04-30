using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class IxeleceMaterialManagement : MonoBehaviour
    {
        public static IxeleceMaterialManagement Instance;

        [Header("Body Parts To Fade")]
        public List<SkinnedMeshRenderer> mainBodyRenderers = new List<SkinnedMeshRenderer>();
        public List<SkinnedMeshRenderer> propsRenderers = new List<SkinnedMeshRenderer>();
        public List<MeshRenderer> leftOverRenderers = new List<MeshRenderer>();

        [Header("Body Parts Materials")]
        [SerializeField] Material mainBodyMaterial;
        [SerializeField] Material propsMaterial;
        [SerializeField] Material teleportMaterial;
 
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }


        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void ClearMaterialLists()
        {
            mainBodyRenderers.Clear();
            propsRenderers.Clear();
            leftOverRenderers.Clear();
        }
        public void RevertIxeleceMaterial()
        {
            //Main body materials
            foreach(SkinnedMeshRenderer ren in mainBodyRenderers)
            {
                ren.material = mainBodyMaterial;
            }

            foreach(MeshRenderer ren in leftOverRenderers)
            {
                ren.material = mainBodyMaterial;
            }

            //Props materials
            foreach(SkinnedMeshRenderer ren in propsRenderers)
            {
                ren.material = propsMaterial;
            }
        }

        public void SetIxeleceTeleportMaterial()
        {
            //Main body materials
            foreach (SkinnedMeshRenderer ren in mainBodyRenderers)
            {
                ren.material = teleportMaterial;
            }

            foreach (MeshRenderer ren in leftOverRenderers)
            {
                ren.material = teleportMaterial;
            }

            //Props materials
            foreach (SkinnedMeshRenderer ren in propsRenderers)
            {
                ren.material = teleportMaterial;
            }
        }

        public void FadeTeleportMaterials(float duration, float startAlpha, float endAlpha)
        {
            StartCoroutine(FadeTeleportMaterial(duration, startAlpha, endAlpha));
        }

        public IEnumerator FadeTeleportMaterial(float duration,float startAlpha, float endAlpha)
        {
            if (duration > 0f)
            {
                teleportMaterial.color = new Color(teleportMaterial.color.r, teleportMaterial.color.g, teleportMaterial.color.b, startAlpha);
                float timer = 0;

                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    float newAlpha = Mathf.Lerp(teleportMaterial.color.a, endAlpha, duration * Time.deltaTime);
                    teleportMaterial.color = new Color(teleportMaterial.color.r, teleportMaterial.color.g, teleportMaterial.color.b, newAlpha);

                    foreach (SkinnedMeshRenderer ren in mainBodyRenderers)
                    {
                        ren.material.color = teleportMaterial.color;
                    }

                    foreach (MeshRenderer ren in leftOverRenderers)
                    {
                        ren.material.color = teleportMaterial.color;
                    }

                    //Props materials
                    foreach (SkinnedMeshRenderer ren in propsRenderers)
                    {
                        ren.material.color = teleportMaterial.color;
                    }

                    yield return null;
                }
            }

            teleportMaterial.color = new Color(teleportMaterial.color.r, teleportMaterial.color.g, teleportMaterial.color.b, endAlpha);

            yield return null;
        }
    }
}
