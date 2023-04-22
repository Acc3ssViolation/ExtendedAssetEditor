using HarmonyLib;
using System;

namespace ExtendedAssetEditor.Detour
{
    internal class HarmonyDetours : IDetour
    {
        private readonly IHarmonyDetour[] _detours = new IHarmonyDetour[] { 
            new DecorationPropertiesPanelDetour(),
        };

        private Harmony _harmony;

        public void Deploy()
        {
            if (_harmony != null)
            {
                Util.LogWarning("Harmony patches already present");
                return;
            }

            var harmony = new Harmony(Mod.HarmonyPackage);
            Harmony.VersionInfo(out Version currentVersion);
            
            Util.Log("Harmony v" + currentVersion, true);

            foreach (var detour in _detours)
            {
                detour.Deploy(harmony);
            }

            Util.Log("Harmony patches applied", true);
            _harmony = harmony;
        }

        public void Revert()
        {
            throw new NotSupportedException("Cannot revert Harmony patches");
        }
    }
}
