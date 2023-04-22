﻿using ColossalFramework;
using System;
using UnityEngine;
using static BuildingInfo;
using static PathUnit;
using static RenderManager;
using static Vehicle;

namespace ExtendedAssetEditor.Detour
{
    internal static class VehicleDetours
    {
        private static void RenderLodMesh(VehicleInfoBase info, ref Matrix4x4 tyreMatrix, ref Vector4 lightState, ref Vector4 tyrePosition, Color color, ref Vector3 position, ref Matrix4x4 bodyMatrix)
        {
            VehicleManager vehicleManager = Singleton<VehicleManager>.instance;
            MaterialPropertyBlock matBlock = vehicleManager.m_materialBlock;
            matBlock.Clear();
            matBlock.SetMatrix(vehicleManager.ID_TyreMatrix, tyreMatrix);
            matBlock.SetVector(vehicleManager.ID_TyrePosition, tyrePosition);
            matBlock.SetVector(vehicleManager.ID_LightState, lightState);
            Mesh mesh = info.m_lodMesh;
            Material material = info.m_lodMaterial;
            if (mesh != null && material != null)
            {
                matBlock.SetVectorArray(vehicleManager.ID_TyreLocation, info.m_generatedInfo.m_tyres);
                if (!(info is VehicleInfo))
                {
                    // Required for submeshes
                    info.m_lodTransforms.SetValues(bodyMatrix);
                    matBlock.SetMatrixArray(vehicleManager.ID_VehicleTransform, info.m_lodTransforms);
                    info.m_lodLightStates.SetValues(lightState);
                    matBlock.SetVectorArray(vehicleManager.ID_VehicleLightState, info.m_lodLightStates);
                    info.m_lodColors.ForEachRef((ref Vector4 c) => c = color.linear);
                    matBlock.SetVectorArray(vehicleManager.ID_VehicleColor, info.m_lodColors);
                    info.m_lodMin = Vector3.Min(info.m_lodMin, position);
                    info.m_lodMax = Vector3.Max(info.m_lodMax, position);
                    Bounds bounds = new Bounds();
                    bounds.SetMinMax(info.m_lodMin - new Vector3(100f, 100f, 100f), info.m_lodMax + new Vector3(100f, 100f, 100f));
                    mesh.bounds = bounds;
                }
                
                Graphics.DrawMesh(mesh, bodyMatrix, material, info.m_prefabDataLayer, null, 0, matBlock);
            }
        }

