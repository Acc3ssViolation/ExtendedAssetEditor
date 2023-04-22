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
        private static GameObject _gameObject;
        private static GameObject _uiObject;
        private static HarmonyDetours _harmonyDetours;

        private readonly List<IDetour> _detours = new List<IDetour>();

        public static GameObject GameObject
        {
            get
            {
                return _gameObject;
            }
        }

        public ModLoadingExtension()
        {
            // TODO: Port these to Harmony as well
            _detours.Add(new RenderingDetours());
            _detours.Add(new PrefabInfoDetour());
        }

        public void OnEnabled()
        {
            CitiesHarmony.API.HarmonyHelper.EnsureHarmonyInstalled();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if(Mod.IsValidLoadMode(mode))
            {
                // Since Harmony does not support proper unpatching we must make sure to only deploy it once
                // (Unpatch actually removes ALL patches on a method, not just the ones we applied)
                if (CitiesHarmony.API.HarmonyHelper.IsHarmonyInstalled && _harmonyDetours == null)
                {
                    _harmonyDetours = new HarmonyDetours();
                    _harmonyDetours.Deploy();
                }

                foreach(var detour in _detours)
                {
                    detour.Deploy();
                }

                _gameObject = new GameObject(Mod.ModName);
                _gameObject.AddComponent<PrefabWatcher>();
                _gameObject.AddComponent<SnapshotBehaviour>();

                // UI
                UIView view = UIView.GetAView();
                _uiObject = new GameObject(Mod.ModName + " UI");
                _uiObject.transform.SetParent(view.transform);
                var p = _uiObject.AddComponent<UIPanel>();
                p.relativePosition = Vector2.zero;
                p.isVisible = true;
                p.size = Vector3.zero;
                
                var o = new GameObject();
                o.transform.SetParent(_uiObject.transform);
                o.AddComponent<UIMainPanel>();

                o = new GameObject();
                o.transform.SetParent(_uiObject.transform);
                o.AddComponent<UIPropPanel>();

                // Up the trailer count limit that the default properties panel uses to 100
                var maxTrailersField = typeof(DecorationPropertiesPanel).GetField("m_MaxTrailers", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                maxTrailersField.SetValue(null, 100);
            }
        }

        public override void OnLevelUnloading()
        {
            if(_gameObject != null)
            {
                foreach(var detour in _detours)
                {
                    detour.Revert();
                }
                GameObject.Destroy(_gameObject);
            }
            if(_uiObject != null)
            {
                GameObject.Destroy(_uiObject);
            }
        }
    }
}
