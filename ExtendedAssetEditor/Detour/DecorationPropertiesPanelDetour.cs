/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ExtendedAssetEditor.Detour
{
    public class DecorationPropertiesPanelDetour : DecorationPropertiesPanel
    {
        private List<DetourItem> m_detours;

        public DecorationPropertiesPanelDetour()
        {
            m_detours = new List<DetourItem>();
            MethodInfo original, detour;

            original = typeof(ToolManager).GetMethod("ProcessIndirectFields", BindingFlags.Instance | BindingFlags.NonPublic);
            detour = GetType().GetMethod("ProcessIndirectFieldsDetour", BindingFlags.Instance | BindingFlags.NonPublic);
            m_detours.Add(new DetourItem("DecorationPropertiesPanel.ProcessIndirectFields", original, detour));
        }

        public void Deploy()
        {
            foreach(var detour in m_detours)
            {
                detour.Deploy();
            }
        }

        public void Revert()
        {
            foreach(var detour in m_detours)
            {
                detour.Revert();
            }
        }

        public void ProcessIndirectFieldsDetour()
        {
            BuildingInfo buildingInfo = ToolsModifierControl.toolController.m_editPrefabInfo as BuildingInfo;
            VehicleInfo vehicleInfo = ToolsModifierControl.toolController.m_editPrefabInfo as VehicleInfo;
            if(buildingInfo != null)
            {
                if(!this.m_UseTemplateMilestone)
                {
                    buildingInfo.m_UnlockMilestone = null;
                }
                buildingInfo.m_useColorVariations = this.m_UseColorVariations;
            }
            if(vehicleInfo != null)
            {
                VehicleInfo trailerAsset = this.m_TrailerAsset;
                if(trailerAsset != null)
                {
                    if(this.m_TrailerOffsetComponent != null)
                    {
                        DecorationPropertiesPanel.ReflectionInfo reflectionInfo = this.m_TrailerOffsetField.objectUserData as DecorationPropertiesPanel.ReflectionInfo;
                        if(reflectionInfo != null && (VehicleInfo)reflectionInfo.targetObject != trailerAsset)
                        {
                            reflectionInfo.targetObject = trailerAsset;
                            this.m_TrailerOffsetField.objectUserData = reflectionInfo;
                            this.m_TrailerOffsetField.text = trailerAsset.m_attachOffsetFront.ToString();
                            this.m_TrailerOffsetComponent.isVisible = (trailerAsset != null);
                        }
                    }
                    if(this.m_TrailerCount > DecorationPropertiesPanel.m_MaxTrailers)
                    {
                        this.m_TrailerCount = DecorationPropertiesPanel.m_MaxTrailers;
                        if(this.m_TrailerCountField != null)
                        {
                            this.m_TrailerCountField.color = Color.red;
                        }
                    }
                    vehicleInfo.m_trailers = new VehicleInfo.VehicleTrailer[this.m_TrailerCount];
                    for(int i = 0; i < this.m_TrailerCount; i++)
                    {
                        vehicleInfo.m_trailers[i].m_info = trailerAsset;
                        vehicleInfo.m_trailers[i].m_invertProbability = 0;
                        vehicleInfo.m_trailers[i].m_probability = 100;
                    }
                    this.CopyTrailerColorFromMain();
                }
                else
                {
                    vehicleInfo.m_trailers = null;
                    if(this.m_TrailerOffsetComponent != null)
                    {
                        this.m_TrailerOffsetComponent.isVisible = false;
                    }
                }
            }
        }
    }
}*/