        // This is a modified copy of the original Vehicle.RenderInstance implementation because trying to do this via patching is a massive pain
        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, VehicleInfo info, Vector3 position, Quaternion rotation, Vector3 swayPosition, Vector4 lightState, Vector4 tyrePosition, Vector3 velocity, float acceleration, Color color, Flags flags, int variationMask, InstanceID id, bool underground, bool overground)
        {
            if ((cameraInfo.m_layerMask & (1 << info.m_prefabDataLayer)) == 0)
            {
                return;
            }

            Vector3 scale = Vector3.one;
            if ((flags & Flags.Inverted) != 0)
            {
                scale = new Vector3(-1f, 1f, -1f);
                Vector4 vector = lightState;
                lightState.x = vector.y;
                lightState.y = vector.x;
                lightState.z = vector.w;
                lightState.w = vector.z;
            }

            // CHANGE: We allow using the in-game rendering checks using an option
            bool isAssetEditor = Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor;
            var useDefaultEditorRendering = (!DisplayOptions.ActiveOptions.UseGateIndex) && isAssetEditor;

            info.m_vehicleAI.RenderExtraStuff(id.Vehicle, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[id.Vehicle], cameraInfo, id, position, rotation, tyrePosition, lightState, scale, swayPosition, underground, overground);
            if (cameraInfo.CheckRenderDistance(position, info.m_lodRenderDistance))
            {
                VehicleManager instance = Singleton<VehicleManager>.instance;
                Matrix4x4 bodyMatrix = info.m_vehicleAI.CalculateBodyMatrix(flags, ref position, ref rotation, ref scale, ref swayPosition);
                Matrix4x4 value = info.m_vehicleAI.CalculateTyreMatrix(flags, ref position, ref rotation, ref scale, ref bodyMatrix);
                if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None)
                {
                    RenderGroup.MeshData effectMeshData = info.m_vehicleAI.GetEffectMeshData();
                    EffectInfo.SpawnArea area = new EffectInfo.SpawnArea(bodyMatrix, effectMeshData, info.m_generatedInfo.m_tyres, info.m_lightPositions);
                    if (info.m_effects != null)
                    {
                        for (int i = 0; i < info.m_effects.Length; i++)
                        {
                            VehicleInfo.Effect effect = info.m_effects[i];
                            if (((effect.m_vehicleFlagsRequired | effect.m_vehicleFlagsForbidden) & flags) == effect.m_vehicleFlagsRequired && effect.m_parkedFlagsRequired == VehicleParked.Flags.None)
                            {
                                effect.m_effect.RenderEffect(id, area, velocity, acceleration, 1f, -1f, Singleton<SimulationManager>.instance.m_simulationTimeDelta, cameraInfo);
                            }
                        }
                    }
                }

                if ((flags & Flags.Inverted) != 0)
                {
                    tyrePosition.x = 0f - tyrePosition.x;
                    tyrePosition.y = 0f - tyrePosition.y;
                }

                MaterialPropertyBlock materialBlock = instance.m_materialBlock;
                materialBlock.Clear();
                materialBlock.SetMatrix(instance.ID_TyreMatrix, value);
                materialBlock.SetVector(instance.ID_TyrePosition, tyrePosition);
                materialBlock.SetVector(instance.ID_LightState, lightState);
                if (!isAssetEditor)
                {
                    materialBlock.SetColor(instance.ID_Color, color);
                }

                bool isMainMeshRendered = true;
                if (isAssetEditor)
                {
                    isMainMeshRendered = BuildingDecoration.IsMainMeshRendered();
                }

                if (info.m_subMeshes != null)
                {
                    for (int j = 0; j < info.m_subMeshes.Length; j++)
                    {
                        VehicleInfo.MeshInfo meshInfo = info.m_subMeshes[j];
                        VehicleInfoBase subInfo = meshInfo.m_subInfo;
                        
                        // CHANGE: Asset editor rendering
                        if ((!useDefaultEditorRendering && ((meshInfo.m_vehicleFlagsRequired | meshInfo.m_vehicleFlagsForbidden) & flags) == meshInfo.m_vehicleFlagsRequired && (meshInfo.m_variationMask & variationMask) == 0 && meshInfo.m_parkedFlagsRequired == VehicleParked.Flags.None) || (useDefaultEditorRendering && BuildingDecoration.IsSubMeshRendered(subInfo)))
                        {
                            if (!(subInfo != null))
                            {
                                continue;
                            }

                            instance.m_drawCallData.m_defaultCalls++;
                            if (underground)
                            {
                                if (subInfo.m_undergroundMaterial == null && subInfo.m_material != null)
                                {
                                    VehicleProperties properties = instance.m_properties;
                                    if (properties != null)
                                    {
                                        subInfo.m_undergroundMaterial = new Material(properties.m_undergroundShader);
                                        subInfo.m_undergroundMaterial.CopyPropertiesFromMaterial(subInfo.m_material);
                                    }
                                }

                                subInfo.m_undergroundMaterial.SetVectorArray(instance.ID_TyreLocation, subInfo.m_generatedInfo.m_tyres);
                                Graphics.DrawMesh(subInfo.m_mesh, bodyMatrix, subInfo.m_undergroundMaterial, instance.m_undergroundLayer, null, 0, materialBlock);
                            }

                            if (overground)
                            {
                                subInfo.m_material.SetVectorArray(instance.ID_TyreLocation, subInfo.m_generatedInfo.m_tyres);
                                Graphics.DrawMesh(subInfo.m_mesh, bodyMatrix, subInfo.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                            }
                        }
                        else if (subInfo == null)
                        {
                            isMainMeshRendered = false;
                        }
                    }
                }

                if (!isMainMeshRendered)
                {
                    return;
                }

                instance.m_drawCallData.m_defaultCalls++;
                if (underground)
                {
                    if (info.m_undergroundMaterial == null && info.m_material != null)
                    {
                        VehicleProperties properties2 = instance.m_properties;
                        if (properties2 != null)
                        {
                            info.m_undergroundMaterial = new Material(properties2.m_undergroundShader);
                            info.m_undergroundMaterial.CopyPropertiesFromMaterial(info.m_material);
                        }
                    }

                    info.m_undergroundMaterial.SetVectorArray(instance.ID_TyreLocation, info.m_generatedInfo.m_tyres);
                    Graphics.DrawMesh(info.m_mesh, bodyMatrix, info.m_undergroundMaterial, instance.m_undergroundLayer, null, 0, materialBlock);
                }

                if (overground)
                {
                    info.m_material.SetVectorArray(instance.ID_TyreLocation, info.m_generatedInfo.m_tyres);
                    Graphics.DrawMesh(info.m_mesh, bodyMatrix, info.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                }

                return;
            }

            Matrix4x4 bodyMatrix2 = info.m_vehicleAI.CalculateBodyMatrix(flags, ref position, ref rotation, ref scale, ref swayPosition);
            // CHANGE: Asset editor rendering
            if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor)
            {
                // The LOD rendering logic further down the method may not work, because the combined LOD meshes are not always present for the main meshes of editor assets.
                // In case they are missing we will render them ourselves.
                var renderMainLodMeshEditor = true;
                var tyreMatrix = info.m_vehicleAI.CalculateTyreMatrix(flags, ref position, ref rotation, ref scale, ref bodyMatrix2);

                if (!useDefaultEditorRendering)
                {
                    if (info.m_subMeshes != null)
                    {
                        for (int l = 0; l < info.m_subMeshes.Length; l++)
                        {
                            VehicleInfo.MeshInfo meshInfo = info.m_subMeshes[l];
                            VehicleInfoBase subInfo = meshInfo.m_subInfo;
                            //var missingCombinedLodMeshes = (subInfo != null) ? (subInfo.m_lodMeshCombined1 == null) : (info.m_lodMeshCombined1 == null);

                            if (((meshInfo.m_vehicleFlagsRequired | meshInfo.m_vehicleFlagsForbidden) & flags) == meshInfo.m_vehicleFlagsRequired && (meshInfo.m_variationMask & variationMask) == 0 && meshInfo.m_parkedFlagsRequired == VehicleParked.Flags.None)
                            {
                                if (subInfo == null)
                                {
                                    // Main mesh should be shown. Render it now if we don't have the combined LOD meshes, render it later if we do.
                                    renderMainLodMeshEditor = true;
                                    continue;
                                }

                                // We only want to use this rendering when the combined meshes are missing
                                RenderLodMesh(subInfo, ref tyreMatrix, ref lightState, ref tyrePosition, color, ref position, ref bodyMatrix2);
                            }
                            else if (subInfo == null)
                            {
                                // Main mesh should be hidden
                                renderMainLodMeshEditor = false;
                            }
                        }
                    }
                }
                
                if (renderMainLodMeshEditor)
                {
                    RenderLodMesh(info, ref tyreMatrix, ref lightState, ref tyrePosition, color, ref position, ref bodyMatrix2);
                }

                return;
            }
            else if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None)
            {
                RenderGroup.MeshData effectMeshData2 = info.m_vehicleAI.GetEffectMeshData();
                EffectInfo.SpawnArea area2 = new EffectInfo.SpawnArea(bodyMatrix2, effectMeshData2, info.m_generatedInfo.m_tyres, info.m_lightPositions);
                if (info.m_effects != null)
                {
                    for (int k = 0; k < info.m_effects.Length; k++)
                    {
                        VehicleInfo.Effect effect2 = info.m_effects[k];
                        if (((effect2.m_vehicleFlagsRequired | effect2.m_vehicleFlagsForbidden) & flags) == effect2.m_vehicleFlagsRequired && effect2.m_parkedFlagsRequired == VehicleParked.Flags.None)
                        {
                            effect2.m_effect.RenderEffect(id, area2, velocity, acceleration, 1f, -1f, Singleton<SimulationManager>.instance.m_simulationTimeDelta, cameraInfo);
                        }
                    }
                }
            }

