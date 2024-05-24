using Erikduss;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erikduss
{
    public class RectangleGroundIndicator : GroundIndicator
    {
        bool rotateIndicator = false;
        int rotatingDirection = 0;

        public override void SetIndicatorSize(float size)
        {
            objectToScale.transform.localScale = new Vector3(size / 5, size, size);
        }

        protected override void Update()
        {
            base.Update();

            if(rotateIndicator)
            {
                //left
                if(rotatingDirection == 0)
                {
                    float rotationStep = -0.1f;
                    Quaternion fixedRotation = transform.rotation * Quaternion.Euler(0, rotationStep, 0);

                    transform.rotation = fixedRotation;
                }
                else //right
                {
                    float rotationStep = 0.1f;
                    Quaternion fixedRotation = transform.rotation * Quaternion.Euler(0, rotationStep, 0);

                    transform.rotation = fixedRotation;
                }
            }
        }

        public void SetRotatingIndicator(int direction)
        {
            rotateIndicator = true;
            rotatingDirection = direction;
        }

        public override void ReturnThisProjectileToPool()
        {
            base.ReturnThisProjectileToPool();

            rotateIndicator = false;
            rotatingDirection = 0;
        }
    }
}
