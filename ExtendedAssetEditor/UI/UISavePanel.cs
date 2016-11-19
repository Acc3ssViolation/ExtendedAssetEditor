using ColossalFramework.UI;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.IO;
using System.IO;
using ColossalFramework.Packaging;
using ColossalFramework.Globalization;

namespace ExtendedAssetEditor.UI
{
    public class UISavePanel : UIPanel
    {
        public const int WIDTH = 910;
        public const int HEIGHT = 640;

        private UIButton m_saveButton;
        private UIButton m_cancelButton;

        // TESTING
        private UITextField m_packageField;
        private UITextField m_nameField;
        //

        private VehicleInfo m_info;

        public override void Start()
        {
            base.Start();

            UIView view = UIView.GetAView();
            width = WIDTH;
            height = HEIGHT;
            backgroundSprite = "MenuPanel2";
            name = Mod.name + " Save Panel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            // Start in the center of the screen
            relativePosition = new Vector3((view.fixedWidth - width) / 2, (view.fixedHeight - height) / 2);

            CreateComponents();
        }

        private void CreateComponents()
        {
            int headerHeight = 40;

            // Label
            UILabel label = AddUIComponent<UILabel>();
            label.text = "Save Asset";
            label.relativePosition = new Vector3(WIDTH / 2 - label.width / 2, 10);

            // Drag handle
            UIDragHandle handle = AddUIComponent<UIDragHandle>();
            handle.target = this;
            handle.constrainToScreen = true;
            handle.width = WIDTH;
            handle.height = headerHeight;
            handle.relativePosition = Vector3.zero;

            // Fields
            label = AddUIComponent<UILabel>();
            label.text = "Package name:";
            label.relativePosition = new Vector3(10, headerHeight + 15);
            m_packageField = UIUtils.CreateTextField(this);
            m_packageField.width = 400;
            m_packageField.relativePosition = new Vector3(20 + label.width, headerHeight + 10);

            // Fields
            label = AddUIComponent<UILabel>();
            label.text = "Asset name:";
            label.relativePosition = new Vector3(10, headerHeight + 45);
            m_nameField = UIUtils.CreateTextField(this);
            m_nameField.width = 400;
            m_nameField.relativePosition = new Vector3(m_packageField.relativePosition.x, headerHeight + 40);

            // Save button
            m_saveButton = UIUtils.CreateButton(this);
            m_saveButton.text = "Save";
            m_saveButton.textScale = 1.3f;
            m_saveButton.width = 153f;
            m_saveButton.height = 47f;
            m_saveButton.eventClicked += (c, b) =>
            {
                string assetName = m_nameField.text.Trim();
                string packageName = m_packageField.text.Trim();
                string file = GetSavePathName(packageName);
                if(!File.Exists(file))
                {
                    SaveAsset(assetName, packageName);
                }
                else
                {
                    ConfirmPanel.ShowModal("CONFIRM_SAVEOVERRIDE", delegate (UIComponent comp, int ret)
                    {
                        if(ret == 1)
                        {
                            SaveAsset(assetName, packageName);
                        }
                    });
                }
            };
            m_saveButton.relativePosition = new Vector3(WIDTH / 2 - m_saveButton.width - 50, HEIGHT - m_saveButton.height - 10);

            // Cancel button
            m_cancelButton = UIUtils.CreateButton(this);
            m_cancelButton.text = "Cancel";
            m_cancelButton.textScale = 1.3f;
            m_cancelButton.width = 153f;
            m_cancelButton.height = 47f;
            m_cancelButton.eventClicked += (c, b) =>
            {
                m_info = null;
                isVisible = false;
            };
            m_cancelButton.relativePosition = new Vector3(WIDTH / 2 + 50, HEIGHT - m_cancelButton.height - 10);
        }

        public void ShowForAsset(VehicleInfo info)
        {
            if(isVisible)
            {
                return;
            }

            m_info = info;
            isVisible = true;
        }

