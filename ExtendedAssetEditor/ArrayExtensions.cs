using System;

namespace ExtendedAssetEditor
{
    internal static class ArrayExtensions
    {
        public static void SetValues<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = value;
        }

        public delegate void ActionRef<T>(ref T obj);
        public static void ForEachRef<T>(this T[] array, ActionRef<T> action)
        {
            for (int i = 0; i < array.Length; i++)
                action(ref array[i]);
        }
    }
}
