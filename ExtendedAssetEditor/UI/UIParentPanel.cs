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
            PrefabWatcher.instance.prefabBecameVehicle += () =>
            {
                Debug.Log("Prefab became vehicle");
                isVisible = true;
            };
            PrefabWatcher.instance.prefabWasVehicle += () =>
            {
                Debug.Log("Prefab was vehicle");
                isVisible = false;
            };
            PrefabWatcher.instance.trailersChanged += (string[] names) =>
            {
                Debug.Log("Trailers changed");
            };
            PrefabWatcher.instance.prefabChanged += () =>
            {
                Debug.Log("Prefab changed");
            };
        }
    }
}
