using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExtendedAssetEditor
{
    public static class Util
    {
        private static Dictionary<Type, Dictionary<int, string>> m_lightEffectDict = new Dictionary<Type, Dictionary<int, string>>();
        private static bool m_initialized;

        public static void Log(object message, bool always = false)
        {
            //if(!enableLogs && !always) { return; }

            Debug.Log(Mod.name + ": " + message.ToString());
        }

        public static void LogError(object message)
        {
            Debug.LogError(Mod.name + ": " + message.ToString());
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning(Mod.name + ": " + message.ToString());
        }

        /// <summary>
        /// Checks if info has an effect named effectName and returns the index of the effect. -1 if no effect can be found.
        /// </summary>
        public static int GetEffectIndex(VehicleInfo info, string effectName)
        {
            if(info != null && !string.IsNullOrEmpty(effectName))
            {
                if(info.m_effects != null)
                {
                    for(int i = 0; i < info.m_effects.Length; i++)
                    {
                        if(info.m_effects[i].m_effect.name == effectName)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the effect name of the light effect for the given info and index
        /// </summary>
        public static string LightIndexToEffectName(VehicleInfo info, int index)
        {
            if(!m_initialized)
            {
                Initialize();
            }

            string result = null;
            Dictionary<int, string> dictionary;
            m_lightEffectDict.TryGetValue(info.m_vehicleAI.GetType(), out dictionary);
            if(dictionary != null)
            {
                dictionary.TryGetValue(index, out result);
            }
            return result;
        }

        /// <summary>
        /// Adds the effect with name effectName to info
        /// </summary>
        public static bool AddEffect(VehicleInfo info, string effectName, Vehicle.Flags flagsRequired = Vehicle.Flags.Created, Vehicle.Flags flagsForbidden = 0)
        {
            EffectInfo effect = EffectCollection.FindEffect(effectName);
            if(effect == null)
                return false;

            int size = info.m_effects != null ? info.m_effects.Length + 1 : 1;
            VehicleInfo.Effect[] tmp = new VehicleInfo.Effect[size];
            if(size > 1)
            {
                Array.Copy(info.m_effects, tmp, size - 1);
            }
            var newEffect = new VehicleInfo.Effect
            {
                m_effect = effect,
                m_parkedFlagsForbidden = VehicleParked.Flags.Created,
                m_parkedFlagsRequired = VehicleParked.Flags.None,
                m_vehicleFlagsForbidden = flagsForbidden,
                m_vehicleFlagsRequired = flagsRequired,
            };
            tmp[size - 1] = newEffect;
            info.m_effects = tmp;

            return true;
        }

        /// <summary>
        /// Removes an effect at index
        /// </summary>
        public static bool RemoveEffect(VehicleInfo info, int index)
        {
            if(info.m_effects == null || index < 0 || index >= info.m_effects.Length)
                return false;

            List<VehicleInfo.Effect> tmp = new List<VehicleInfo.Effect>();
            tmp.AddRange(info.m_effects);
            tmp.RemoveAt(index);
            info.m_effects = tmp.ToArray();
            return true;
        }

        /// <summary>
        /// Removes all effects with the name effectName
        /// </summary>
        public static void RemoveEffect(VehicleInfo info, string effectName)
        {
            int index; 
            do
            {
                index = GetEffectIndex(info, effectName);
                RemoveEffect(info, index);
            }
            while(index >= 0);
        }

        /// <summary>
        /// Removes all effects that have the given name and at least the given requiredflags set
        /// </summary>
        public static void RemoveEffect(VehicleInfo info, string effectName, Vehicle.Flags requiredFlags)
        {
            int index;
            do
            {
                index = GetEffectIndex(info, effectName);

                if(info.m_effects == null || index < 0 || index >= info.m_effects.Length)
                    continue;

                if((info.m_effects[index].m_vehicleFlagsRequired & requiredFlags) > 0)
                {
                    List<VehicleInfo.Effect> tmp = new List<VehicleInfo.Effect>();
                    tmp.AddRange(info.m_effects);
                    tmp.RemoveAt(index);
                    info.m_effects = tmp.ToArray();
                }
                
            }
            while(index >= 0);
        }

        /// <summary>
        /// Returns a copy of the array with the last element removed
        /// </summary>
        public static T[] ShortenArray<T>(T[] array)
        {
            List<T> tmp = new List<T>();
            tmp.AddRange(array);
            tmp.RemoveAt(tmp.Count - 1);
            return tmp.ToArray();
        }

        /// <summary>
        /// Returns a copy of the array with newItem added to the end
        /// </summary>
        public static T[] LengthenArray<T>(T[] array, T newItem)
        {
            List<T> tmp = new List<T>();
            tmp.AddRange(array);
            tmp.Add(newItem);
            return tmp.ToArray();
        }

        /// <summary>
        /// Creates and instantiates a copy of a VehicleInfo
        /// </summary>
        public static VehicleInfo InstantiateVehicleCopy(VehicleInfo template)
        {
            if(template == null)
            {
                return null;
            }

            VehicleInfo copyInfo = GameObject.Instantiate(template);
            copyInfo.name = template.name;
            copyInfo.gameObject.SetActive(false);

            // Create generated info
            copyInfo.m_generatedInfo = ScriptableObject.CreateInstance<VehicleInfoGen>();
            copyInfo.m_generatedInfo.name = copyInfo.name;
            copyInfo.m_generatedInfo.m_vehicleInfo = copyInfo;
            copyInfo.CalculateGeneratedInfo();

            // Create LOD object
            if(template.m_lodObject != null)
            {
                GameObject copyLod = GameObject.Instantiate(template.m_lodObject);
                copyLod.SetActive(false);
                // Set sharedmaterial
                Renderer lodRenderer = copyLod.GetComponent<Renderer>();
                if(lodRenderer != null)
                {
                    lodRenderer.sharedMaterial = lodRenderer.material;
                }
                copyInfo.m_lodObject = copyLod;
            }

            // Set sharedmaterial
            Renderer r = copyInfo.GetComponent<Renderer>();
            if(r != null)
            {
                r.sharedMaterial = r.material;
            }
            copyInfo.InitializePrefab();
            copyInfo.m_prefabInitialized = true;
            return copyInfo;
        }

        /// <summary>
        /// Creates and instantiates a copy of a PropInfo
        /// </summary>
        public static PropInfo InstantiatePropCopy(PropInfo template)
        {
            if(template == null)
            {
                return null;
            }

            PropInfo copyInfo = GameObject.Instantiate(template);
            copyInfo.name = template.name;
            copyInfo.gameObject.SetActive(false);

            // Create generated info
            copyInfo.m_generatedInfo = ScriptableObject.CreateInstance<PropInfoGen>();
            copyInfo.m_generatedInfo.name = copyInfo.name;
            copyInfo.m_generatedInfo.m_propInfo = copyInfo;
            copyInfo.CalculateGeneratedInfo();

            // Create LOD object
            if(template.m_lodObject != null)
            {
                GameObject copyLod = GameObject.Instantiate(template.m_lodObject);
                copyLod.SetActive(false);
                // Set sharedmaterial
                Renderer lodRenderer = copyLod.GetComponent<Renderer>();
                if(lodRenderer != null)
                {
                    lodRenderer.sharedMaterial = lodRenderer.material;
                }
                copyInfo.m_lodObject = copyLod;
            }

            // Set sharedmaterial
            Renderer r = copyInfo.GetComponent<Renderer>();
            if(r != null)
            {
                r.sharedMaterial = r.material;
            }
            copyInfo.InitializePrefab();
            copyInfo.m_prefabInitialized = true;
            return copyInfo;
        }

        private static void Initialize()
        {
            m_initialized = true;
            Dictionary<int, string> trains = new Dictionary<int, string>();
            trains.Add(0, "Train Light Left");
            trains.Add(1, "Train Light Right");
            trains.Add(2, "Train Light Center");

            /*var largeCar = new Dictionary<int, string>();
            largeCar.Add(0, "Large Car Light Left");
            largeCar.Add(1, "Large Car Light Right");

            var smallCar = new Dictionary<int, string>();
            smallCar.Add(0, "Small Car Light Left");
            smallCar.Add(1, "Small Car Light Right");

            var fireTruck = new Dictionary<int, string>();
            fireTruck.Add(2, "Fire Truck Light Left");
            fireTruck.Add(3, "Fire Truck Light Right");
            fireTruck.Add(4, "Fire Truck Light Left2");
            fireTruck.Add(5, "Fire Truck Light Right2");

            var ambulance = new Dictionary<int, string>();
            ambulance.Add(2, "Ambulance Light Left");
            ambulance.Add(3, "Ambulance Light Right");
            ambulance.Add(4, "Ambulance Light Left2");
            ambulance.Add(5, "Ambulance Light Right2");

            var police = new Dictionary<int, string>();
            police.Add(2, "Police Car Light Left");
            police.Add(3, "Police Car Light Right");

            m_lightEffectDict.Add(typeof(MaintenanceTruckAI), largeCar);
            m_lightEffectDict.Add(typeof(GarbageTruckAI), largeCar);
            m_lightEffectDict.Add(typeof(CargoTruckAI), largeCar);
            m_lightEffectDict.Add(typeof(MaintenanceTruckAI), largeCar);*/

            m_lightEffectDict.Add(typeof(PassengerTrainAI), trains);
            m_lightEffectDict.Add(typeof(CargoTrainAI), trains);
        }

        /// <summary>
        /// Makes sure everyone gets the same paintjob. Taken from DecorationPropertiesPanel.
        /// </summary>
        public static void CopyTrailerColorFromMain()
        {
            VehicleInfo vehicleInfo = ToolsModifierControl.toolController.m_editPrefabInfo as VehicleInfo;
            if(vehicleInfo != null)
            {
                Renderer component = vehicleInfo.GetComponent<Renderer>();
                if(component != null && component.sharedMaterial != null)
                {
                    Color color = component.sharedMaterial.GetColor("_Color");
                    if(vehicleInfo.m_trailers != null && vehicleInfo.m_trailers.Length > 0)
                    {
                        VehicleInfo info = vehicleInfo.m_trailers[0].m_info;
                        Renderer component2 = info.GetComponent<Renderer>();
                        if(component2 != null && component2.sharedMaterial != null)
                        {
                            component2.sharedMaterial.SetColor("_Color", color);
                        }
                        if(info.m_lodObject != null)
                        {
                            Renderer component3 = info.m_lodObject.GetComponent<Renderer>();
                            if(component3 != null && component3.sharedMaterial != null)
                            {
                                component3.sharedMaterial.SetColor("_Color", color);
                            }
                        }
                    }
                }
            }
        }
    }
}
