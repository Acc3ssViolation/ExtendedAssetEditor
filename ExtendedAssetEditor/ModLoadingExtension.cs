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

        private List<IDetour> m_detours = new List<IDetour>();

        public static GameObject GameObject
        {
            get
            {
                return m_gameObject;
            }
        }

        public ModLoadingExtension()
        {
            m_detours.Add(new RenderingDetours());
            m_detours.Add(new PrefabInfoDetour());
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if(Mod.IsValidLoadMode(mode))
            {
                foreach(var detour in m_detours)
                {
                    detour.Deploy();
                }
                DisplayOptions options = new DisplayOptions();

                m_gameObject = new GameObject(Mod.name);
                m_gameObject.AddComponent<PrefabWatcher>();
                m_gameObject.AddComponent<SnapshotBehaviour>();

                // UI
                UIView view = UIView.GetAView();
                m_uiObject = new GameObject(Mod.name + " UI");
                m_uiObject.transform.SetParent(view.transform);
                var p = m_uiObject.AddComponent<UIPanel>();
                p.relativePosition = Vector2.zero;
                p.isVisible = true;
                p.size = Vector3.zero;
                var o = new GameObject();
                o.transform.SetParent(m_uiObject.transform);
                o.AddComponent<UIMainPanel>();

                // Up the trailer count limit that the default properties panel uses to 100
                var maxTrailersField = typeof(DecorationPropertiesPanel).GetField("m_MaxTrailers", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                maxTrailersField.SetValue(null, 100);
            }
        }

        public override void OnLevelUnloading()
        {
            if(m_gameObject != null)
            {
                foreach(var detour in m_detours)
                {
                    detour.Revert();
                }
                GameObject.Destroy(m_gameObject);
            }
            if(m_uiObject != null)
            {
                GameObject.Destroy(m_uiObject);
            }
        }
    }
}
