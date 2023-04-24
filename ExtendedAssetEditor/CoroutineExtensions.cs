using System;
using System.Collections;
using UnityEngine;

namespace ExtendedAssetEditor
{
    internal static class CoroutineExtensions
    {
        public static IEnumerator Append(this IEnumerator coroutine, Action action, MonoBehaviour monoBehaviour)
        {
            yield return monoBehaviour.StartCoroutine(coroutine);
            action();
        }

        public static IEnumerator Prepend(this IEnumerator coroutine, Action action, MonoBehaviour monoBehaviour)
        {
            action();
            yield return monoBehaviour.StartCoroutine(coroutine);
        }
    }
}
