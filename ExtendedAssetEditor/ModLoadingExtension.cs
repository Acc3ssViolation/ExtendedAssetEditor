using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ICities;
using ExtendedAssetEditor.Rendering;

namespace ExtendedAssetEditor
{
    public class ModLoadingExtension : LoadingExtensionBase
    {
        private static GameObject m_gameObject;
        private static RenderingDetours m_renderDetours;

        public static GameObject GameObject
        {
            get
            {
                return m_gameObject;
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if(Mod.IsValidLoadMode(mode))
            {
                m_renderDetours = new RenderingDetours();
                m_renderDetours.Deploy();
                DisplayOptions options = new DisplayOptions();
                m_gameObject = new GameObject(Mod.name);
                m_gameObject.AddComponent<PrefabWatcher>();
            }
        }

        public override void OnLevelUnloading()
        {
            if(m_gameObject != null)
            {
                m_renderDetours.Revert();
                GameObject.Destroy(m_gameObject);
            }
        }
    }
}
