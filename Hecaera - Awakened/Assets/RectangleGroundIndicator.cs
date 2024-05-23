using Erikduss;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class RectangleGroundIndicator : GroundIndicator
    {
        public override void SetIndicatorSize(float size)
        {
            objectToScale.transform.localScale = new Vector3(size / 5, size, size);
        }
    }
}
