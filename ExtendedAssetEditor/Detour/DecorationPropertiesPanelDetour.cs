using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;
using Harmony;
using System.Runtime.CompilerServices;

namespace ExtendedAssetEditor.Detour
{
    /// <summary>
    /// Detours PrefabInfo methods
    /// </summary>
    public class DecorationPropertiesPanelDetour : IDetour
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

        public void Deploy()
        {
            var harmony = HarmonyInstance.Create(Mod.harmonyPackage);
            Version currentVersion;
            if(harmony.VersionInfo(out currentVersion).ContainsKey(Mod.harmonyPackage))
            {
                Util.LogWarning("Harmony patches already present");
                return;
            }
            Util.Log("Harmony v" + currentVersion, true);

            // Harmony

            // private UIComponent AddField(UIComponent container, string locale, float width, Type type, string name, int arrayIndex, object target, object initialValue)
            var addFieldSrc = typeof(DecorationPropertiesPanel).GetMethod("AddField", BindingFlags.NonPublic | BindingFlags.Instance, null,
                new Type[] { typeof(UIComponent), typeof(string), typeof(float), typeof(Type), typeof(string), typeof(int), typeof(object), typeof(object) },
                null);

            var addFieldPost = typeof(DecorationPropertiesPanelDetour).GetMethod("AddField_Postfix", BindingFlags.Static | BindingFlags.Public);

            Util.Log("DecorationPropertiesPanel.TrySpawn is " + (addFieldSrc == null ? "null" : "not null"));
            Util.Log("Patching methods...", true);

            harmony.Patch(addFieldSrc, new HarmonyMethod(addFieldPost), null);

            Util.Log("Harmony patches applied", true);

        }

        public void Revert()
        {
            // TODO: Revert when possible
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AddField_Postfix(DecorationPropertiesPanel __instance, UIComponent container, Type type, float width, object target)
        {
            if(type == typeof(VehicleInfo.MeshInfo))
            {
                var info = (VehicleInfo.MeshInfo)target;
                addFieldInfo.Invoke(__instance, new object[] { container, "Variation", width, typeof(int), "m_variationMask", target, info.m_variationMask });
            }
        }
    }
}