        private void SaveAsset(string assetName, string packageName)
        {
            if(m_info == null || string.IsNullOrEmpty(assetName) || string.IsNullOrEmpty(packageName))
                return;

            Debug.Log("Starting save for asset " + assetName + " in package " + packageName);

            Package package = new Package(packageName);

            // Create lead vehicle prefab object
            VehicleInfo leadInfo = Util.InstantiateVehicleCopy(m_info);
            leadInfo.name = assetName;

            string[] steamTags = leadInfo.GetSteamTags();

            // Set up trailers
            if(m_info.m_trailers != null && m_info.m_trailers.Length > 0)
            {
                // Keep track of added trailer prefabs to prevent duplicates
                Dictionary<string, VehicleInfo> addedTrailers = new Dictionary<string, VehicleInfo>();

                for(int i = 0; i < m_info.m_trailers.Length; i++)
                {
                    VehicleInfo trailerInfo;
                    if(!addedTrailers.TryGetValue(m_info.m_trailers[i].m_info.name, out trailerInfo))
                    {
                        Debug.Log("Trailer " + m_info.m_trailers[i].m_info.name + " not yet in package " + packageName);

                        // Trailer not yet added to package
                        trailerInfo = Util.InstantiateVehicleCopy(m_info.m_trailers[i].m_info);
                        // TODO: Customizable trailer naming conventions
                        trailerInfo.name = "Trailer" + addedTrailers.Count;

                        Debug.Log("Renaming copy of trailer " + m_info.m_trailers[i].m_info.name  + " to " + trailerInfo.name + " in package " + packageName);

                        // Needed because of the 'set engine' feature
                        trailerInfo.m_trailers = null;

                        // Add stuff to package
                        Package.Asset trailerAsset = package.AddAsset(trailerInfo.name, trailerInfo.gameObject);

                        package.AddAsset(packageName + "_trailer" + addedTrailers.Count, new CustomAssetMetaData
                        {
                            assetRef = trailerAsset,
                            name = trailerInfo.name,
                            guid = Guid.NewGuid().ToString(),
                            steamTags = steamTags,
                            type = CustomAssetMetaData.Type.Trailer,
                            timeStamp = DateTime.Now,
                            dlcMask = AssetImporterAssetTemplate.GetAssetDLCMask(trailerInfo)
                        }, UserAssetType.CustomAssetMetaData);

                        // Fuck the locale

                        // Update dictonary
                        addedTrailers.Add(m_info.m_trailers[i].m_info.name, trailerInfo);

                        Debug.Log("Finished adding trailer " + trailerInfo.name + " to package " + packageName);
                    }

                    leadInfo.m_trailers[i].m_info = trailerInfo;
                }
            }

            // Add lead vehicle to package
            Package.Asset leadAsset = package.AddAsset(assetName + "_Data", leadInfo.gameObject);

            package.AddAsset(packageName, new CustomAssetMetaData
            {
                // Name of asset
                name = assetName,
                // Time created?
                timeStamp = DateTime.Now,
                // Reference to Asset (VehicleInfo GameObject) added to the package earlier
                assetRef = leadAsset,
                // Steam tags
                steamTags = steamTags,
                // Asset GUID
                guid = Guid.NewGuid().ToString(),
                // Type of this asset
                type = CustomAssetMetaData.Type.Vehicle,
                // DLCs required for this asset
                dlcMask = AssetImporterAssetTemplate.GetAssetDLCMask(leadInfo)
            }, UserAssetType.CustomAssetMetaData, false);

            // Set main asset to lead vehicle
            package.packageMainAsset = packageName;

            // Create and add locale
            Locale locale = new Locale();
            locale.AddLocalizedString(new Locale.Key
            {
                m_Identifier = "VEHICLE_TITLE",
                m_Key = assetName + "_Data"
            }, assetName);
            package.AddAsset(assetName + "_Locale", locale, false);

            // Save package to file
            package.Save(GetSavePathName(packageName));

            Debug.Log("Finished save for asset " + assetName + " in package " + packageName);
        }

        private string GetSavePathName(string saveName)
        {
            string path = PathUtils.AddExtension(PathEscaper.Escape(saveName), PackageManager.packageExtension);
            return Path.Combine(DataLocation.assetsPath, path);
        }
    }
}
