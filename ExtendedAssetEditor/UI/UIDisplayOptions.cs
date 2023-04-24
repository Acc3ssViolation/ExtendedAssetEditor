using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace ExtendedAssetEditor.UI
{
    public class UIDisplayOptions : UIPanel
    {
        public const int WIDTH = 220;
        public const int HEIGHT = 340;

        private UIDropDown m_directionDropdown;
        private UICheckBox m_doorCheckbox;
        private UICheckBox m_emergencyCheckbox1;
        private UICheckBox m_emergencyCheckbox2;
        private UICheckBox m_landingCheckbox;
        private UICheckBox m_takeOffCheckbox;
        private UICheckBox m_showSettingsCheckbox;
        private UICheckBox m_useGateIndexCheckbox;
        private UIDropDown m_gateIndex;

        // Mapped from TransferManager.TransferReason via CargoTrainAI.RefreshVariations
        private static readonly string[] GateIndexNames = new string[]
        {
            "Generic",
            "Generic empty",
            "Animal products",
            "Grain",
            "Logs",
            "Logs empty",
            "Oil products",
            "Ore",
            "Ore empty",
            "(not for trains)"
        };

        public override void Start()
        {
            base.Start();

            UIView view = UIView.GetAView();
            width = WIDTH;
            height = HEIGHT;
            backgroundSprite = "MenuPanel2";
            name = Mod.ModName + " Display Options Panel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            // Start in the top
            relativePosition = new Vector3(10 + UIMainPanel.WIDTH + 10 + UISettingsPanel.WIDTH + 10, 10);

            // Events
            PrefabWatcher.Instance.PrefabBecameVehicle += OnBecameVehicle;
            PrefabWatcher.Instance.PrefabWasVehicle += OnWasVehicle;
            DisplayOptions.ActiveOptions.EventChanged += OnOptionsChanged;

            CreateComponents();
            OnOptionsChanged();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // Events
            PrefabWatcher.Instance.PrefabBecameVehicle -= OnBecameVehicle;
            PrefabWatcher.Instance.PrefabWasVehicle -= OnWasVehicle;
            DisplayOptions.ActiveOptions.EventChanged -= OnOptionsChanged;
        }

        private void OnBecameVehicle()
        {
            isVisible = true;
        }

        private void OnWasVehicle()
        {
            isVisible = false;
        }

        private void OnOptionsChanged()
        {
            m_gateIndex.isVisible = DisplayOptions.ActiveOptions.UseGateIndex;
        }

        private void CreateComponents()
        {
            int headerHeight = 40;
            UIHelperBase uiHelper = new UIHelper(this);

            // Label
            UILabel label = AddUIComponent<UILabel>();
            label.text = "Display Options";
            label.relativePosition = new Vector3(WIDTH / 2 - label.width / 2, 10);

            // Drag handle
            UIDragHandle handle = AddUIComponent<UIDragHandle>();
            handle.target = this;
            handle.constrainToScreen = true;
            handle.width = WIDTH;
            handle.height = headerHeight;
            handle.relativePosition = Vector3.zero;

            // 'moving' direction
            label = AddUIComponent<UILabel>();
            label.text = "Direction:";
            label.relativePosition = new Vector3(10, headerHeight + 15);

            m_directionDropdown = UIUtils.CreateDropDown(this);
            m_directionDropdown.width = WIDTH - 30 - label.width;
            m_directionDropdown.relativePosition = new Vector3(label.relativePosition.x + label.width + 10, headerHeight + 10);
            m_directionDropdown.AddItem("Forward");
            m_directionDropdown.AddItem("Reverse");
            m_directionDropdown.selectedIndex = 0;
            m_directionDropdown.eventSelectedIndexChanged += (c, i) => {
                DisplayOptions.ActiveOptions.Reversed = (i == 1);
            };
            m_directionDropdown.tooltip = "Simulates vehicle movement direction so you can preview headlights.";

            // Door display
            m_doorCheckbox = (UICheckBox)uiHelper.AddCheckbox("Show doors", DisplayOptions.ActiveOptions.ShowDoors, (b) => {
                DisplayOptions.ActiveOptions.ShowDoors = b;
            });
            m_doorCheckbox.relativePosition = new Vector3(10, headerHeight + 50);
            m_doorCheckbox.width = WIDTH - 20;
            m_doorCheckbox.tooltip = "Show vehicle door locations and editor.";

            // Emergency lights 1
            m_emergencyCheckbox1 = (UICheckBox)uiHelper.AddCheckbox("Show emergency 1", DisplayOptions.ActiveOptions.ShowEmergency, (b) => {
                DisplayOptions.ActiveOptions.ShowEmergency = b;
            });
            m_emergencyCheckbox1.relativePosition = new Vector3(10, headerHeight + 80);
            m_emergencyCheckbox1.width = WIDTH - 20;
            m_emergencyCheckbox1.tooltip = "Show vehicle emergency effects.";

            // Emergency lights 2
            m_emergencyCheckbox2 = (UICheckBox)uiHelper.AddCheckbox("Show emergency 2", DisplayOptions.ActiveOptions.ShowEmergency2, (b) => {
                DisplayOptions.ActiveOptions.ShowEmergency2 = b;
            });
            m_emergencyCheckbox2.relativePosition = new Vector3(10, headerHeight + 110);
            m_emergencyCheckbox2.width = WIDTH - 20;
            m_emergencyCheckbox2.tooltip = "Show vehicle emergency effects.";

            // Takeoff
            m_takeOffCheckbox = (UICheckBox)uiHelper.AddCheckbox("Show takeoff", DisplayOptions.ActiveOptions.ShowTakeOff, (b) => {
                DisplayOptions.ActiveOptions.ShowTakeOff = b;
            });
            m_takeOffCheckbox.relativePosition = new Vector3(10, headerHeight + 140);
            m_takeOffCheckbox.width = WIDTH - 20;
            m_takeOffCheckbox.tooltip = "Show takeoff specific effects.";

            // Landing
            m_landingCheckbox = (UICheckBox)uiHelper.AddCheckbox("Show landing", DisplayOptions.ActiveOptions.ShowLanding, (b) => {
                DisplayOptions.ActiveOptions.ShowLanding = b;
            });
            m_landingCheckbox.relativePosition = new Vector3(10, headerHeight + 170);
            m_landingCheckbox.width = WIDTH - 20;
            m_landingCheckbox.tooltip = "Show landing specific effects.";

            // Settings
            m_showSettingsCheckbox = (UICheckBox)uiHelper.AddCheckbox("Show settings", DisplayOptions.ActiveOptions.ShowSettingsPanel, (b) => {
                DisplayOptions.ActiveOptions.ShowSettingsPanel = b;
            });
            m_showSettingsCheckbox.relativePosition = new Vector3(10, headerHeight + 200);
            m_showSettingsCheckbox.width = WIDTH - 20;
            m_showSettingsCheckbox.tooltip = "Show settings panel.";

            // Use gate index
            m_useGateIndexCheckbox = (UICheckBox)uiHelper.AddCheckbox("Show cargo", DisplayOptions.ActiveOptions.UseGateIndex, (b) => {
                DisplayOptions.ActiveOptions.UseGateIndex = b;
            });
            m_useGateIndexCheckbox.relativePosition = new Vector3(10, headerHeight + 230);
            m_useGateIndexCheckbox.width = WIDTH - 20;
            m_useGateIndexCheckbox.tooltip = "Enable cargo variation preview using submeshes.";

            // Gate index
            m_gateIndex = UIUtils.CreateDropDown(this);
            m_gateIndex.width = WIDTH - 20;
            m_gateIndex.relativePosition = new Vector3(10, headerHeight + 260);
            for (var i = 0; i < GateIndexNames.Length; i++)
            {
                m_gateIndex.AddItem(i.ToString() + " - " + GateIndexNames[i]);
            }
            m_gateIndex.selectedIndex = 0;
            m_gateIndex.eventSelectedIndexChanged += (c, i) => {
                DisplayOptions.ActiveOptions.GateIndex = i;
            };
            m_gateIndex.tooltip = "Select a cargo type to preview.";
        }
    }
}
