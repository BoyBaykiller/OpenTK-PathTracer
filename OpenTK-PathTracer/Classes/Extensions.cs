using System.Collections.Generic;

namespace OpenTK_PathTracer
{
    public static class Extensions
    {
        public static T[] AddArray<T>(this T[] arr0, T[] arr1) where T : struct
        {
            List<T> byteList = new List<T>(arr0);
            byteList.AddRange(arr1);
            return byteList.ToArray();
        }
    }
}