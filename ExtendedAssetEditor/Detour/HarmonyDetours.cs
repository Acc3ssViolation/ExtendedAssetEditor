using HarmonyLib;
using System;
using UnityEngine;

namespace ExtendedAssetEditor.Detour
{
    internal class HarmonyDetours : IDetour
    {
        private IHarmonyDetour[] m_detours = new IHarmonyDetour[] { 
            new DecorationPropertiesPanelDetour(),
            // TODO: Enable this if we want to use it for variation displays
            //new BuildingDecorationDetour(),
        };

        private Harmony m_harmony;

        public void Deploy()
        {
            if (m_harmony != null)
            {
                Util.LogWarning("Harmony patches already present");
                return;
            }

            var harmony = new Harmony(Mod.harmonyPackage);
            Harmony.VersionInfo(out Version currentVersion);
            
            Util.Log("Harmony v" + currentVersion, true);

            foreach (var detour in m_detours)
            {
                detour.Deploy(harmony);
            }

            Util.Log("Harmony patches applied", true);
            m_harmony = harmony;
        }

        public void Revert()
        {
            if (m_harmony== null)
            {
                return;
            }

            foreach (var detour in m_detours)
            {
                detour.Revert(m_harmony);
            }
            m_harmony = null;

            Util.Log("Harmony patches removed", true);
        }
    }
}
