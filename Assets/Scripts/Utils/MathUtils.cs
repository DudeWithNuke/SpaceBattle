using System;

namespace Utils
{
    public class MathUtils
    {
        public static bool IsGreaterThanOdd(float value)
        {
            return value > Math.Floor(value) && (int)Math.Floor(value) % 2 != 0 
                   || value > Math.Floor(value) + 1 && (int)Math.Floor(value) % 2 == 0;
        }
    }
}