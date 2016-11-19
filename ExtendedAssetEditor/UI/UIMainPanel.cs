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
    public class UIMainPanel : UIPanel
    {
        private static UIMainPanel main;

        public const int WIDTH = 300;
        public const int HEIGHT = 400;

        private UIDropDown m_vehicleDropdown;
        private UIPanel m_lightPanel;
        private UIDropDown m_lightDropdown;
        private UICheckBox m_passengerLightCheckbox;
        private UIButton m_lightAddButton;
        private UIButton m_lightRemoveButton;
        /*private UITextField m_lightPosX;
        private UITextField m_lightPosY;
        private UITextField m_lightPosZ;*/


        private UIFloatField m_lightPosXField;
        private UIFloatField m_lightPosYField;
        private UIFloatField m_lightPosZField;


        private UICheckBox m_invertCheckbox;
        private UIButton m_engineButton;

        private UIButton m_saveButton;
        private UIButton m_loadButton;

        private VehicleInfo m_mainVehicleInfo;
        private VehicleInfo m_selectedVehicleInfo;

        private UIDisplayOptions m_displayOptionsPanel;
        private UIDoorTool m_doorTool;
        private UISavePanel m_savePanel;

        private bool m_checkingEvents;

        private VehicleInfo m_engineAsTrailer;

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

            CreateComponents();
            UpdateVehicleInfo();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if(m_displayOptionsPanel != null)
            {
                GameObject.Destroy(m_displayOptionsPanel);
            }
            if(m_doorTool != null)
            {
                GameObject.Destroy(m_doorTool);
            }
            if(m_savePanel != null)
            {
                GameObject.Destroy(m_savePanel);
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

            // Checkbox invert
            m_invertCheckbox = (UICheckBox)uiHelper.AddCheckbox("Is inverted", false, (b) => {
                if(!m_checkingEvents) { return; }

                if(m_vehicleDropdown.items.Length > 1)
                {
                    m_mainVehicleInfo.m_trailers[m_vehicleDropdown.selectedIndex - 1].m_invertProbability = b ? 100 : 0;
                    m_doorTool.UpdateMarkerVisibility();
                }
            });
            m_invertCheckbox.width = WIDTH - 20;
            m_invertCheckbox.relativePosition = new Vector3(10, headerHeight + 50);

            // Button engine
            m_engineButton = UIUtils.CreateButton(this);
            m_engineButton.eventClicked += (c, b) => {
                if(!m_checkingEvents) { return; }

                if(m_vehicleDropdown.items.Length > 1)
                {
                    m_mainVehicleInfo.m_trailers[m_vehicleDropdown.selectedIndex - 1].m_info = GetEngineAsTrailerUnchecked();
                    //m_trailerDropwdown.items[m_trailerDropwdown.selectedIndex] = m_vehicleInfo.name;
                }
            };
            m_engineButton.text = "Set engine";
            m_engineButton.relativePosition = new Vector3(10, headerHeight + 80);

            // Light panel
            m_lightPanel = AddUIComponent<UIPanel>();
            m_lightPanel.relativePosition = new Vector3(0, headerHeight + 120);
            m_lightPanel.size = new Vector2(WIDTH, HEIGHT - m_lightPanel.relativePosition.y - 50);
            UIHelper uiLightsHelper = new UIHelper(m_lightPanel);

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
            /*label = m_lightPanel.AddUIComponent<UILabel>();
            label.text = "Pos X:";
            label.relativePosition = new Vector3(10, 112);
            m_lightPosX = UIUtils.CreateTextField(m_lightPanel);
            m_lightPosX.relativePosition = new Vector3(70, 110);
            m_lightPosX.tooltip = "X position of the selected light";
            m_lightPosX.eventTextChanged += (c, s) => {
                if(!m_checkingEvents) { return; }

                if(m_selectedVehicleInfo != null && m_selectedVehicleInfo.m_lightPositions?.Length > 0)
                {
                    FloatFieldHandler(m_lightPosX, s, ref m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].x);
                }
            };*/

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
            m_lightPosXField.buttonDown.eventClicked -= (c, b) => {
                if(!m_checkingEvents) { return; }

                m_lightPosXField.SetValue(m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].x - 0.1f);
            };

            // Light pos y
            /*label = m_lightPanel.AddUIComponent<UILabel>();
            label.text = "Pos Y:";
            label.relativePosition = new Vector3(10, 142);
            m_lightPosY = UIUtils.CreateTextField(m_lightPanel);
            m_lightPosY.relativePosition = new Vector3(70, 140);
            m_lightPosY.tooltip = "Y position of the selected light";
            m_lightPosY.eventTextChanged += (c, s) => {
                if(!m_checkingEvents) { return; }

                if(m_selectedVehicleInfo != null && m_selectedVehicleInfo.m_lightPositions?.Length > 0)
                {
                    FloatFieldHandler(m_lightPosY, s, ref m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].y);
                }
            };*/
            m_lightPosYField = UIFloatField.CreateField("Pos X:", m_lightPanel);
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
            m_lightPosYField.buttonDown.eventClicked -= (c, b) => {
                if(!m_checkingEvents) { return; }

                m_lightPosYField.SetValue(m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].y - 0.1f);
            };

            // Light pos z
            /*label = m_lightPanel.AddUIComponent<UILabel>();
            label.text = "Pos Z:";
            label.relativePosition = new Vector3(10, 172);
            m_lightPosZ = UIUtils.CreateTextField(m_lightPanel);
            m_lightPosZ.relativePosition = new Vector3(70, 170);
            m_lightPosZ.tooltip = "Z position of the selected light";
            m_lightPosZ.eventTextChanged += (c, s) => {
                if(!m_checkingEvents) { return; }

                if(m_selectedVehicleInfo != null && m_selectedVehicleInfo.m_lightPositions?.Length > 0)
                {
                    FloatFieldHandler(m_lightPosZ, s, ref m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].z);
                }
            };*/
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
            m_lightPosZField.buttonDown.eventClicked -= (c, b) => {
                if(!m_checkingEvents) { return; }

                m_lightPosZField.SetValue(m_selectedVehicleInfo.m_lightPositions[m_lightDropdown.selectedIndex].z - 0.1f);
            };

