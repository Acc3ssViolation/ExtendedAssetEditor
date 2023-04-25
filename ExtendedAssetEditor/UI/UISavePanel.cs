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
using ColossalFramework.Importers;
using ColossalFramework.Threading;
using ColossalFramework;
using ColossalFramework.Plugins;
using System.Collections;
using UnityEngine.Networking.Types;

namespace ExtendedAssetEditor.UI
{
    public class UISavePanel : UIPanel
    {
        public const int WIDTH = 910;
        public const int HEIGHT = 640;

        private UIButton m_saveButton;
        private UIButton m_cancelButton;
        private UIDropDown m_namingDropdown;

        // TESTING
        private UITextField m_packageField;
        private UITextField m_nameField;
        private UIPanel m_snapshotContainer;
        private UITextureSprite m_snapshotSprite;
        private UILabel m_snapshotLabel;
        private UIButton m_snapshotNext;
        private UIButton m_snapshotPrev;
        private int m_currentSnapshot;
        private List<string> m_snapshotPaths = new List<string>();
        private static readonly string[] m_extensions = Image.GetExtensions(Image.SupportedFileFormat.PNG);
        private FileSystemReporter[] m_fSReporter = new FileSystemReporter[m_extensions.Length];
        private UICheckBox m_generateThumbnails;
        private UICheckBox m_cleanNames;
        //

        private VehicleInfo m_info;

        public override void Start()
        {
            base.Start();

            UIView view = UIView.GetAView();
            width = WIDTH;
            height = HEIGHT;
            backgroundSprite = "MenuPanel2";
            name = Mod.ModName + " Save Panel";
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

            // Thumbnail generation checkbox
            m_generateThumbnails = UIUtils.CreateCheckBox(this);
            m_generateThumbnails.text = "Regenerate thumbnails";
            m_generateThumbnails.relativePosition = new Vector3(10, headerHeight + 70);
            m_generateThumbnails.tooltip = "Generate new thumbnails when saving the asset instead of using the existing ones";

            // Name cleaning
            m_cleanNames = UIUtils.CreateCheckBox(this);
            m_cleanNames.text = "Clean names (Experimental)";
            m_cleanNames.relativePosition = new Vector3(10, headerHeight + 100);
            m_cleanNames.tooltip = "Clean up names of Unity objects in the asset";

            // Naming dropdown menu
            label = AddUIComponent<UILabel>();
            label.text = "Trailer naming:";
            label.relativePosition = new Vector3(m_nameField.relativePosition.x + m_nameField.width + 20, headerHeight + 10);
            m_namingDropdown = UIUtils.CreateDropDown(this);
            m_namingDropdown.text = "Trailer naming convention";
            m_namingDropdown.AddItem("Package name (TrailerPackageName0)");     //0
            m_namingDropdown.AddItem("Default (Trailer0)");                     //1
            m_namingDropdown.selectedIndex = 0;
            m_namingDropdown.width = 300;
            m_namingDropdown.relativePosition = new Vector3(m_nameField.relativePosition.x + m_nameField.width + 20, headerHeight + 35);


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

            // Snapshot container
            GameObject go = GameObject.Find("SnapshotContainer");
            if(go != null)
            {
                m_snapshotContainer = GameObject.Instantiate<UIPanel>(go.GetComponent<UIPanel>());
                m_snapshotContainer.transform.SetParent(transform);
                m_snapshotSprite = m_snapshotContainer.Find<UITextureSprite>("SnapShot");
                m_snapshotLabel = m_snapshotContainer.Find<UILabel>("CurrentSnapShot");
                m_snapshotPrev = m_snapshotContainer.Find<UIButton>("Previous");
                m_snapshotPrev.eventClicked += (c, b) =>
                {
                    OnPreviousSnapshot();
                };
                m_snapshotNext = m_snapshotContainer.Find<UIButton>("Next");
                m_snapshotNext.eventClicked += (c, b) =>
                {
                    OnNextSnapshot();
                };
            }
        }

        private void OnPreviousSnapshot()
        {
            m_currentSnapshot = (m_currentSnapshot - 1) % m_snapshotPaths.Count;
            if(m_currentSnapshot < 0)
            {
                m_currentSnapshot += m_snapshotPaths.Count;
            }
            RefreshSnapshot();
        }

