using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtendedAssetEditor.Detour
{
    internal interface IHarmonyDetour
    {
        void Deploy(Harmony harmony);
        void Revert(Harmony harmony);
    }
}
