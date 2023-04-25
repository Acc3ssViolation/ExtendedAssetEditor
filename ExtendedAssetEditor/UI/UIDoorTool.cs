using ColossalFramework.UI;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtendedAssetEditor.UI
{
    public class UIDoorTool : UIPanel
    {
        public const int WIDTH = 245;
        public const int HEIGHT = 220;

        private UIDropDown m_doorDropdown;
        private UIFloatField m_posFieldX;
        private UIFloatField m_posFieldY;
        private UIFloatField m_posFieldZ;
        private UIButton m_addButton;
        private UIButton m_removeButton;
        private bool m_checkEvents;

        private VehicleInfo m_mainInfo;
        private VehicleInfo m_selectedInfo;

        private List<GameObject> m_doorMarkers = new List<GameObject>();
        private int m_activeMarkerCount;
        private int m_trailerIndex = -1;


        private CoroutineHelper m_updateDoorList;


        public override void Start()
        {
            base.Start();

            UIView view = UIView.GetAView();
            width = WIDTH;
            height = HEIGHT;
            backgroundSprite = "MenuPanel2";
            name = Mod.ModName + " Doors Panel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            // Start in the top right corner next to the display options tool
            relativePosition = new Vector3(10 + UIMainPanel.WIDTH + 10 + UISettingsPanel.WIDTH + 10 + UIDisplayOptions.WIDTH + 10, 10);

            // Events
            PrefabWatcher.Instance.PrefabBecameVehicle += () =>
            {
                isVisible = DisplayOptions.ActiveOptions.ShowDoors;
                UpdateMarkerVisibility();
            };
            PrefabWatcher.Instance.PrefabWasVehicle += () =>
            {
                isVisible = false;
                UpdateMarkerVisibility();
            };
            UIMainPanel.eventSelectedUpdated += (v, i) =>
            {
                m_mainInfo = v;
                m_trailerIndex = i;
                m_selectedInfo = (m_trailerIndex < 0) ? m_mainInfo : m_mainInfo.m_trailers[m_trailerIndex].m_info;
                UpdateDoorList();
            };
            DisplayOptions.ActiveOptions.EventChanged += () =>
            {
                isVisible = DisplayOptions.ActiveOptions.ShowDoors;
                UpdateMarkerVisibility();
            };

            var prop = PrefabCollection<PropInfo>.FindLoaded("Door Marker");
            if(prop != null)
            {
                GameObject prefab = new GameObject("EAE Door Marker");
                var filter = prefab.AddComponent<MeshFilter>();
                filter.sharedMesh = prop.m_mesh;
                var renderer = prefab.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = prop.m_material;
                prefab.transform.parent = ModLoadingExtension.GameObject.transform;

                // 8 is enough, right?
                m_doorMarkers.Add(prefab);
                for(int i = 0; i < 7; i++)
                {
                    GameObject copy = GameObject.Instantiate(prefab);
                    copy.transform.parent = ModLoadingExtension.GameObject.transform;
                    m_doorMarkers.Add(copy);
                }

                UpdateMarkerVisibility();
            }

            CreateComponents();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            foreach(var g in m_doorMarkers)
            {
                GameObject.Destroy(g);
            }
            m_doorMarkers.Clear();
        }

        private void CreateComponents()
        {
            int headerHeight = 40;

            // Label
            UILabel label = AddUIComponent<UILabel>();
            label.text = "Doors";
            label.relativePosition = new Vector3(WIDTH / 2 - label.width / 2, 10);

            // Drag handle
            UIDragHandle handle = AddUIComponent<UIDragHandle>();
            handle.target = this;
            handle.constrainToScreen = true;
            handle.width = WIDTH;
            handle.height = headerHeight;
            handle.relativePosition = Vector3.zero;

            // Door selection
            label = AddUIComponent<UILabel>();
            label.text = "Door:";
            label.relativePosition = new Vector3(10, headerHeight + 15);

            m_doorDropdown = UIUtils.CreateDropDown(this);
            m_doorDropdown.width = WIDTH - 30 - label.width;
            m_doorDropdown.relativePosition = new Vector3(label.relativePosition.x + label.width + 10, headerHeight + 10);
            m_doorDropdown.eventSelectedIndexChanged += OnDoorSelectionChanged;

            // Door pos fields
            m_posFieldX = UIFloatField.CreateField("Pos X:", this);
            m_posFieldX.panel.relativePosition = new Vector3(10, headerHeight + 90);
            m_posFieldX.textField.eventTextChanged += (c, s) => {
                if(!m_checkEvents || m_selectedInfo == null || m_selectedInfo.m_doors == null || m_selectedInfo.m_doors.Length == 0) { return; }

                UIFloatField.FloatFieldHandler(m_posFieldX.textField, s, ref m_selectedInfo.m_doors[m_doorDropdown.selectedIndex].m_location.x);
                UpdateMarkerVisibility();
            };
            m_posFieldX.buttonUp.eventClicked += (c, b) => {
                if(!m_checkEvents || m_selectedInfo == null || m_selectedInfo.m_doors == null || m_selectedInfo.m_doors.Length == 0) { return; }

                m_posFieldX.SetValue(m_selectedInfo.m_doors[m_doorDropdown.selectedIndex].m_location.x + 0.1f);
            };
            m_posFieldX.buttonDown.eventClicked += (c, b) => {
                if(!m_checkEvents || m_selectedInfo == null || m_selectedInfo.m_doors == null || m_selectedInfo.m_doors.Length == 0) { return; }

                m_posFieldX.SetValue(m_selectedInfo.m_doors[m_doorDropdown.selectedIndex].m_location.x - 0.1f);
            };


            m_posFieldY = UIFloatField.CreateField("Pos Y:", this);
            m_posFieldY.panel.relativePosition = new Vector3(10, headerHeight + 120);
            m_posFieldY.textField.eventTextChanged += (c, s) => {
                if(!m_checkEvents || m_selectedInfo == null || m_selectedInfo.m_doors == null || m_selectedInfo.m_doors.Length == 0) { return; }

                UIFloatField.FloatFieldHandler(m_posFieldY.textField, s, ref m_selectedInfo.m_doors[m_doorDropdown.selectedIndex].m_location.y);
                UpdateMarkerVisibility();
            };
            m_posFieldY.buttonUp.eventClicked += (c, b) => {
                if(!m_checkEvents || m_selectedInfo == null || m_selectedInfo.m_doors == null || m_selectedInfo.m_doors.Length == 0) { return; }

                m_posFieldY.SetValue(m_selectedInfo.m_doors[m_doorDropdown.selectedIndex].m_location.y + 0.1f);
            };
            m_posFieldY.buttonDown.eventClicked += (c, b) => {
                if(!m_checkEvents || m_selectedInfo == null || m_selectedInfo.m_doors == null || m_selectedInfo.m_doors.Length == 0) { return; }

                m_posFieldY.SetValue(m_selectedInfo.m_doors[m_doorDropdown.selectedIndex].m_location.y - 0.1f);
            };


            m_posFieldZ = UIFloatField.CreateField("Pos Z:", this);
            m_posFieldZ.panel.relativePosition = new Vector3(10, headerHeight + 150);
            m_posFieldZ.textField.eventTextChanged += (c, s) => {
                if(!m_checkEvents || m_selectedInfo == null || m_selectedInfo.m_doors == null || m_selectedInfo.m_doors.Length == 0) { return; }

                UIFloatField.FloatFieldHandler(m_posFieldZ.textField, s, ref m_selectedInfo.m_doors[m_doorDropdown.selectedIndex].m_location.z);
                UpdateMarkerVisibility();
            };
            m_posFieldZ.buttonUp.eventClicked += (c, b) => {
                if(!m_checkEvents || m_selectedInfo == null || m_selectedInfo.m_doors == null || m_selectedInfo.m_doors.Length == 0) { return; }

                m_posFieldZ.SetValue(m_selectedInfo.m_doors[m_doorDropdown.selectedIndex].m_location.z + 0.1f);
            };
            m_posFieldZ.buttonDown.eventClicked += (c, b) => {
                if(!m_checkEvents || m_selectedInfo == null || m_selectedInfo.m_doors == null || m_selectedInfo.m_doors.Length == 0) { return; }

                m_posFieldZ.SetValue(m_selectedInfo.m_doors[m_doorDropdown.selectedIndex].m_location.z - 0.1f);
            };

            // Add and remove buttons
            m_addButton = UIUtils.CreateButton(this);
            m_addButton.text = "Add";
            m_addButton.relativePosition = new Vector3(WIDTH / 2 - m_addButton.width - 10, headerHeight + 50);
            m_addButton.eventClicked += (c, b) => {
                if(!m_checkEvents || m_selectedInfo == null) { return; }

                TryAddDoor();
            };

            m_removeButton = UIUtils.CreateButton(this);
            m_removeButton.text = "Remove";
            m_removeButton.relativePosition = new Vector3(WIDTH / 2 + 10, headerHeight + 50);
            m_removeButton.eventClicked += (c, b) => {
                if(!m_checkEvents || m_selectedInfo == null) { return; }

                TryRemoveDoor();
            };

            //
            m_checkEvents = true;
        }

        private void TryAddDoor()
        {
            if(m_selectedInfo.m_doors == null)
            {
                m_selectedInfo.m_doors = new VehicleInfo.VehicleDoor[0];
            }
            if(m_selectedInfo.m_doors.Length < m_doorMarkers.Count)
            {
                VehicleInfo.VehicleDoor door = new VehicleInfo.VehicleDoor();
                door.m_location = Vector3.zero;
                door.m_type = VehicleInfo.DoorType.Both;

                m_selectedInfo.m_doors = Util.LengthenArray(m_selectedInfo.m_doors, door);
                UpdateDoorList();
                m_doorDropdown.selectedIndex = m_doorDropdown.items.Length - 1;
            }
        }

        private void TryRemoveDoor()
        {
            if(m_selectedInfo.m_doors == null)
            {
                return;
            }
            if(m_doorDropdown.selectedIndex >= 0 && m_doorDropdown.selectedIndex < m_selectedInfo.m_doors.Length)
            {
                List<VehicleInfo.VehicleDoor> doorList = new List<VehicleInfo.VehicleDoor>();
                doorList.AddRange(m_selectedInfo.m_doors);
                doorList.RemoveAt(m_doorDropdown.selectedIndex);
                m_selectedInfo.m_doors = doorList.ToArray();
                UpdateDoorList();
            }
        }

        private void UpdateDoorList()
        {
            if(m_updateDoorList == null)
            {
                m_updateDoorList = CoroutineHelper.Create(UpdateDoorListImpl);
            }
            m_updateDoorList.Run();
        }

        private void UpdateDoorListImpl()
        {
            m_activeMarkerCount = 0;
            foreach(var g in m_doorMarkers)
            {
                g.SetActive(false);
            }

            if(m_selectedInfo != null && m_doorDropdown != null)
            {
                // Dropdown update
                int selectedIndex = m_doorDropdown.selectedIndex;
                int count = m_selectedInfo.m_doors != null ? m_selectedInfo.m_doors.Length : 0;

                string[] doorNames = new string[count];
                for(int i = 0; i < count; i++)
                {
                    doorNames[i] = $"Door {i} ({m_selectedInfo.m_doors[i].m_type})";
                }

                m_doorDropdown.items = doorNames;
                if(count != 0)
                {
                    selectedIndex = Mathf.FloorToInt(Mathf.Clamp(selectedIndex, 0, count));
                }
                else
                {
                    selectedIndex = -1;
                }

                m_activeMarkerCount = count;

                m_doorDropdown.selectedIndex = selectedIndex;
                OnDoorSelectionChanged(null, selectedIndex);
            }
            else
            {
                UpdateMarkerVisibility();
            }
        }

        public void UpdateMarkerVisibility()
        {
            foreach(var g in m_doorMarkers)
            {
                g.SetActive(false);
            }
            if(isVisible)
            {
                for(int i = 0; i < m_doorMarkers.Count && i < m_activeMarkerCount; i++)
                {
                    m_doorMarkers[i].SetActive(true);
                    m_doorMarkers[i].transform.position = GetDoorPositionUnchecked(i);
                }
            }
        }

        private Vector3 GetDoorPositionUnchecked(int doorIndex)
        {
            if(m_trailerIndex < 0)
            {
                return m_selectedInfo.m_doors[doorIndex].m_location + new Vector3(0, 60, 0);
            }
            else
            {
                Quaternion rotation = m_mainInfo.m_trailers[m_trailerIndex].m_invertProbability >= 100 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
                Vector3 doorPos = rotation * m_selectedInfo.m_doors[doorIndex].m_location;

                float num = m_mainInfo.m_generatedInfo.m_size.z * 0.5f - m_mainInfo.m_attachOffsetBack;
                float frontDelta = 0;
                float backDelta = 0;
                bool isInverted = false;
                for(int i = 0; i <= m_trailerIndex; i++)
                {
                    num += isInverted ? frontDelta : backDelta;

                    VehicleInfo info = m_mainInfo.m_trailers[i].m_info;
                    isInverted = (m_mainInfo.m_trailers[i].m_invertProbability >= 100);
                    frontDelta = info.m_generatedInfo.m_size.z * 0.5f - info.m_attachOffsetFront;
                    backDelta = info.m_generatedInfo.m_size.z * 0.5f - info.m_attachOffsetBack;
                    num += isInverted ? backDelta : frontDelta;
                }

                doorPos += new Vector3(0, 60, -num);
                return doorPos;
            }
        }

        private void OnDoorSelectionChanged(UIComponent component, int value)
        {
            if (m_doorDropdown.selectedIndex < 0)
                return;

            if(m_selectedInfo != null)
            {
                if(m_selectedInfo.m_doors != null && m_selectedInfo.m_doors.Length != 0)
                {
                    m_checkEvents = false;
                    m_posFieldX.SetValue(m_selectedInfo.m_doors[m_doorDropdown.selectedIndex].m_location.x);
                    m_posFieldY.SetValue(m_selectedInfo.m_doors[m_doorDropdown.selectedIndex].m_location.y);
                    m_posFieldZ.SetValue(m_selectedInfo.m_doors[m_doorDropdown.selectedIndex].m_location.z);
                    m_checkEvents = true;
                    UpdateMarkerVisibility();
                }
            }
        }
    }
}
