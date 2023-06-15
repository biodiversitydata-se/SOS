using System;

namespace SOS.Lib.Extensions
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// Round doubel to significant digits
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static double RoundToSignificantDigits(this double value, int digits)
        {
            if (value == 0)
            {
                return 0;
            }
   
            var scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))) + 1);
            return scale * Math.Round(value / scale, digits);
        }
    }
}
