using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExtendedAssetEditor.UI
{
    public class UIDisplayOptions : UIPanel
    {
        public const int WIDTH = 220;
        public const int HEIGHT = 280;

        private UIDropDown m_directionDropdown;
        private UICheckBox m_doorCheckbox;
        private UICheckBox m_emergencyCheckbox1;
        private UICheckBox m_emergencyCheckbox2;
        private UICheckBox m_landingCheckbox;
        private UICheckBox m_takeOffCheckbox;
        private UICheckBox m_showSettingsCheckbox;


        public override void Start()
        {
            base.Start();

            UIView view = UIView.GetAView();
            width = WIDTH;
            height = HEIGHT;
            backgroundSprite = "MenuPanel2";
            name = Mod.name + " Display Options Panel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            // Start in the top
            relativePosition = new Vector3(10 + UIMainPanel.WIDTH + 10 + UISettingsPanel.WIDTH + 10, 10);

            // Events
            PrefabWatcher.instance.prefabBecameVehicle += () =>
            {
                Debug.Log("Prefab became vehicle");
                isVisible = true;
            };
            PrefabWatcher.instance.prefabWasVehicle += () =>
            {
                Debug.Log("Prefab was vehicle");
                isVisible = false;
            };

            CreateComponents();
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
                DisplayOptions.activeOptions.Reversed = (i == 1);
            };
            m_directionDropdown.tooltip = "Simulates vehicle movement direction so you can preview headlights.";

            // Door display
            m_doorCheckbox = (UICheckBox)uiHelper.AddCheckbox("Show doors", DisplayOptions.activeOptions.ShowDoors, (b) => {
                DisplayOptions.activeOptions.ShowDoors = b;
            });
            m_doorCheckbox.relativePosition = new Vector3(10, headerHeight + 50);
            m_doorCheckbox.width = WIDTH - 20;
            m_doorCheckbox.tooltip = "Show vehicle door locations and editor.";

            // Emergency lights 1
            m_emergencyCheckbox1 = (UICheckBox)uiHelper.AddCheckbox("Show emergency 1", DisplayOptions.activeOptions.ShowEmergency, (b) => {
                DisplayOptions.activeOptions.ShowEmergency = b;
            });
            m_emergencyCheckbox1.relativePosition = new Vector3(10, headerHeight + 80);
            m_emergencyCheckbox1.width = WIDTH - 20;
            m_emergencyCheckbox1.tooltip = "Show vehicle emergency effects.";

            // Emergency lights 2
            m_emergencyCheckbox2 = (UICheckBox)uiHelper.AddCheckbox("Show emergency 2", DisplayOptions.activeOptions.ShowEmergency2, (b) => {
                DisplayOptions.activeOptions.ShowEmergency2 = b;
            });
            m_emergencyCheckbox2.relativePosition = new Vector3(10, headerHeight + 110);
            m_emergencyCheckbox2.width = WIDTH - 20;
            m_emergencyCheckbox2.tooltip = "Show vehicle emergency effects.";

            // Takeoff
            m_takeOffCheckbox = (UICheckBox)uiHelper.AddCheckbox("Show takeoff", DisplayOptions.activeOptions.ShowTakeOff, (b) => {
                DisplayOptions.activeOptions.ShowTakeOff = b;
            });
            m_takeOffCheckbox.relativePosition = new Vector3(10, headerHeight + 140);
            m_takeOffCheckbox.width = WIDTH - 20;
            m_takeOffCheckbox.tooltip = "Show takeoff specific effects.";

            // Landing
            m_landingCheckbox = (UICheckBox)uiHelper.AddCheckbox("Show landing", DisplayOptions.activeOptions.ShowLanding, (b) => {
                DisplayOptions.activeOptions.ShowLanding = b;
            });
            m_landingCheckbox.relativePosition = new Vector3(10, headerHeight + 170);
            m_landingCheckbox.width = WIDTH - 20;
            m_landingCheckbox.tooltip = "Show landing specific effects.";

            // Settings
            m_showSettingsCheckbox = (UICheckBox)uiHelper.AddCheckbox("Show settings", DisplayOptions.activeOptions.ShowSettingsPanel, (b) => {
                DisplayOptions.activeOptions.ShowSettingsPanel = b;
            });
            m_showSettingsCheckbox.relativePosition = new Vector3(10, headerHeight + 200);
            m_showSettingsCheckbox.width = WIDTH - 20;
            m_showSettingsCheckbox.tooltip = "Show settings panel.";
        }
    }
}
