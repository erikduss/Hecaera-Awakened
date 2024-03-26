using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class GroundIndicator : MonoBehaviour
    {
        [SerializeField] GameObject childObjectToScale;

        [SerializeField] public float indicatorSize = 1f;

        public virtual void SetIndicatorSize(float size)
        {
            childObjectToScale.transform.localScale = new Vector3(size, 5, size);
        }
    }
}
