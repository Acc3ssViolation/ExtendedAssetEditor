using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtendedAssetEditor.Detour
{
    public class PropManagerDetour : IDetour
    {
        private DetourItem m_checkLimitsDetour;
        public const int MAX_ASSET_PROPS_AND_TREES = 512;

        public PropManagerDetour()
        {
            var original = typeof(PropManager).GetMethod("CheckLimits");
            var replacement = GetType().GetMethod("CheckLimits");
            m_checkLimitsDetour = new DetourItem("PropManager.CheckLimits", original, replacement);
        }

        public void Deploy()
        {
            m_checkLimitsDetour.Deploy();
        }

        public void Revert()
        {
            m_checkLimitsDetour.Revert();
        }

        public bool CheckLimits()
        {
            ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
            if((mode & ItemClass.Availability.MapEditor) != ItemClass.Availability.None)
            {
                if(Singleton<PropManager>.instance.m_propCount >= 50000)
                {
                    return false;
                }
            }
            else if((mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                // Up the limit for asset editor props and trees
                if(Singleton<PropManager>.instance.m_propCount + Singleton<TreeManager>.instance.m_treeCount >= MAX_ASSET_PROPS_AND_TREES)
                {
                    return false;
                }
            }
            else if(Singleton<PropManager>.instance.m_propCount >= 65531)
            {
                return false;
            }
            return true;
        }
    }
}
