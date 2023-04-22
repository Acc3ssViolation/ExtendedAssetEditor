using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;
using ColossalFramework;
using ICities;

namespace ExtendedAssetEditor.UI
{
    public class UIPropPanel : UIPanel
    {
        public const int WIDTH = 300;
        public const int HEIGHT = 220;

        private UIPanel m_selectRef;
        private UIButton m_vehicleButton;

        public override void Start()
        {
            base.Start();

            PrefabWatcher.Instance.PrefabBecameProp += () =>
            {
                Debug.Log("Enabling UIPropPanel");
                Show();
            };

            PrefabWatcher.Instance.PrefabWasProp += () =>
            {
                Debug.Log("Disabling UIPropPanel");
                Hide();
            };

            UIView view = UIView.GetAView();
            width = WIDTH;
            height = HEIGHT;
            backgroundSprite = "MenuPanel2";
            name = Mod.ModName + " Prop Panel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            // Start in the top right corner
            relativePosition = new Vector3(view.fixedWidth - width - 10, 60);

            m_selectRef = GameObject.Find("SelectReference").GetComponent<UIPanel>();
            m_selectRef.isVisible = false;

            CreateComponents();
        }

        void CreateComponents()
        {
            int headerHeight = 40;
            UIHelperBase uiHelper = new UIHelper(this);

            // Label
            UILabel label = AddUIComponent<UILabel>();
            label.text = "Prop Thingy";
            label.relativePosition = new Vector3(WIDTH / 2 - label.width / 2, 10);

            // Drag handle
            UIDragHandle handle = AddUIComponent<UIDragHandle>();
            handle.target = this;
            handle.constrainToScreen = true;
            handle.width = WIDTH;
            handle.height = headerHeight;
            handle.relativePosition = Vector3.zero;

            // Vehicle import button
            m_vehicleButton = UIUtils.CreateButton(this);
            m_vehicleButton.text = "Import Vehicle";
            m_vehicleButton.width = WIDTH - 20;
            m_vehicleButton.relativePosition = new Vector3(10, headerHeight + 10);
            m_vehicleButton.eventClicked += (c, b) =>
            {
                ImportVehicle();
            };
        }

        void ImportVehicle()
        {
            var assetImporterAssetTemplate = m_selectRef.GetComponent<AssetImporterAssetTemplate>();
            assetImporterAssetTemplate.ReferenceCallback = new AssetImporterAssetTemplate.ReferenceCallbackDelegate(OnConfirmLoad);
            assetImporterAssetTemplate.Reset();
            assetImporterAssetTemplate.RefreshWithFilter(AssetImporterAssetTemplate.Filter.Vehicles);
            m_selectRef.isVisible = true;
        }

        private void OnConfirmLoad(PrefabInfo reference)
        {
            if(reference != null)
            {
                var vehicle = reference as VehicleInfo;
                if(vehicle != null)
                {
                    var prefabInfo = Singleton<ToolController>.instance.m_editPrefabInfo;
                    if(prefabInfo != null)
                    {
                        var propInfo = prefabInfo as PropInfo;
                        if(propInfo != null)
                        {
                            SwapMeshes(vehicle, propInfo);
                        }
                        else
                        {
                            Debug.LogError("Expected a PropInfo as m_editPrefabInfo but didn't get one!");
                        }
                    }
                }
                else
                {
                    Debug.LogError("Expected a VehicleInfo but didn't get one!");
                }
            }
            m_selectRef.isVisible = false;
        }

        private void SwapMeshes(VehicleInfo template, PropInfo target)
        {
            var vehicle = Util.InstantiateVehicleCopy(template);
            var vehicleGO = vehicle.gameObject;
            var propGO = target.gameObject;

            var vehicleMf = vehicleGO.GetComponent<MeshFilter>();
            var propMf = propGO.GetComponent<MeshFilter>();

            // set mesh
            vehicleMf.sharedMesh = vehicleMf.mesh;
            propMf.mesh = vehicleMf.sharedMesh;
            propMf.sharedMesh = propMf.mesh;

            // set material
            var renderer = propGO.GetComponent<Renderer>();
            var vehRenderer = vehicleGO.GetComponent<Renderer>();
            if(renderer != null)
            {
                var shader = renderer.sharedMaterial.shader;
                var shaderKeyWords = renderer.sharedMaterial.shaderKeywords;

                var material = vehRenderer.sharedMaterial;
                CleanACIMap(material);
                renderer.sharedMaterial = material;

                renderer.sharedMaterial.shader = shader;
                renderer.sharedMaterial.shaderKeywords = shaderKeyWords;
            }

            // set lod
            if(vehicle.m_lodObject != null)
            {
                var lodRenderer = target.m_lodObject.GetComponent<Renderer>();
                var lodVehRenderer = vehicle.m_lodObject.GetComponent<Renderer>();

                var material = lodVehRenderer.sharedMaterial;
                CleanACIMap(material);
                lodVehRenderer.sharedMaterial = material;

                lodVehRenderer.sharedMaterial.shader = lodRenderer.sharedMaterial.shader;
                lodVehRenderer.sharedMaterial.shaderKeywords = lodRenderer.sharedMaterial.shaderKeywords;

                target.m_lodObject = vehicle.m_lodObject;
            }

            // Recalculate generated info
            target.CalculateGeneratedInfo();
            target.m_mesh = null;       // Required to get InitializePrefab to do its thing
            target.m_prefabInitialized = false;
            target.InitializePrefab();
            target.m_prefabInitialized = true;
            
            Destroy(vehicleGO);
        }

        /// <summary>
        /// Removes illumination data from the ACI map of the material.
        /// A new texture will be used.
        /// </summary>
        /// <param name="material">Material to modify</param>
        private void CleanACIMap(Material material)
        {
            var aci = material.GetTexture("_ACIMap") as Texture2D;
            if(aci != null)
            {
                var newTexture = new Texture2D(aci.width, aci.height, TextureFormat.RGB24, true);
                
                var pixels = aci.GetPixels32();

                for(int i = 0; i < pixels.Length; i++)
                {
                    pixels[i].b = 0;
                }

                newTexture.SetPixels32(pixels);
                newTexture.Apply(true);
                newTexture.Compress(true);
                material.SetTexture("_ACIMap", newTexture);
            }
            else
            {
                Debug.LogError("Could not get ACI map from material " + material.name);
            }
        }
    }
}
