using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class CircleGroundIndicator : GroundIndicator
    {
        [SerializeField] private List<MeshRenderer> indicatorRenderers = new List<MeshRenderer>();

        private void Start()
        {
            foreach(var renderer in indicatorRenderers)
            {
                renderer.material = Instantiate(renderer.material); //make a copy to make sure every indicator only modifies temporary materials
            }

            StartCoroutine(FadeInIndicators(.5f, 0, 1));
        }

        private IEnumerator FadeInIndicators(float duration, float startAlpha, float endAlpha)
        {
            if (duration > 0f)
            {
                foreach (var renderer in indicatorRenderers)
                {
                    renderer.material.SetFloat("_CutoutAlphaMul", startAlpha);
                    //renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, startAlpha);
                }
                
                float timer = 0;

                while (timer < duration)
                {
                    timer += Time.deltaTime;

                    foreach (var renderer in indicatorRenderers)
                    {
                        float newAlpha = Mathf.Lerp(renderer.material.GetFloat("_CutoutAlphaMul"), endAlpha, duration * Time.deltaTime);
                        renderer.material.SetFloat("_CutoutAlphaMul", newAlpha);
                        //renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, newAlpha);
                    }

                    yield return null;
                }
            }

            foreach (var renderer in indicatorRenderers)
            {
                renderer.material.SetFloat("_CutoutAlphaMul", endAlpha);
                //renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, endAlpha);
            }

            yield return null;
        }
    }
}
