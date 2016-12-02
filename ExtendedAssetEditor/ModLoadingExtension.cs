using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ICities;
using ExtendedAssetEditor.Detour;
using ColossalFramework.UI;
using ExtendedAssetEditor.UI;

namespace ExtendedAssetEditor
{
    public class ModLoadingExtension : LoadingExtensionBase
    {
        private static GameObject m_gameObject;
        private static GameObject m_uiObject;
        private static RenderingDetours m_renderDetours;
        private static PrefabInfoDetour m_prefabInfoDetour;

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
                m_prefabInfoDetour = new PrefabInfoDetour();
                m_prefabInfoDetour.Deploy();

                DisplayOptions options = new DisplayOptions();

                m_gameObject = new GameObject(Mod.name);
                m_gameObject.AddComponent<PrefabWatcher>();

                // UI
                UIView view = UIView.GetAView();
                m_uiObject = new GameObject(Mod.name + " UI");
                m_uiObject.AddComponent<UIMainPanel>();
                m_uiObject.transform.SetParent(view.transform);

                // Up the trailer count limit that the default properties panel uses to 100
                var maxTrailersField = typeof(DecorationPropertiesPanel).GetField("m_MaxTrailers", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                maxTrailersField.SetValue(null, 100);
            }
        }

        public override void OnLevelUnloading()
        {
            if(m_gameObject != null)
            {
                m_renderDetours.Revert();
                m_prefabInfoDetour.Revert();
                GameObject.Destroy(m_gameObject);
            }
            if(m_uiObject != null)
            {
                GameObject.Destroy(m_uiObject);
            }
        }
    }
}
