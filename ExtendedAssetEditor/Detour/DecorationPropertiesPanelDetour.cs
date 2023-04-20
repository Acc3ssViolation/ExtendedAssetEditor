using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;
using HarmonyLib;
using System.Runtime.CompilerServices;

namespace ExtendedAssetEditor.Detour
{
    /// <summary>
    /// Detours PrefabInfo methods
    /// </summary>
    public class DecorationPropertiesPanelDetour : IHarmonyDetour
    {
        private static MethodInfo addFieldInfo;

        public DecorationPropertiesPanelDetour()
        {
            // private UIComponent AddField(UIComponent container, string locale, float width, Type type, string name, object target, object initialValue)
            addFieldInfo = typeof(DecorationPropertiesPanel).GetMethod("AddField", BindingFlags.NonPublic | BindingFlags.Instance, null,
                new Type[] { typeof(UIComponent), typeof(string), typeof(float), typeof(Type), typeof(string), typeof(object), typeof(object) }, 
                null);

            Util.Log("AddField MethodInfo exists: " + (addFieldInfo != null).ToString());
        }

        public void Deploy(Harmony harmony)
        {
            // private UIComponent AddField(UIComponent container, string locale, float width, Type type, string name, int arrayIndex, object target, object initialValue)
            var addFieldSrc = typeof(DecorationPropertiesPanel).GetMethod("AddField", BindingFlags.NonPublic | BindingFlags.Instance, null,
                new Type[] { typeof(UIComponent), typeof(string), typeof(float), typeof(Type), typeof(string), typeof(int), typeof(object), typeof(object) },
                null);

            var addFieldPost = typeof(DecorationPropertiesPanelDetour).GetMethod("AddField_Postfix", BindingFlags.Static | BindingFlags.Public);

            Util.Log("DecorationPropertiesPanel.TrySpawn is " + (addFieldSrc == null ? "null" : "not null"));

            harmony.Patch(addFieldSrc, new HarmonyMethod(addFieldPost), null);
        }

        public void Revert(Harmony harmony)
        {
            // TODO: Revert when possible
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AddField_Postfix(DecorationPropertiesPanel __instance, UIComponent container, string locale, Type type, float width, string name, object target)
        {
            var wasMainMeshField = type == typeof(Vehicle.Flags) && name == "m_vehicleFlagsForbidden" && locale.StartsWith("Hide Main Mesh Cond.", StringComparison.CurrentCultureIgnoreCase);
            var wasSubMeshField = type == typeof(VehicleInfo.MeshInfo);
            var variationLocale = wasMainMeshField ? "Main Mesh Variation" : "Variation";
            if (wasMainMeshField || wasSubMeshField)
            {
                // TODO: Turn this into a dropdown, possibly with the option of a numerical override
                var info = (VehicleInfo.MeshInfo)target;
                var fieldName = nameof(info.m_variationMask);
                var hasVariationField = IsFieldReferenceAdded(container, fieldName);
                if (!hasVariationField)
                    addFieldInfo.Invoke(__instance, new object[] { container, variationLocale, width, typeof(int), fieldName, target, info.m_variationMask });
                else if (info.m_subInfo != null)
                    Util.Log($"Variation field was already added for submesh '{info.m_subInfo.name}'");
                else
                    Util.Log($"Variation field was already added for main mesh");
            }
        }

        private static bool IsFieldReferenceAdded(UIComponent container, string fieldName, object target = null)
        {
            return container.components.Any((c) =>
            {
                var info = c.Find<UITextField>("Value")?.objectUserData;
                if (info == null)
                    return false;

                try
                {
                    // Oh lord above
                    unsafe
                    {
                        var typed = *(ReflectionInfo*)&info;
                        if (typed.targetObject == target && typed.fieldName == fieldName)
                            return true;
                    }
                }
                catch (Exception e)
                {
                    Util.LogError(e);
                }

                return false;
            });
        }

        // Copy of DecorationPropertiesPanel.ReflectionInfo
        private class ReflectionInfo
        {
            public object targetObject;

            public string fieldName;

            public int elementIndex = -1;

            public ReflectionInfo nestedInfo;
        }
    }
}
