using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ExtendedAssetEditor.AssetBundles
{
    public static class AssetBundleLoader
    {
        private static bool m_initialized;
        private static string m_modBasePath;

        public static GameObject LoadGameObject(string bundle, string assetName)
        {
            if(!m_initialized)
            {
                Initialize();
            }

            GameObject obj = null;
            try
            {
                string absUri = "file:///" + m_modBasePath + "/" + bundle;
                WWW www = new WWW(absUri);
                AssetBundle bundleObject = www.assetBundle;

                UnityEngine.Object a = bundleObject.LoadAsset(assetName);
                obj = GameObject.Instantiate(a) as GameObject;
                bundleObject.Unload(false);
            }
            catch(Exception e)
            {
                Debug.LogError("Exception trying to load bundle: " + bundle + "\r\n" + e.ToString());
            }

            if(obj != null)
            {
                Debug.Log("Loaded asset " + assetName + " from bundle " + bundle);
            }

            return obj;
        }

        private static void Initialize()
        {
            if(m_initialized)
                return;

            m_initialized = true;

            Assembly asm = Assembly.GetAssembly(typeof(AssetBundleLoader));
            var pluginInfo = PluginManager.instance.FindPluginInfo(asm);
            m_modBasePath = pluginInfo.modPath.Replace("\\", "/");
        }
    }
}
