using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ExtendedAssetEditor.Detour
{
    /// <summary>
    /// Detours PrefabInfo methods
    /// </summary>
    public class PrefabInfoDetour
    {
        private DetourItem m_decorationAreaDetour;

        public PrefabInfoDetour()
        {
            var original = typeof(PrefabInfo).GetMethod("GetDecorationArea");
            var replacement = GetType().GetMethod("GetDecorationArea");
            m_decorationAreaDetour = new DetourItem("PrefabInfo.GetDecorationArea", original, replacement);
        }

        public void Deploy()
        {
            m_decorationAreaDetour.Deploy();
        }

        public void Revert()
        {
            m_decorationAreaDetour.Revert();
        }

        public virtual void GetDecorationArea(out int width, out int length, out float offset)
        {
            // Give larger width and length to give the camera a bit more room to move around in
            width = 64;
            length = 64;
            offset = 0f;
        }
    }
}
