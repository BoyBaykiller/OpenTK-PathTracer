using System.Collections.Generic;

namespace OpenTK_PathTracer
{
    public static class Extensions
    {
        public static T[] AddArray<T>(this T[] arr0, T[] arr1) where T : struct
        {
            List<T> tempList = new List<T>(arr0);
            tempList.AddRange(arr1);
            return tempList.ToArray();
        }
    }
}