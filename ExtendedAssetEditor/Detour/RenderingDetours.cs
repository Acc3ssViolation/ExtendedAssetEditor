using System;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using System.Reflection;

namespace ExtendedAssetEditor.Detour
{
    /// <summary>
    /// Handles all changes to rendering of vehicles in the asset editor
    /// </summary>
    public class RenderingDetours : IDetour
    {
        private readonly List<DetourItem> _detours;

        public RenderingDetours()
        {
            _detours = new List<DetourItem>();
            MethodInfo original, detour;

            original = typeof(ToolManager).GetMethod("EndRenderingImpl", BindingFlags.Instance | BindingFlags.NonPublic);
            detour = GetType().GetMethod(nameof(EndRenderingImpl), BindingFlags.Instance | BindingFlags.NonPublic);
            _detours.Add(new DetourItem("ToolManager.EndRenderingImpl", original, detour));
        }

        public void Deploy()
        {
            foreach(var detour in _detours)
            {
                detour.Deploy();
            }
        }

        public void Revert()
        {
            foreach(var detour in _detours)
            {
                detour.Revert();
            }
        }

        /// <summary>
        /// Detour for ToolManager.EndRenderingImpl
        /// </summary>
        private void EndRenderingImpl(RenderManager.CameraInfo cameraInfo)
        {
            ToolController properties = Singleton<ToolManager>.instance.m_properties;
            if(properties != null)
            {
                PrefabInfo editPrefabInfo = properties.m_editPrefabInfo;
                if(editPrefabInfo != null)
                {
                    VehicleInfo vehicleInfo = editPrefabInfo as VehicleInfo;
                    if(vehicleInfo != null)
                    {
                        // Modified implementation
                        RenderInfo(cameraInfo, vehicleInfo, new Vector3(0f, 60f, 0f), false);
                        if(vehicleInfo.m_trailers != null)
                        {
                            // Bug(?) fix: main info's m_attachOffsetBack did not get applied
                            float num = vehicleInfo.m_generatedInfo.m_size.z * 0.5f - vehicleInfo.m_attachOffsetBack;

                            for(int i = 0; i < vehicleInfo.m_trailers.Length; i++)
                            {
                                VehicleInfo info = vehicleInfo.m_trailers[i].m_info;
                                // New: Support correct display of inverted trailers
                                bool isInverted = (vehicleInfo.m_trailers[i].m_invertProbability >= 100);
                                float frontDelta = info.m_generatedInfo.m_size.z * 0.5f - info.m_attachOffsetFront;
                                float backDelta = info.m_generatedInfo.m_size.z * 0.5f - info.m_attachOffsetBack;
                                num += isInverted ? backDelta : frontDelta;
                                Vector3 position = new Vector3(0f, 60f, 0f) + new Vector3(0f, 0f, -num);

                                RenderInfo(cameraInfo, info, position, isInverted);

                                // Change for inverted vehicles
                                num += isInverted ? frontDelta : backDelta;
                            }
                        }
                    }
                    else
                    {
                        //Default
                        editPrefabInfo.RenderMesh(cameraInfo);
                    }
                    
                }
                try
                {
                    ToolBase currentTool = properties.CurrentTool;
                    if(currentTool != null)
                    {
                        currentTool.RenderGeometry(cameraInfo);
                    }
                }
                catch(Exception ex)
                {
                    UIView.ForwardException(ex);
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Tool error: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        private void RenderInfo(RenderManager.CameraInfo cameraInfo, VehicleInfo info, Vector3 position, bool inverted)
        {
            // Full implementation of VehicleInfo.RenderMesh so we can change its facing direction
            if(info.m_lodMeshData == null && info.m_lodMesh != null && info.m_lodMesh.vertexCount > 0)
            {
                info.m_lodMeshData = new RenderGroup.MeshData(info.m_lodMesh);
                info.m_lodMeshData.UpdateBounds();
            }
            // Change for inverted vehicles
            // Quaternion rotation = isInverted ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            Quaternion rotation = Quaternion.identity;
            Vehicle.Flags flags = Vehicle.Flags.Created | Vehicle.Flags.Spawned;
            if(inverted)
            {
                flags |= Vehicle.Flags.Inverted;
            }
            // Effect display options
            if(DisplayOptions.ActiveOptions.Reversed)
            {
                flags |= Vehicle.Flags.Reversed;
            }
            if(DisplayOptions.ActiveOptions.ShowLanding)
            {
                flags |= Vehicle.Flags.Landing;
            }
            if(DisplayOptions.ActiveOptions.ShowTakeOff)
            {
                flags |= Vehicle.Flags.TakingOff;
            }
            if(DisplayOptions.ActiveOptions.ShowEmergency)
            {
                flags |= Vehicle.Flags.Emergency1;
            }
            if(DisplayOptions.ActiveOptions.ShowEmergency2)
            {
                flags |= Vehicle.Flags.Emergency2;
            }

            var gateIndex = DisplayOptions.ActiveOptions.GateIndex;
            if (gateIndex < 0)
                gateIndex = 0;
            else if (gateIndex > 31)
                gateIndex = 31;
            var variation = ~(1 << gateIndex);

            // NOTE: We use the VehicleDetours implementation which we need to render some things
            if (DisplayOptions.ActiveOptions.UseGateIndex)
            {
                VehicleDetours.RenderInstance(cameraInfo, info, position, rotation, Vector3.zero, Vector4.zero, Vector4.zero, Vector3.zero, 0f, info.m_color0, flags, variation, InstanceID.Empty);
            }
            else
            {
                Vehicle.RenderInstance(cameraInfo, info, position, rotation, Vector3.zero, Vector4.zero, Vector4.zero, Vector3.zero, 0f, info.m_color0, flags, 0, variation, InstanceID.Empty, false, true);
            }
            // End of VehicleInfo.RenderMesh
        }
    }
}