#if PRERELEASE
            // Save button
            m_saveButton = UIUtils.CreateButton(this);
            m_saveButton.text = "Save Asset";
            m_saveButton.width = (WIDTH - 30) / 2;
            m_saveButton.relativePosition = new Vector3(10, m_lightPanel.relativePosition.y + m_lightPanel.height + 10);
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
            m_loadButton.relativePosition = new Vector3(20 + m_saveButton.width, m_lightPanel.relativePosition.y + m_lightPanel.height + 10);
#endif
            m_checkingEvents = true;
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
                    m_invertCheckbox.isVisible = true;
                    m_engineButton.isVisible = true;
                    m_selectedVehicleInfo = m_mainVehicleInfo.m_trailers[listIndex - 1].m_info;
                    m_invertCheckbox.isChecked = m_mainVehicleInfo.m_trailers[listIndex - 1].m_invertProbability == 100 ? true : false;
                    if(main == this)
                    {
                        eventSelectedUpdated?.Invoke(m_mainVehicleInfo, listIndex - 1);
                    }
                }
                else
                {
                    m_selectedVehicleInfo = m_mainVehicleInfo;
                    m_invertCheckbox.isVisible = false;
                    m_engineButton.isVisible = false;
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

                m_checkingEvents = true;

                UpdateLightsPanel();
            }
        }

        /// <summary>
        /// Adds a light to the selected vehicle, as long as it doesn't have the maximum number of them already.
        /// </summary>
        private void AddLight()
        {
            if(m_selectedVehicleInfo != null)
            {
                int newIndex = m_lightDropdown.items.Length;
                string lightEffect = Util.LightIndexToEffectName(m_selectedVehicleInfo, newIndex);
                if(lightEffect != null)
                {
                    m_selectedVehicleInfo.m_lightPositions = Util.LengthenArray(m_selectedVehicleInfo.m_lightPositions, Vector3.zero);
                    Util.AddEffect(m_selectedVehicleInfo, lightEffect);
                    UpdateLightsPanel();
                }
            }
        }

        /// <summary>
        /// Removes the light with the highest index from the vehicle
        /// </summary>
        private void RemoveLight()
        {
            if(m_selectedVehicleInfo != null && m_selectedVehicleInfo.m_lightPositions != null)
            {
                int index = m_selectedVehicleInfo.m_lightPositions.Length - 1;
                string lightEffect = Util.LightIndexToEffectName(m_selectedVehicleInfo, index);
                if(lightEffect != null)
                {
                    m_selectedVehicleInfo.m_lightPositions = Util.ShortenArray(m_selectedVehicleInfo.m_lightPositions);
                    Util.RemoveEffect(m_selectedVehicleInfo, lightEffect);
                    UpdateLightsPanel();
                }
            }
        }

        private VehicleInfo GetEngineAsTrailerUnchecked()
        {
            if(m_engineAsTrailer == null)
            {
                m_engineAsTrailer = Util.InstantiateVehicleCopy(m_mainVehicleInfo);
                m_engineAsTrailer.m_trailers = null;
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
                        trailerNames[i + 1] = m_mainVehicleInfo.m_trailers[i].m_info.name;
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
