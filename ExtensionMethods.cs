using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static float ToPercentage(this float value, float maxValue)
        {
            return value / maxValue * 100.0f;
        }
    }
}
