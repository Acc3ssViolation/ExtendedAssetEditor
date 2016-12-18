using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;
using ColossalFramework;
using ICities;
using ExtendedAssetEditor.UI.Effects;

namespace ExtendedAssetEditor.UI
{
    public class UIMainPanel : UIPanel
    {
        private static UIMainPanel main;

        public const int WIDTH = 300;
        public const int HEIGHT = 480;

        private UIDropDown m_vehicleDropdown;
        private UIPanel m_lightPanel;
        private UIDropDown m_lightDropdown;
        private UICheckBox m_passengerLightCheckbox;
        private UIButton m_lightAddButton;
        private UIButton m_lightRemoveButton;
        private UIFloatField m_lightPosXField;
        private UIFloatField m_lightPosYField;
        private UIFloatField m_lightPosZField;
        private UIPanel m_trailerPanel;
        private UICheckBox m_invertCheckbox;
        private UIButton m_engineButton;
        private UIButton m_engineCopyButton;
        private UIButton m_insertTrailerButton;

        private Vector3 m_trailerPanelStart;
        private Vector3 m_lightPanelStart;

        private UIButton m_saveButton;
        private UIButton m_loadButton;
        private UIButton m_effectButton;

        private VehicleInfo m_mainVehicleInfo;
        private VehicleInfo m_selectedVehicleInfo;

        private UIDisplayOptions m_displayOptionsPanel;
        private UIDoorTool m_doorTool;
        private UISavePanel m_savePanel;
        private UIEffectPanel m_effectPanel;

        private bool m_checkingEvents;

        private VehicleInfo m_engineAsTrailer;
        private UIPanel m_selectRef;
        private VehicleInfo.VehicleTrailer[] m_trailersToInstantiate;

        public delegate void OnSelectedVehicleUpdated(VehicleInfo mainVehicle, int trailerIndex);
        public static event OnSelectedVehicleUpdated eventSelectedUpdated;

        public override void Start()
        {
            if(main == null)
            {
                main = this;
            }
            else
            {
                Debug.LogWarning("Multiple UIMainPanels for " + Mod.name);
            }

            base.Start();

            UIView view = UIView.GetAView();
            width = WIDTH;
            height = HEIGHT;
            backgroundSprite = "MenuPanel2";
            name = Mod.name + " Main Panel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            // Start in the top right corner
            relativePosition = new Vector3(view.fixedWidth - width - 10, 60);

            // Create display options panel
            m_displayOptionsPanel = new GameObject().AddComponent<UIDisplayOptions>();
            m_displayOptionsPanel.transform.SetParent(transform.parent);

            // Create doors panel
            m_doorTool = new GameObject().AddComponent<UIDoorTool>();
            m_doorTool.transform.SetParent(transform.parent);

            // Create save panel
            m_savePanel = new GameObject().AddComponent<UISavePanel>();
            m_savePanel.transform.SetParent(transform.parent);

            // Create effect panel
            m_effectPanel = new GameObject().AddComponent<UIEffectPanel>();
            m_effectPanel.transform.SetParent(transform.parent);

            // Events
            PrefabWatcher.instance.prefabBecameVehicle += () =>
            {
                Debug.Log("Prefab became vehicle");
                isVisible = true;
                UpdateVehicleInfo();
            };
            PrefabWatcher.instance.prefabWasVehicle += () =>
            {
                Debug.Log("Prefab was vehicle");
                isVisible = false;
            };
            PrefabWatcher.instance.trailersChanged += (string[] names) =>
            {
                Debug.Log("Trailers changed");
                UpdateVehicleInfo();
            };
            PrefabWatcher.instance.prefabChanged += () =>
            {
                Debug.Log("Prefab changed");
                UpdateVehicleInfo();
            };

            m_selectRef = GameObject.Find("SelectReference").GetComponent<UIPanel>();
            m_selectRef.isVisible = false;

            CreateComponents();
            UpdateVehicleInfo();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if(m_displayOptionsPanel != null)
            {
                GameObject.Destroy(m_displayOptionsPanel.gameObject);
            }
            if(m_doorTool != null)
            {
                GameObject.Destroy(m_doorTool.gameObject);
            }
            if(m_savePanel != null)
            {
                GameObject.Destroy(m_savePanel.gameObject);
            }
            if(m_effectPanel != null)
            {
                GameObject.Destroy(m_effectPanel.gameObject);
            }
        }

