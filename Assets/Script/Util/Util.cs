using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public static class MathfExt
    {
        public static float Remap (float value, float inMin, float inMax, float outMin, float outMax)
        {
            return (value - inMin) / (inMax - inMin) * (outMax - outMin) + outMin;
        }
    }
}
