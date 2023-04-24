using ColossalFramework;
using System;
using UnityEngine;

namespace ExtendedAssetEditor
{
    public class PrefabWatcher : MonoBehaviour
    {
        public static PrefabWatcher Instance { get; private set; }

        public delegate void OnPrefabChanged();
        public delegate void OnTrailersChanged(string[] names);

        public event OnPrefabChanged PrefabBecameVehicle;
        public event OnPrefabChanged PrefabWasVehicle;
        public event OnPrefabChanged PrefabBecameBuilding;
        public event OnPrefabChanged PrefabWasBuilding;
        public event OnPrefabChanged PrefabBecameProp;
        public event OnPrefabChanged PrefabWasProp;
        public event OnPrefabChanged PrefabChanged;
        public event OnTrailersChanged TrailersChanged;

        private Type _typeOfPrefab;
        private string _prefabName;
        private string[] _trailerNames;

        public void Awake()
        {
            if(Instance != null)
            {
                Util.LogWarning("More than 1 PrefabWatcher active!");
                return;
            }
            _trailerNames = new string[0];
            Instance = this;
        }

        public void LateUpdate()
        {
            ToolController properties = Singleton<ToolManager>.instance.m_properties;
            if(properties != null && properties.m_editPrefabInfo != null)
            {
                if(_typeOfPrefab != properties.m_editPrefabInfo.GetType())
                {
                    if(_typeOfPrefab == typeof(VehicleInfo))
                    {
                        PrefabWasVehicle?.Invoke();
                    }
                    else if(_typeOfPrefab == typeof(BuildingInfo))
                    {
                        PrefabWasBuilding?.Invoke();
                    }
                    else if(_typeOfPrefab == typeof(PropInfo))
                    {
                        PrefabWasProp?.Invoke();
                    }

                    _typeOfPrefab = properties.m_editPrefabInfo.GetType();

                    if(_typeOfPrefab == typeof(VehicleInfo))
                    {
                        PrefabBecameVehicle?.Invoke();
                    }
                    else if(_typeOfPrefab == typeof(BuildingInfo))
                    {
                        PrefabBecameBuilding?.Invoke();
                    }
                    else if(_typeOfPrefab == typeof(PropInfo))
                    {
                        PrefabBecameProp?.Invoke();
                    }
                }

                if(_prefabName != properties.m_editPrefabInfo.name)
                {
                    _prefabName = properties.m_editPrefabInfo.name;
                    PrefabChanged?.Invoke();
                    Util.Log($"Prefab changed to '{_prefabName}'");
                }

                int trailerCount = ((properties.m_editPrefabInfo as VehicleInfo)?.m_trailers != null) ? (properties.m_editPrefabInfo as VehicleInfo).m_trailers.Length : 0;
                bool trailersHaveChanged = false;
                if(_trailerNames.Length != trailerCount)
                {
                    trailersHaveChanged = true;
                }
                else
                {
                    for(int i = 0; i < trailerCount; i++)
                    {
                        if(_trailerNames[i] != (properties.m_editPrefabInfo as VehicleInfo).m_trailers[i].m_info.name)
                        {
                            trailersHaveChanged = true;
                            break;
                        }
                    }
                }               
                if(trailersHaveChanged)
                {
                    _trailerNames = new string[trailerCount];
                    for(int i = 0; i < trailerCount; i++)
                    {
                        _trailerNames[i] = (properties.m_editPrefabInfo as VehicleInfo).m_trailers[i].m_info.name;
                    }
                    TrailersChanged?.Invoke(_trailerNames);
                }
            }
        }
    }
}
