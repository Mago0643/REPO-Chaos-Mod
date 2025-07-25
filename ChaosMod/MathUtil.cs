using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ChaosMod
{
    internal class MathUtil
    {
        public static float remapToRange(float value, float start1, float stop1, float start2, float stop2)
	    {
		    return start2 + (value - start1) * ((stop2 - start2) / (stop1 - start1));
	    }
    }
}
