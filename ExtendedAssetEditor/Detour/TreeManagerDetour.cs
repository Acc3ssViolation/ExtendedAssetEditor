using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtendedAssetEditor.Detour
{
    public class TreeManagerDetour : IDetour
    {
        private DetourItem m_checkLimitsDetour;

        public TreeManagerDetour()
        {
            var original = typeof(TreeManager).GetMethod("CheckLimits");
            var replacement = GetType().GetMethod("CheckLimits");
            m_checkLimitsDetour = new DetourItem("TreeManager.CheckLimits", original, replacement);
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
                if(Singleton<TreeManager>.instance.m_treeCount >= 250000)
                {
                    return false;
                }
            }
            else if((mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                if(Singleton<TreeManager>.instance.m_treeCount + Singleton<PropManager>.instance.m_propCount >= PropManagerDetour.MAX_ASSET_PROPS_AND_TREES)
                {
                    return false;
                }
            }
            else if(Singleton<TreeManager>.instance.m_treeCount >= 262139)
            {
                return false;
            }
            return true;
        }
    }
}
