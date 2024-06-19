using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Axegen
{
    public class CoroutineMonoBehaviour : MonoBehaviour { }

    public static class CoroutineHelper
    {
        private static Dictionary<string, Coroutine> coroutines = new Dictionary<string, Coroutine>();
        public static Dictionary<string, Coroutine> Coroutines { get { return coroutines; } }
        private static CoroutineMonoBehaviour coroutineParent;
        public static Coroutine StartCoroutine(IEnumerator routine, string id)
        {
            if (coroutineParent == null)
            {
                coroutineParent = new GameObject("CoroutineParent").AddComponent<CoroutineMonoBehaviour>();
            }
            if (coroutines.ContainsKey(id))
            {
                // Stop the coroutine if it already exists 
                StopCoroutine(id);
            }
            var coroutine = coroutineParent.StartCoroutine(routine);
            coroutines.Add(id, coroutine);
            return coroutine;
        }
        public static void StopCoroutine(string id)
        {
            if (coroutines.ContainsKey(id))
            {
                var coroutine = coroutines[id];
                if (coroutine != null) coroutineParent.StopCoroutine(coroutine);
                coroutines.Remove(id);
            }
        }
        public static void StopCoroutine(Coroutine coroutine)
        {
            foreach (var pair in coroutines)
            {
                if (pair.Value == coroutine)
                {
                    coroutines.Remove(pair.Key);
                    coroutineParent.StopCoroutine(coroutine);
                    return;
                }
            }
        }
    }
}
