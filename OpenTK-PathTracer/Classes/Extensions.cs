using System;
using System.IO;

namespace OpenTK_PathTracer
{
    public static class Extensions
    {
        public static T[] AddArray<T>(this T[] arr0, T[] arr1) where T : struct
        {
            int oldLength = arr0.Length;
            Array.Resize(ref arr0, arr0.Length + arr1.Length);
            for (int i = 0; i < arr1.Length; i++)
                arr0[i + oldLength] = arr1[i];

            return arr0;
        }

        public static string GetPathContent(this string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"{path} does not exist");

            return File.ReadAllText(path);
        }
    }
}