        private void CreateComponents()
        {
            int headerHeight = 40;
            UIHelperBase uiHelper = new UIHelper(this);

            // Label
            UILabel label = AddUIComponent<UILabel>();
            label.text = "Vehicles";
            label.relativePosition = new Vector3(WIDTH / 2 - label.width / 2, 10);

            // Drag handle
            UIDragHandle handle = AddUIComponent<UIDragHandle>();
            handle.target = this;
            handle.constrainToScreen = true;
            handle.width = WIDTH;
            handle.height = headerHeight;
            handle.relativePosition = Vector3.zero;

            // Dropdown
            m_vehicleDropdown = UIUtils.CreateDropDown(this);
            m_vehicleDropdown.width = WIDTH - 20;
            m_vehicleDropdown.relativePosition = new Vector3(10, headerHeight + 10);
            m_vehicleDropdown.eventSelectedIndexChanged += DropdownSelctionChanged;

            // Trailer panel
            m_trailerPanel = AddUIComponent<UIPanel>();
            m_trailerPanel.size = Vector2.zero;
            m_trailerPanel.relativePosition = new Vector3(0, headerHeight + 50);
            UIHelper uiTrailerHelper = new UIHelper(m_trailerPanel);
            m_trailerPanelStart = m_trailerPanel.relativePosition;

            // Checkbox invert
            m_invertCheckbox = (UICheckBox)uiTrailerHelper.AddCheckbox("Is inverted", false, (b) => {
                if(!m_checkingEvents) { return; }

                if(m_vehicleDropdown.items.Length > 1)
                {
                    m_mainVehicleInfo.m_trailers[m_vehicleDropdown.selectedIndex - 1].m_invertProbability = b ? 100 : 0;
                    m_doorTool.UpdateMarkerVisibility();
                }
            });
            m_invertCheckbox.width = WIDTH - 20;
            m_invertCheckbox.relativePosition = new Vector3(10, 0);

            // Button engine
            m_engineButton = UIUtils.CreateButton(m_trailerPanel);
            m_engineButton.eventClicked += (c, b) => {
                if(!m_checkingEvents) { return; }

                if(m_vehicleDropdown.items.Length > 1)
                {
                    m_mainVehicleInfo.m_trailers[m_vehicleDropdown.selectedIndex - 1].m_info = m_mainVehicleInfo;
                }
            };
            m_engineButton.text = "Set engine";
            m_engineButton.tooltip = "Sets this trailer to the same vehicle as the engine. Recommened to be used for the last trailer only.";
            m_engineButton.relativePosition = new Vector3(10, 30);

            // Button engine (copy)
            m_engineCopyButton = UIUtils.CreateButton(m_trailerPanel);
            m_engineCopyButton.eventClicked += (c, b) => {
                if(!m_checkingEvents) { return; }

                if(m_vehicleDropdown.items.Length > 1)
                {
                    m_mainVehicleInfo.m_trailers[m_vehicleDropdown.selectedIndex - 1].m_info = GetEngineAsTrailerUnchecked();
                }
            };
            m_engineCopyButton.width = 140;
            m_engineCopyButton.text = "Set engine (copy)";
            m_engineCopyButton.tooltip = "Sets this trailer to a copy of the engine. This means that you can edit this trailer's settings without it affecting the actual engine.";
            m_engineCopyButton.relativePosition = new Vector3(WIDTH - 10 - m_engineCopyButton.width, 30);

            // Button insert trailer
            m_insertTrailerButton = UIUtils.CreateButton(m_trailerPanel);
            m_insertTrailerButton.eventClicked += (c, b) => {
                if(!m_checkingEvents) { return; }

                InsertTrailer(m_selectedVehicleInfo, m_vehicleDropdown.selectedIndex);
            };
            m_insertTrailerButton.text = "Duplicate";
            m_insertTrailerButton.tooltip = "Insert another trailer of this type after the currently selected trailer.";
            m_insertTrailerButton.relativePosition = new Vector3(10, 70);

            // Light panel
            m_lightPanel = AddUIComponent<UIPanel>();
            m_lightPanel.relativePosition = new Vector3(0, headerHeight + 160);
            m_lightPanel.size = Vector2.zero;
            UIHelper uiLightsHelper = new UIHelper(m_lightPanel);
            m_lightPanelStart = m_lightPanel.relativePosition;

            // Checkbox passenger light
            m_passengerLightCheckbox = (UICheckBox)uiLightsHelper.AddCheckbox("Has passenger light", false, (b) => {
                if(!m_checkingEvents) { return; }

                if(m_selectedVehicleInfo != null)
                {
                    int index = Util.GetEffectIndex(m_selectedVehicleInfo, "Train Light Passenger");
                    if(b && index < 0)
                    {
                        // Add effect
                        Util.AddEffect(m_selectedVehicleInfo, "Train Light Passenger");
                    }
                    else if(!b && index >= 0)
                    {
                        // Remove effect
                        Util.RemoveEffect(m_selectedVehicleInfo, index);
                    }
                }
            });
            m_passengerLightCheckbox.width = WIDTH - 20;
            m_passengerLightCheckbox.relativePosition = new Vector3(10, 0);

            // Lights dropdown
            m_lightDropdown = UIUtils.CreateDropDown(m_lightPanel);
            m_lightDropdown.width = WIDTH - 20;
            m_lightDropdown.relativePosition = new Vector3(10, 30);
            m_lightDropdown.eventSelectedIndexChanged += LightSelectionChanged;

            // Light add button
            m_lightAddButton = UIUtils.CreateButton(m_lightPanel);
            m_lightAddButton.text = "Add";
            m_lightAddButton.tooltip = "Adds another light to the vehicle, if possible. Recommened to only use lights on front and/or rear vehicle(s).";
            m_lightAddButton.relativePosition = new Vector3(10, 70);
            m_lightAddButton.eventClicked += (c, b) => {
                AddLight();
            };

            // Light remove button
            m_lightRemoveButton = UIUtils.CreateButton(m_lightPanel);
            m_lightRemoveButton.text = "Remove";
            m_lightRemoveButton.tooltip = "Removes the light with the highest index from the vehicle.";
            m_lightRemoveButton.relativePosition = new Vector3(WIDTH - m_lightRemoveButton.width - 10, 70);
            m_lightRemoveButton.eventClicked += (c, b) => {
                RemoveLight();
            };

            // Light pos x
            m_lightPosXField = UIFloatField.CreateField("Pos X:", m_lightPanel);
            m_lightPosXField.panel.relativePosition = new Vector3(10, 110);
            m_lightPosXField.textField.eventTextChanged += (c, s) => {
                if(!m_checkingEvents) { return; }

                if(m_selectedVehicleInfo != null && m_selectedVehicleInfo.m_lightPositions?.Length > 0)
                {
                    UIFloatField.FloatFieldHandler(m_lightPosXField.textField, s, ref m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].x);
                }
            };
            m_lightPosXField.buttonUp.eventClicked += (c, b) => {
                if(!m_checkingEvents) { return; }

                m_lightPosXField.SetValue(m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].x + 0.1f);
            };
            m_lightPosXField.buttonDown.eventClicked += (c, b) => {
                if(!m_checkingEvents) { return; }

                m_lightPosXField.SetValue(m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].x - 0.1f);
            };

            // Light pos y
            m_lightPosYField = UIFloatField.CreateField("Pos Y:", m_lightPanel);
            m_lightPosYField.panel.relativePosition = new Vector3(10, 140);
            m_lightPosYField.textField.eventTextChanged += (c, s) => {
                if(!m_checkingEvents) { return; }

                if(m_selectedVehicleInfo != null && m_selectedVehicleInfo.m_lightPositions?.Length > 0)
                {
                    UIFloatField.FloatFieldHandler(m_lightPosYField.textField, s, ref m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].y);
                }
            };
            m_lightPosYField.buttonUp.eventClicked += (c, b) => {
                if(!m_checkingEvents) { return; }

                m_lightPosYField.SetValue(m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].y + 0.1f);
            };
            m_lightPosYField.buttonDown.eventClicked += (c, b) => {
                if(!m_checkingEvents) { return; }

                m_lightPosYField.SetValue(m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].y - 0.1f);
            };

            // Light pos z
            m_lightPosZField = UIFloatField.CreateField("Pos Z:", m_lightPanel);
            m_lightPosZField.panel.relativePosition = new Vector3(10, 170);
            m_lightPosZField.textField.eventTextChanged += (c, s) => {
                if(!m_checkingEvents) { return; }

                if(m_selectedVehicleInfo != null && m_selectedVehicleInfo.m_lightPositions?.Length > 0)
                {
                    UIFloatField.FloatFieldHandler(m_lightPosZField.textField, s, ref m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].z);
                }
            };
            m_lightPosZField.buttonUp.eventClicked += (c, b) => {
                if(!m_checkingEvents) { return; }

                m_lightPosZField.SetValue(m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].z + 0.1f);
            };
            m_lightPosZField.buttonDown.eventClicked += (c, b) => {
                if(!m_checkingEvents) { return; }

                m_lightPosZField.SetValue(m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].z - 0.1f);
            };

            // Effect panel button
            m_effectButton = m_saveButton = UIUtils.CreateButton(this);
            m_effectButton.text = "Effect Editor";
            m_effectButton.width = (WIDTH - 20);
            m_effectButton.relativePosition = new Vector3(10, m_lightPanel.relativePosition.y + 200);
            m_effectButton.eventClicked += (c, b) =>
            {
                if(m_mainVehicleInfo != null && m_effectPanel != null)
                    m_effectPanel.Show();
            };

            // Save button
            m_saveButton = UIUtils.CreateButton(this);
            m_saveButton.text = "Save Asset";
            m_saveButton.width = (WIDTH - 30) / 2;
            m_saveButton.relativePosition = new Vector3(10, m_effectButton.relativePosition.y + m_effectButton.height + 10);
            m_saveButton.eventClicked += (c, b) => {
                if(m_mainVehicleInfo != null && m_savePanel != null)
                {
                    m_savePanel.ShowForAsset(m_mainVehicleInfo);
                }
            };

            // Load button
            m_loadButton = UIUtils.CreateButton(this);
            m_loadButton.text = "Load Asset";
            m_loadButton.width = (WIDTH - 30) / 2;
            m_loadButton.relativePosition = new Vector3(20 + m_saveButton.width, m_effectButton.relativePosition.y + m_effectButton.height + 10);
            m_loadButton.eventClicked += (c, b) =>
            {
                if(m_selectRef.isVisible)
                {
                    return;
                }
                AssetImporterAssetTemplate assetImporterAssetTemplate = m_selectRef.GetComponent<AssetImporterAssetTemplate>();
                assetImporterAssetTemplate.ReferenceCallback = new AssetImporterAssetTemplate.ReferenceCallbackDelegate(OnConfirmLoad);
                assetImporterAssetTemplate.Reset();
                assetImporterAssetTemplate.RefreshWithFilter(AssetImporterAssetTemplate.Filter.Vehicles);
                m_selectRef.isVisible = true;
            };

            m_checkingEvents = true;
        }

        public override void Update()
        {
            base.Update();

            if(m_trailersToInstantiate != null && m_mainVehicleInfo != null && m_mainVehicleInfo.m_trailers != null)
            {
                if(m_trailersToInstantiate.Length != m_mainVehicleInfo.m_trailers.Length)
                {
                    Debug.Log("Trailer array length mismatch!");
                    return;
                }

                Debug.Log("Restoring correct trailers for asset.");
                for(int index = 0; index < m_trailersToInstantiate.Length; index++)
                {
                    bool isInstantiated = false;
                    for(int i = 0; i < m_mainVehicleInfo.m_trailers.Length; i++)
                    {
                        if(m_trailersToInstantiate[index].m_info.name == m_mainVehicleInfo.m_trailers[i].m_info.name)
                        {
                            isInstantiated = true;
                            m_trailersToInstantiate[index].m_info = m_mainVehicleInfo.m_trailers[i].m_info;
                            Debug.Log("Trailer " + m_trailersToInstantiate[index].m_info.name + " already loaded");
                            break;
                        }
                    }
                    if(!isInstantiated)
                    {
                        Debug.Log("Trailer " + m_trailersToInstantiate[index].m_info.name + " not yet loaded, instantiating...");
                        m_trailersToInstantiate[index].m_info = Util.InstantiateVehicleCopy(m_trailersToInstantiate[index].m_info);
                    }
                    m_mainVehicleInfo.m_trailers[index] = m_trailersToInstantiate[index];

                    // Do check for missing effects
                    CheckMissingEffects(m_mainVehicleInfo.m_trailers[index].m_info);
                }
                Util.CopyTrailerColorFromMain();
                Debug.Log("Instantiated all trailers!");
                m_trailersToInstantiate = null;
            }
        }

        private void OnConfirmLoad(PrefabInfo reference)
        {
            if(reference != null)
            {
                reference = Util.InstantiateVehicleCopy(reference as VehicleInfo);
                if(reference != null)
                {
                    VehicleInfo vehicle = reference as VehicleInfo;
                    if(vehicle.m_trailers != null)
                    {
                        string str = "";
                        foreach(var t in vehicle.m_trailers)
                        {
                            str += t.m_info != null ? t.m_info.name : "NULL";
                            str += "\r\n";
                        }
                        Debug.Log(str);

                        m_trailersToInstantiate = new VehicleInfo.VehicleTrailer[vehicle.m_trailers.Length];
                        vehicle.m_trailers.CopyTo(m_trailersToInstantiate, 0);
                    }
                    
                    CheckMissingEffects((VehicleInfo)reference);
                    ToolsModifierControl.toolController.m_editPrefabInfo = reference;
                }
            }
            m_selectRef.isVisible = false;
        }

        private void CheckMissingEffects(VehicleInfo vehicle)
        {
            if(vehicle != null && vehicle.m_effects != null)
            {
                for(int i = 0; i < vehicle.m_effects.Length; i++)
                {
                    if(vehicle.m_effects[i].m_effect == null)
                    {
                        ConfirmPanel.ShowModal(Mod.name, "Detected missing effects on vehicle " + vehicle.name + ", do you want EAE to remove these for you? Click cancel to ignore this warning.", delegate (UIComponent comp, int ret)
                        {
                            if(ret == 1)
                            {
                                List<VehicleInfo.Effect> list = new List<VehicleInfo.Effect>();
                                list.AddRange(vehicle.m_effects);
                                list.RemoveAll(x => x.m_effect == null);
                                vehicle.m_effects = list.ToArray();
                            }
                        });
                        break;
                    }
                }
            }
        }

        private void InsertTrailer(VehicleInfo trailerInfo, int insertionIndex)
        {
            if(m_mainVehicleInfo.m_trailers == null)
            {
                if(insertionIndex == 0)
                {
                    m_mainVehicleInfo.m_trailers = new VehicleInfo.VehicleTrailer[0];
                }
                else
                {
                    return;
                }
            }

            List<VehicleInfo.VehicleTrailer> trailerList = new List<VehicleInfo.VehicleTrailer>();
            trailerList.AddRange(m_mainVehicleInfo.m_trailers);
            trailerList.Insert(insertionIndex, new VehicleInfo.VehicleTrailer
            {
                m_info = trailerInfo,
                m_invertProbability = 0,
                m_probability = 100
            });
            m_mainVehicleInfo.m_trailers = trailerList.ToArray();
        }

        private void LightSelectionChanged(UIComponent component, int value)
        {
            if(!m_checkingEvents) { return; }

            // Update stuff
            m_checkingEvents = false;


            if(m_selectedVehicleInfo != null && m_selectedVehicleInfo.m_lightPositions?.Length > 0)
            {
                m_lightPosXField.SetValue(m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].x);
                m_lightPosYField.SetValue(m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].y);
                m_lightPosZField.SetValue(m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].z);
            }
            else
            {
                m_lightPosXField.SetValue(0);
                m_lightPosYField.SetValue(0);
                m_lightPosZField.SetValue(0);
            }

            m_checkingEvents = true;
        }

        private void DropdownSelctionChanged(UIComponent component, int listIndex)
        {
            if(!m_checkingEvents) { return; }

            if(m_mainVehicleInfo != null)
            {
                m_checkingEvents = false;

                if(m_mainVehicleInfo.m_trailers != null && listIndex >= 1 && listIndex <= m_mainVehicleInfo.m_trailers.Length)
                {
                    // Select a trailer
                    m_trailerPanel.isVisible = true;
                    m_trailerPanel.relativePosition = m_trailerPanelStart;
                    m_lightPanel.relativePosition = m_lightPanelStart;

                    m_selectedVehicleInfo = m_mainVehicleInfo.m_trailers[listIndex - 1].m_info;
                    m_invertCheckbox.isChecked = m_mainVehicleInfo.m_trailers[listIndex - 1].m_invertProbability == 100 ? true : false;
                    if(main == this)
                    {
                        eventSelectedUpdated?.Invoke(m_mainVehicleInfo, listIndex - 1);
                    }

                }
                else
                {

                    // Select main vehicle
                    m_selectedVehicleInfo = m_mainVehicleInfo;
                    m_trailerPanel.isVisible = false;
                    m_lightPanel.relativePosition = m_trailerPanelStart;

                    if(main == this)
                    {
                        eventSelectedUpdated?.Invoke(m_mainVehicleInfo, -1);
                    }

                }

                if(m_selectedVehicleInfo.m_vehicleAI is TrainAI)
                {
                    m_lightAddButton.isVisible = true;
                    m_lightRemoveButton.isVisible = true;
                }
                else
                {
                    m_lightAddButton.isVisible = false;
                    m_lightRemoveButton.isVisible = false;
                }

                if(listIndex >= 0 && listIndex < m_vehicleDropdown.items.Length)
                {
                    m_vehicleDropdown.tooltip = m_vehicleDropdown.items[listIndex];
                }

                m_checkingEvents = true;

                UpdateLightsPanel();
            }
        }

        /// <summary>
        /// Adds a light to the selected vehicle, also adds train lights automatically.
        /// </summary>
        private void AddLight()
        {
            if(m_selectedVehicleInfo != null)
            {
                int newIndex = m_lightDropdown.items.Length;
                string lightEffect = Util.LightIndexToEffectName(m_selectedVehicleInfo, newIndex);
                m_selectedVehicleInfo.m_lightPositions = Util.LengthenArray(m_selectedVehicleInfo.m_lightPositions, Vector3.zero);
                if(lightEffect != null)
                {
                    Util.AddEffect(m_selectedVehicleInfo, lightEffect);
                }
                UpdateLightsPanel();
            }
        }

        /// <summary>
        /// Removes the light with the highest index from the vehicle, also removes train lights automatically.
        /// </summary>
        private void RemoveLight()
        {
            if(m_selectedVehicleInfo != null && m_selectedVehicleInfo.m_lightPositions != null)
            {
                int index = m_selectedVehicleInfo.m_lightPositions.Length - 1;
                m_selectedVehicleInfo.m_lightPositions = Util.ShortenArray(m_selectedVehicleInfo.m_lightPositions);
                string lightEffect = Util.LightIndexToEffectName(m_selectedVehicleInfo, index);
                if(lightEffect != null)
                {
                    Util.RemoveEffect(m_selectedVehicleInfo, lightEffect);
                }
                UpdateLightsPanel();
            }
        }

        private VehicleInfo GetEngineAsTrailerUnchecked()
        {
            if(m_engineAsTrailer == null)
            {
                m_engineAsTrailer = Util.InstantiateVehicleCopy(m_mainVehicleInfo);
                m_engineAsTrailer.m_trailers = null;
                m_engineAsTrailer.name += " (Copy)";
            }
            return m_engineAsTrailer;
        }

        private void UpdateVehicleInfo()
        {
            VehicleInfo prevMainVehicle = m_mainVehicleInfo;
            ToolController properties = Singleton<ToolManager>.instance.m_properties;
            if(properties != null)
            {
                m_mainVehicleInfo = properties.m_editPrefabInfo as VehicleInfo;
            }
            else
            {
                m_mainVehicleInfo = null;
                m_selectedVehicleInfo = null;
            }

            if(prevMainVehicle != m_mainVehicleInfo && m_engineAsTrailer != null)
            {
                GameObject.Destroy(m_engineAsTrailer.gameObject);
            }

            UpdateVehicleList();
        }

        private void UpdateVehicleList()
        {
            if(m_vehicleDropdown != null && m_mainVehicleInfo != null)
            {
                m_checkingEvents = false;

                int selectedIndex = m_vehicleDropdown.selectedIndex;
                int count = m_mainVehicleInfo.m_trailers != null ? m_mainVehicleInfo.m_trailers.Length : 0;
                count++;
                string[] trailerNames = new string[count];
                trailerNames[0] = m_mainVehicleInfo.name;
                if(count > 1)
                {
                    for(int i = 0; i < m_mainVehicleInfo.m_trailers.Length; i++)
                    {
                        trailerNames[i + 1] = (i + 1) + " - " + m_mainVehicleInfo.m_trailers[i].m_info.name;
                    }
                }
                m_vehicleDropdown.items = trailerNames;
                selectedIndex = Mathf.FloorToInt(Mathf.Clamp(selectedIndex, 0, count));

                m_checkingEvents = true;

                m_vehicleDropdown.selectedIndex = selectedIndex;
                // Force dropdown update
                DropdownSelctionChanged(null, selectedIndex);
            }
        }

        private void UpdateLightsPanel()
        {
            if(m_lightDropdown != null && m_selectedVehicleInfo != null)
            {
                m_checkingEvents = false;

                // Set passenger light button
                m_passengerLightCheckbox.isChecked = (Util.GetEffectIndex(m_selectedVehicleInfo, "Train Light Passenger") >= 0);

                // Dropdown update
                int selectedIndex = m_lightDropdown.selectedIndex;
                int count = m_selectedVehicleInfo.m_lightPositions != null ? m_selectedVehicleInfo.m_lightPositions.Length : 0;

                string[] lightNames = new string[count];
                for(int i = 0; i < count; i++)
                {
                    lightNames[i] = Util.LightIndexToEffectName(m_selectedVehicleInfo, i) ?? "Light " + i;
                }

                m_lightDropdown.items = lightNames;
                if(count != 0)
                {
                    selectedIndex = Mathf.FloorToInt(Mathf.Clamp(selectedIndex, 0, count));
                }
                else
                {
                    selectedIndex = -1;
                }

                m_checkingEvents = true;

                m_lightDropdown.selectedIndex = selectedIndex;
                // Force light update
                LightSelectionChanged(null, selectedIndex);
            }
        }
    }
}