        private void OnNextSnapshot()
        {
            m_currentSnapshot = (m_currentSnapshot + 1) % m_snapshotPaths.Count;
            RefreshSnapshot();
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if(isVisible)
            {
                if(Singleton<LoadingManager>.exists)
                {
                    Singleton<LoadingManager>.instance.autoSaveTimer.Pause();
                }
                m_snapshotContainer.relativePosition = new Vector3(WIDTH - m_snapshotContainer.width - 20, 100);
                SnapshotTool tool = ToolsModifierControl.GetTool<SnapshotTool>();
                if(tool != null)
                {
                    string snapShotPath = tool.snapShotPath;
                    if(!string.IsNullOrEmpty(snapShotPath))
                    {
                        for(int i = 0; i < m_extensions.Length; i++)
                        {
                            m_fSReporter[i] = new FileSystemReporter("*" + m_extensions[i], snapShotPath, new FileSystemReporter.ReporterEventHandler(Refresh));
                            if(m_fSReporter[i] != null)
                            {
                                m_fSReporter[i].Start();
                            }
                        }
                    }
                }
                Refresh();
            }
            else
            {
                if(Singleton<LoadingManager>.exists)
                {
                    Singleton<LoadingManager>.instance.autoSaveTimer.UnPause();
                }
                for(int j = 0; j < m_extensions.Length; j++)
                {
                    if(m_fSReporter[j] != null)
                    {
                        m_fSReporter[j].Stop();
                        m_fSReporter[j].Dispose();
                        m_fSReporter[j] = null;
                    }
                }
            }
        }

