using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ColossalFramework.UI;
using HarmonyLib;

namespace ExtendedAssetEditor.Detour
{
    /// <summary>
    /// Detours PrefabInfo methods
    /// </summary>
    public class BuildingDecorationDetour : IHarmonyDetour
    {
        private static bool m_prefabIsVehicle;

        public void Deploy(Harmony harmony)
        {
            // public static bool IsSubMeshRendered(PrefabInfo subMesh)
            var addFieldSrc = typeof(BuildingDecoration).GetMethod("IsSubMeshRendered", BindingFlags.Public | BindingFlags.Static);
            var addFieldPre = typeof(BuildingDecorationDetour).GetMethod("IsSubMeshRendered", BindingFlags.Static | BindingFlags.Public);

            var prefix = new HarmonyMethod(addFieldPre);
            harmony.Patch(addFieldSrc, prefix);

            PrefabWatcher.instance.prefabBecameVehicle += PrefabBecameVehicle;
            PrefabWatcher.instance.prefabWasVehicle += PrefabWasVehicle;
        }

        public void Revert(Harmony harmony)
        {
            // TODO: Harmony cleanup

            PrefabWatcher.instance.prefabBecameVehicle -= PrefabBecameVehicle;
            PrefabWatcher.instance.prefabWasVehicle -= PrefabWasVehicle;
        }

        private void PrefabWasVehicle()
        {
            m_prefabIsVehicle = false;
        }

        private void PrefabBecameVehicle()
        {
            m_prefabIsVehicle = true;
        }

        public static bool IsSubMeshRendered(PrefabInfo subMesh, ref bool __result)
        {
            if (!m_prefabIsVehicle)
            {
                // Don't override the result
                return false;
            }

            // From Vehicle.RenderInstance
            // ((meshInfo.m_vehicleFlagsRequired | meshInfo.m_vehicleFlagsForbidden) & flags) == meshInfo.m_vehicleFlagsRequired && (meshInfo.m_variationMask & variationMask) == 0 && meshInfo.m_parkedFlagsRequired == VehicleParked.Flags.None)

            // TODO: For this to work we would need some way of mapping the PrefabInfo back to its MeshInfo. This is difficult.
            return false;
        }
    }
}
