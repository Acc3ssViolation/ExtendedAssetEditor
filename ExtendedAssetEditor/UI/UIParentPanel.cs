using ColossalFramework.UI;
using UnityEngine;

namespace ExtendedAssetEditor.UI
{
    public class UIParentPanel : UIPanel
    {
        // We have the same events as PrefabWatcher but we only fire them when we are visible
        public event PrefabWatcher.OnPrefabChanged prefabBecameVehicle;
        public event PrefabWatcher.OnPrefabChanged prefabWasVehicle;
        public event PrefabWatcher.OnPrefabChanged prefabBecameBuilding;
        public event PrefabWatcher.OnPrefabChanged prefabWasBuilding;
        public event PrefabWatcher.OnPrefabChanged prefabBecameProp;
        public event PrefabWatcher.OnPrefabChanged prefabWasProp;
        public event PrefabWatcher.OnPrefabChanged prefabChanged;
        public event PrefabWatcher.OnTrailersChanged trailersChanged;

        public override void Start()
        {
            base.Start();
            PrefabWatcher.Instance.PrefabBecameVehicle += () =>
            {
                Debug.Log("Prefab became vehicle");
                isVisible = true;
            };
            PrefabWatcher.Instance.PrefabWasVehicle += () =>
            {
                Debug.Log("Prefab was vehicle");
                isVisible = false;
            };
            PrefabWatcher.Instance.TrailersChanged += (string[] names) =>
            {
                Debug.Log("Trailers changed");
            };
            PrefabWatcher.Instance.PrefabChanged += () =>
            {
                Debug.Log("Prefab changed");
            };
        }
    }
}
