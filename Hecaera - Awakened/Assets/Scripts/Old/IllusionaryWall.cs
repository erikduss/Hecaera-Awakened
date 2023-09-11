using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike
{
    public class IllusionaryWall : MonoBehaviour
    {
        public bool wallHasBeenHit;
        public Material illusionaryWallMaterial;
        MeshRenderer meshRenderer;
        public float alpha;
        public float fadeTimer = 2.5f;
        public BoxCollider wallCollider;

        public AudioSource audioSource;
        public AudioClip illusionaryWallSound;

        private void Awake()
        {
            illusionaryWallMaterial = Instantiate(illusionaryWallMaterial);
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = illusionaryWallMaterial;
        }

        public void Update()
        {
            if (wallHasBeenHit)
            {
                FadeIllusionaryWall();
            }
        }
        public void FadeIllusionaryWall()
        {
            alpha = meshRenderer.material.color.a;
            alpha = alpha - Time.deltaTime / fadeTimer;
            Color fadedWallColor = new Color(1, 1, 1, alpha);
            meshRenderer.material.color = fadedWallColor;

            if (wallCollider.enabled)
            {
                wallCollider.enabled = false;
                audioSource.PlayOneShot(illusionaryWallSound);
            }

            if(alpha <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
