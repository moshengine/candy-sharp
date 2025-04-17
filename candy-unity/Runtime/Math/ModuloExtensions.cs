namespace Candy.Unity
{
    public static class ModuloExtensions
    {
        /// <summary>
        /// Returns the remainder of the division of x by m.
        /// </summary>
        /// <param name="x">The dividend.</param>
        /// <param name="m">The divisor.</param>
        /// <returns>The remainder of the division of x by m.</returns>
        /// <remarks>
        /// This is a modulo function which always returns a positive value.
        /// </remarks>
        public static int Mod(this int x, int m)
        {
            return (x % m + m) % m;
        }

        /// <summary>
        /// Returns the remainder of the division of x by m.
        /// </summary>
        /// <param name="x">The dividend.</param>
        /// <param name="m">The divisor.</param>
        /// <returns>The remainder of the division of x by m.</returns>
        /// <remarks>
        /// This is a modulo function which always returns a positive value.
        /// </remarks>
        public static float Mod(this float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}
