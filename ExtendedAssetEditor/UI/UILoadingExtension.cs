using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using UnityEngine;
using ColossalFramework.UI;

namespace ExtendedAssetEditor.UI
{
    public class UILoadingExtension : LoadingExtensionBase
    {
        private static GameObject m_uiObject;
        private static UIMainPanel m_trailerPanel;

        public GameObject UIObject { get { return m_uiObject; } }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if(Mod.IsValidLoadMode(mode))
            {
                UIView view = UIView.GetAView();
                m_uiObject = new GameObject(Mod.name + " UI");
                m_trailerPanel = m_uiObject.AddComponent<UIMainPanel>();
                m_uiObject.transform.SetParent(view.transform);
            }
        }

        public override void OnLevelUnloading()
        {
            if(m_uiObject != null)
            {
                GameObject.Destroy(m_uiObject);
            }
        }
    }
}