        private void FetchSnapshots()
        {
            string b = (m_snapshotPaths.Count <= 0) ? null : m_snapshotPaths[m_currentSnapshot];
            m_currentSnapshot = 0;
            m_snapshotPaths.Clear();
            SnapshotTool tool = ToolsModifierControl.GetTool<SnapshotTool>();
            if(tool != null)
            {
                string snapShotPath = tool.snapShotPath;
                if(snapShotPath != null)
                {
                    FileInfo[] fileInfo = SaveHelper.GetFileInfo(snapShotPath);
                    if(fileInfo != null)
                    {
                        FileInfo[] array = fileInfo;
                        for(int i = 0; i < array.Length; i++)
                        {
                            FileInfo fileInfo2 = array[i];
                            if(string.Compare(Path.GetExtension(fileInfo2.Name), ".png", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if(Path.GetFileName(fileInfo2.Name).Contains("snapshot"))
                                {
                                    m_snapshotPaths.Add(fileInfo2.FullName);
                                    if(fileInfo2.FullName == b)
                                    {
                                        m_currentSnapshot = m_snapshotPaths.Count - 1;
                                    }
                                }
                            }
                        }
                    }
                    UIComponent prevButton = Find("Previous");
                    bool isEnabled = m_snapshotPaths.Count > 1;
                    Find("Next").isEnabled = isEnabled;
                    prevButton.isEnabled = isEnabled;
                    RefreshSnapshot();
                }
            }
        }

        private void RefreshSnapshot()
        {
            if(m_snapshotSprite.texture != null)
            {
                Destroy(m_snapshotSprite.texture);
            }
            if(m_snapshotPaths.Count > 0)
            {
                Image image = new Image(m_snapshotPaths[m_currentSnapshot]);
                image.Resize(400, 224);
                m_snapshotSprite.texture = image.CreateTexture();
                m_snapshotLabel.text = m_currentSnapshot + 1 + " / " + m_snapshotPaths.Count;
            }
            else
            {
                m_snapshotSprite.texture = Instantiate(Resources.FindObjectsOfTypeAll<SaveAssetPanel>()[0].m_DefaultAssetPreviewTexture) as Texture2D;
                m_snapshotLabel.text = "Press CTRL+ALT+S to take snapshot";
            }
        }

        private void Refresh(object sender, ReporterEventArgs e)
        {
            ThreadHelper.dispatcher.Dispatch(delegate
            {
                FetchSnapshots();
            });
        }

        private void Refresh()
        {
            FetchSnapshots();
        }

        public void ShowForAsset(VehicleInfo info)
        {
            if(isVisible)
            {
                return;
            }

            // Start in the center of the screen
            UIView view = UIView.GetAView();
            relativePosition = new Vector3((view.fixedWidth - width) / 2, (view.fixedHeight - height) / 2);
            BringToFront();
            m_info = info;
            isVisible = true;
        }

        private void SaveAsset(string assetName, string packageName)
        {
            m_saveButton.isEnabled = false;
            StartCoroutine(SaveAssetCoroutine(assetName, packageName).Append(() => m_saveButton.isEnabled = true, this));
        }

        private IEnumerator SaveAssetCoroutine(string assetName, string packageName)
        {
            if (m_info == null || string.IsNullOrEmpty(assetName) || string.IsNullOrEmpty(packageName))
                yield break;

            Util.Log($"Starting save for asset '{assetName}' in package {packageName}");

            var package = new Package(packageName);

            // Create lead vehicle prefab object
            VehicleInfo leadInfo = Util.InstantiateVehicleCopy(m_info);
            leadInfo.name = assetName;

            string[] steamTags = leadInfo.GetSteamTags();

            // Generate thumnails for main asset
            if (m_generateThumbnails.isChecked)
            {
                yield return StartCoroutine(leadInfo.GenerateThumbnailsCoroutine());
            }

            // Set up trailers
            if (m_info.m_trailers != null && m_info.m_trailers.Length > 0)
            {
                // Keep track of added trailer prefabs to prevent duplicates
                Dictionary<string, VehicleInfo> addedTrailers = new Dictionary<string, VehicleInfo>();

                for(int i = 0; i < m_info.m_trailers.Length; i++)
                {
                    if (!addedTrailers.TryGetValue(m_info.m_trailers[i].m_info.name, out VehicleInfo trailerInfo))
                    {
                        Util.Log($"Trailer '{m_info.m_trailers[i].m_info.name}' not yet in package '{packageName}'");
                        trailerInfo = Util.InstantiateVehicleCopy(m_info.m_trailers[i].m_info);

                        // Generate thumbnails for trailer
                        if (m_generateThumbnails.isChecked)
                        {
                            yield return StartCoroutine(trailerInfo.GenerateThumbnailsCoroutine());
                        }

                        // Set placment mode to Procedural, this seems to be the only difference between engines (Automatic) and trailers (Procedural)
                        trailerInfo.m_placementStyle = ItemClass.Placement.Procedural;

                        // Include packagename in trailer, fixes Duplicate Prefab errors with multi .crp workshop uploads
                        if (m_namingDropdown.selectedIndex == 0)
                        {
                            // TrailerPackageName0
                            trailerInfo.name = "Trailer" + packageName + addedTrailers.Count;
                            Util.Log($"Renaming copy of trailer '{m_info.m_trailers[i].m_info.name}' to '{trailerInfo.name}' in package '{packageName}'");
                        }
                        else
                        {
                            // Default, Trailer0
                            trailerInfo.name = "Trailer" + addedTrailers.Count;
                        }

                        // TODO: Figure out why this was ever added
                        // Fix for 1.6
                        var postFix = " " + packageName;
                        try { trailerInfo.m_mesh.name += postFix; } catch (Exception e) { Debug.LogException(e); Debug.Log("me"); };
                        try { trailerInfo.m_lodMesh.name += postFix; } catch (Exception e) { Debug.LogException(e); Debug.Log("lme"); };
                        try { trailerInfo.m_material.name += postFix; } catch (Exception e) { Debug.LogException(e); Debug.Log("m"); };
                        try { trailerInfo.m_lodMaterial.name += postFix; } catch (Exception e) { Debug.LogException(e); Debug.Log("lm"); };

                        // Needed because of the 'set engine' feature.
                        trailerInfo.m_trailers = null;

                        // Add the trailer
                        if (m_cleanNames.isChecked)
                        {
                            Util.CleanUpNames(trailerInfo);
                        }
                        Package.Asset trailerAsset = package.AddAsset(trailerInfo.name, trailerInfo.gameObject);

                        AssetImporterAssetTemplate.GetAssetDLCMask(trailerInfo, out var expensionMask, out var modderMask);
                        package.AddAsset(packageName + "_trailer" + addedTrailers.Count, new CustomAssetMetaData
                        {
                            assetRef = trailerAsset,
                            name = trailerInfo.name,
                            guid = Guid.NewGuid().ToString(),
                            steamTags = steamTags,
                            type = CustomAssetMetaData.Type.Trailer,
                            timeStamp = DateTime.Now,
                            expansionMask = expensionMask,
                            modderPackMask = modderMask,
                            mods = EmbedModInfo(),
                        }, UserAssetType.CustomAssetMetaData);

                        // Create and add locale
                        Locale trailerLocale = new Locale();
                        trailerLocale.AddLocalizedString(new Locale.Key
                        {
                            m_Identifier = "VEHICLE_TITLE",
                            m_Key = trailerInfo.name
                        }, assetName + " Trailer " + addedTrailers.Count);
                        package.AddAsset(trailerInfo.name + "_Locale", trailerLocale, false);

                        // Update dictonary
                        addedTrailers.Add(m_info.m_trailers[i].m_info.name, trailerInfo);

                        Util.Log($"Finished adding trailer '{trailerInfo.name}' to package '{packageName}'");
                    }

                    // Update the lead info reference
                    leadInfo.m_trailers[i].m_info = trailerInfo;
                }
            }

            // Add lead vehicle to package
            if (m_cleanNames)
            {
                Util.CleanUpNames(leadInfo);
            }
            FixSubmeshInfoNames(leadInfo);
            PackVariationMasksInSubMeshNames(leadInfo);
            Package.Asset leadAsset = package.AddAsset(assetName + "_Data", leadInfo.gameObject);

            // Previews
            Package.Asset steamPreviewRef = null;
            Package.Asset imageRef = null;

            // Add snapshot image
            if(m_snapshotPaths.Count > 0)
            {
                Image image = new Image(m_snapshotPaths[m_currentSnapshot]);
                image.Resize(644, 360);
                steamPreviewRef = package.AddAsset(assetName + "_SteamPreview", image, false, Image.BufferFileFormat.PNG, false, false);
                image = new Image(m_snapshotPaths[m_currentSnapshot]);
                image.Resize(400, 224);
                imageRef = package.AddAsset(assetName + "_Snapshot", image, false, Image.BufferFileFormat.PNG, false, false);
            }

            // TODO: Should we instead OR this with the trailer assets as well?
            AssetImporterAssetTemplate.GetAssetDLCMask(leadInfo, out var leadExpensionMask, out var leadModderMask);
            package.AddAsset(packageName, new CustomAssetMetaData
            {
                // Name of asset
                name = assetName,
                // Time created?
                timeStamp = DateTime.Now,
                // Reference to Asset (VehicleInfo GameObject) added to the package earlier
                assetRef = leadAsset,
                // Snapshot
                imageRef = imageRef,
                // Steam Preview
                steamPreviewRef = steamPreviewRef,
                // Steam tags
                steamTags = steamTags,
                // Asset GUID
                guid = Guid.NewGuid().ToString(),
                // Type of this asset
                type = CustomAssetMetaData.Type.Vehicle,
                // DLCs required for this asset
                expansionMask = leadExpensionMask,
                modderPackMask = leadModderMask,
                // Mods active when making asset
                mods = EmbedModInfo()
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

            Util.Log($"Finished save for asset '{assetName}' in package '{packageName}'");

            m_info = null;
            isVisible = false;
        }

        /// <summary>
        /// Prevents duplicate submesh names from existing
        /// </summary>
        /// <param name="info"></param>
        private void FixSubmeshInfoNames(VehicleInfo info)
        {
            // TODO: Figure out why this was ever added
            var names = new Dictionary<string, int>();
            if(info.m_subMeshes != null)
            {
                foreach(var submesh in info.m_subMeshes)
                {
                    if(submesh.m_subInfo == null) { continue; }

                    if(names.ContainsKey(submesh.m_subInfo.name))
                    {
                        submesh.m_subInfo.name = submesh.m_subInfo.name + " " + names[submesh.m_subInfo.name]++;
                    }
                    else
                    {
                        names.Add(submesh.m_subInfo.name, 1);
                    }
                }
            }
        }

        private void PackVariationMasksInSubMeshNames(VehicleInfo info)
        {
            if(info.m_subMeshes != null)
            {
                foreach(var submesh in info.m_subMeshes)
                {
                    // ONLY write if the submesh variation mask is NOT 0. This leaves the option to use other mods that put info in the submesh mesh name (e.g. additive shader)
                    if(submesh.m_variationMask != 0)
                    {
                        if (submesh.m_subInfo != null && submesh.m_subInfo.m_mesh != null)
                        {
                            // Regular submesh, store it on the submesh mesh
                            submesh.m_subInfo.m_mesh.name = "TrailerVariation " + submesh.m_variationMask;
                        }
                        else if (submesh.m_subInfo == null && info.m_mesh != null)
                        {
                            // Info for main mesh, dump it on the main model mesh
                            info.m_mesh.name = "TrailerVariation " + submesh.m_variationMask;
                        }
                    }
                }
            }
        }

        private string GetSavePathName(string saveName)
        {
            string path = PathUtils.AddExtension(PathEscaper.Escape(saveName), PackageManager.packageExtension);
            return Path.Combine(DataLocation.assetsPath, path);
        }

        private ModInfo[] EmbedModInfo()
        {
            List<ModInfo> list = new List<ModInfo>();
            foreach(PluginManager.PluginInfo current in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                if(current.isEnabled)
                {
                    list.Add(new ModInfo
                    {
                        modName = current.name,
                        modWorkshopID = current.publishedFileID.AsUInt64
                    });
                }
            }
            return list.ToArray();
        }
    }
}
