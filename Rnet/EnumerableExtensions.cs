namespace Rnet
{

    static class EnumerableExtensions
    {

        /// <summary>
        /// Tests one array against the other for equality.
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        public static bool ArrayEquals(this byte[] a1, byte[] a2)
        {
            if (a1 == a2)
                return true;

            if (a1 != null && a2 != null)
            {
                if (a1.Length != a2.Length)
                    return false;

                for (int i = 0; i < a1.Length; i++)
                    if (a1[i] != a2[i])
                        return false;

                return true;
            }

            return false;
        }

    }

}