            bool renderMainLodMesh = true;
            if (info.m_subMeshes != null)
            {
                for (int l = 0; l < info.m_subMeshes.Length; l++)
                {
                    VehicleInfo.MeshInfo meshInfo = info.m_subMeshes[l];
                    VehicleInfoBase subInfo = meshInfo.m_subInfo;
                    if (((meshInfo.m_vehicleFlagsRequired | meshInfo.m_vehicleFlagsForbidden) & flags) == meshInfo.m_vehicleFlagsRequired && (meshInfo.m_variationMask & variationMask) == 0 && meshInfo.m_parkedFlagsRequired == VehicleParked.Flags.None)
                    {
                        if (subInfo == null)
                        {
                            continue;
                        }

                        if (underground)
                        {
                            subInfo.m_undergroundLodTransforms[subInfo.m_undergroundLodCount] = bodyMatrix2;
                            subInfo.m_undergroundLodLightStates[subInfo.m_undergroundLodCount] = lightState;
                            ref Vector4 reference = ref subInfo.m_undergroundLodColors[subInfo.m_undergroundLodCount];
                            reference = color.linear;
                            subInfo.m_undergroundLodMin = Vector3.Min(subInfo.m_undergroundLodMin, position);
                            subInfo.m_undergroundLodMax = Vector3.Max(subInfo.m_undergroundLodMax, position);
                            if (++subInfo.m_undergroundLodCount == subInfo.m_undergroundLodTransforms.Length)
                            {
                                RenderUndergroundLod(cameraInfo, subInfo);
                            }
                        }

                        if (overground)
                        {
                            subInfo.m_lodTransforms[subInfo.m_lodCount] = bodyMatrix2;
                            subInfo.m_lodLightStates[subInfo.m_lodCount] = lightState;
                            ref Vector4 reference2 = ref subInfo.m_lodColors[subInfo.m_lodCount];
                            reference2 = color.linear;
                            subInfo.m_lodMin = Vector3.Min(subInfo.m_lodMin, position);
                            subInfo.m_lodMax = Vector3.Max(subInfo.m_lodMax, position);
                            if (++subInfo.m_lodCount == subInfo.m_lodTransforms.Length)
                            {
                                RenderLod(cameraInfo, subInfo);
                            }
                        }
                    }
                    else if (subInfo == null)
                    {
                        renderMainLodMesh = false;
                    }
                }
            }

