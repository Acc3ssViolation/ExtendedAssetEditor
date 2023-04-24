using System;
using System.Collections;
using UnityEngine;
using ICities;

namespace ExtendedAssetEditor
{
    public class CoroutineHelper : MonoBehaviour
    {
        private const float WaitTime = 0.05f;

        public static GameObject GameObject { get; private set; }

        private Action _action;

        /// <summary>
        /// Runs the method with a short delay.
        /// </summary>
        public void Run(float waitTime = WaitTime)
        {
            StartCoroutine(Coroutine(waitTime));
        }

        private IEnumerator Coroutine(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            try
            {
                _action.Invoke();
            }
            catch(Exception e)
            {
                Util.LogWarning("Exception running CoroutineHelper:\n" + e.Message + "\n" + e.StackTrace);
            }
        }
        
        /// <summary>
        /// Creates a new CoroutineHelper for the given method.
        /// </summary>
        public static CoroutineHelper Create(Action action)
        {
            var helper = GameObject.AddComponent<CoroutineHelper>();
            helper._action = action;
            return helper;
        }

        public static void CreateGameObject()
        {
            if(GameObject == null)
            {
                GameObject = new GameObject("Coroutine Helper");
                DontDestroyOnLoad(GameObject);
            }
        }
    }

    public class CoroutineHelperManager : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            CoroutineHelper.CreateGameObject();
            Util.Log("Coroutine Helper Object created!");
        }

        public override void OnReleased()
        {
            if(CoroutineHelper.GameObject != null)
            {
                GameObject.Destroy(CoroutineHelper.GameObject);
                Util.Log("Coroutine Helper Object destroyed!");
            }
        }
    }
}