            if (!renderMainLodMesh)
            {
                return;
            }

            if (underground)
            {
                info.m_undergroundLodTransforms[info.m_undergroundLodCount] = bodyMatrix2;
                info.m_undergroundLodLightStates[info.m_undergroundLodCount] = lightState;
                ref Vector4 reference3 = ref info.m_undergroundLodColors[info.m_undergroundLodCount];
                reference3 = color.linear;
                info.m_undergroundLodMin = Vector3.Min(info.m_undergroundLodMin, position);
                info.m_undergroundLodMax = Vector3.Max(info.m_undergroundLodMax, position);
                if (++info.m_undergroundLodCount == info.m_undergroundLodTransforms.Length)
                {
                    RenderUndergroundLod(cameraInfo, info);
                }
            }

            if (overground)
            {
                info.m_lodTransforms[info.m_lodCount] = bodyMatrix2;
                info.m_lodLightStates[info.m_lodCount] = lightState;
                ref Vector4 reference4 = ref info.m_lodColors[info.m_lodCount];
                reference4 = color.linear;
                info.m_lodMin = Vector3.Min(info.m_lodMin, position);
                info.m_lodMax = Vector3.Max(info.m_lodMax, position);
                if (++info.m_lodCount == info.m_lodTransforms.Length)
                {
                    RenderLod(cameraInfo, info);
                }
            }
        }
    }
}
