using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ExtendedAssetEditor.UI
{
    public class UISettingsPanel : UIPanel
    {
        public const int WIDTH = 250;
        public const int HEIGHT = 420;

        private VehicleInfo m_leadVehicle;
        private VehicleInfo m_vehicle;

        private UIFloatField m_offsetFront;
        private UIFloatField m_offsetBack;
        private UIFloatField m_acceleration;
        private UIFloatField m_braking;
        private UIFloatField m_nod;
        private UIFloatField m_lean;
        private UIFloatField m_dampers;
        private UIFloatField m_springs;
        private UIFloatField m_turning;

        private UIButton m_applyToAll;

        public override void Start()
        {
            base.Start();

            UIView view = UIView.GetAView();
            width = WIDTH;
            height = HEIGHT;
            backgroundSprite = "MenuPanel2";
            name = Mod.ModName + " Settings Panel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            // Start in the top left corner
            relativePosition = new Vector3(10 + UIMainPanel.WIDTH + 10, 10);

            // Events
            DisplayOptions.ActiveOptions.EventChanged += () =>
            {
                isVisible = DisplayOptions.ActiveOptions.ShowSettingsPanel;
            };
            PrefabWatcher.Instance.PrefabBecameVehicle += () =>
            {
                isVisible = DisplayOptions.ActiveOptions.ShowSettingsPanel;
            };
            PrefabWatcher.Instance.PrefabWasVehicle += () =>
            {
                m_leadVehicle = null;
                m_vehicle = null;
                isVisible = false;
            };
            UIMainPanel.eventSelectedUpdated += (leadVehicle, trailerIndex) =>
            {
                m_leadVehicle = leadVehicle;
                m_vehicle = (trailerIndex < 0) ? leadVehicle : leadVehicle.m_trailers[trailerIndex].m_info;
                RefreshFields();
                Debug.Log("Settings panel: Selected changed");
            };


            CreateComponents();
        }

        private void CreateComponents()
        {
            int headerHeight = 40;
            int labelWidth = 130;

            // Label
            UILabel label = AddUIComponent<UILabel>();
            label.text = "Vehicle Settings";
            label.relativePosition = new Vector3(WIDTH / 2 - label.width / 2, 10);

            // Drag handle
            UIDragHandle handle = AddUIComponent<UIDragHandle>();
            handle.target = this;
            handle.constrainToScreen = true;
            handle.width = WIDTH;
            handle.height = headerHeight;
            handle.relativePosition = Vector3.zero;

            Vector3 nextFreeRelPos = new Vector3(10, headerHeight + 10);
            // Offset front
            m_offsetFront = UIFloatField.CreateField("Offset front:", labelWidth, this, false);
            m_offsetFront.panel.relativePosition = nextFreeRelPos;
            nextFreeRelPos.y += 30;
            m_offsetFront.textField.eventTextChanged += (c, text) =>
            {
                if(m_vehicle != null)
                {
                    m_offsetFront.FloatFieldHandler(ref m_vehicle.m_attachOffsetFront);
                }
            };

            // Offset back
            m_offsetBack = UIFloatField.CreateField("Offset back:", labelWidth, this, false);
            m_offsetBack.panel.relativePosition = nextFreeRelPos;
            nextFreeRelPos.y += 30;
            m_offsetBack.textField.eventTextChanged += (c, text) =>
            {
                if(m_vehicle != null)
                {
                    m_offsetBack.FloatFieldHandler(ref m_vehicle.m_attachOffsetBack);
                }
            };

            // Acceleration
            m_acceleration = UIFloatField.CreateField("Acceleration:", labelWidth, this, false);
            m_acceleration.panel.relativePosition = nextFreeRelPos;
            nextFreeRelPos.y += 30;
            m_acceleration.textField.eventTextChanged += (c, text) =>
            {
                if(m_vehicle != null)
                {
                    m_acceleration.FloatFieldHandler(ref m_vehicle.m_acceleration);
                }
            };

            // Braking
            m_braking = UIFloatField.CreateField("Braking:", labelWidth, this, false);
            m_braking.panel.relativePosition = nextFreeRelPos;
            nextFreeRelPos.y += 30;
            m_braking.textField.eventTextChanged += (c, text) =>
            {
                if(m_vehicle != null)
                {
                    m_braking.FloatFieldHandler(ref m_vehicle.m_braking);
                }
            };

            // Nod
            m_nod = UIFloatField.CreateField("Nod:", labelWidth, this, false);
            m_nod.panel.relativePosition = nextFreeRelPos;
            nextFreeRelPos.y += 30;
            m_nod.textField.eventTextChanged += (c, text) =>
            {
                if(m_vehicle != null)
                {
                    m_nod.FloatFieldHandler(ref m_vehicle.m_nodMultiplier);
                }
            };

            // Lean
            m_lean = UIFloatField.CreateField("Lean:", labelWidth, this, false);
            m_lean.panel.relativePosition = nextFreeRelPos;
            nextFreeRelPos.y += 30;
            m_lean.textField.eventTextChanged += (c, text) =>
            {
                if(m_vehicle != null)
                {
                    m_lean.FloatFieldHandler(ref m_vehicle.m_leanMultiplier);
                }
            };

            // Dampers
            m_dampers = UIFloatField.CreateField("Dampers:", labelWidth, this, false);
            m_dampers.panel.relativePosition = nextFreeRelPos;
            nextFreeRelPos.y += 30;
            m_dampers.textField.eventTextChanged += (c, text) =>
            {
                if(m_vehicle != null)
                {
                    m_dampers.FloatFieldHandler(ref m_vehicle.m_dampers);
                }
            };

            // Springs
            m_springs = UIFloatField.CreateField("Springs:", labelWidth, this, false);
            m_springs.panel.relativePosition = nextFreeRelPos;
            nextFreeRelPos.y += 30;
            m_springs.textField.eventTextChanged += (c, text) =>
            {
                if(m_vehicle != null)
                {
                    m_springs.FloatFieldHandler(ref m_vehicle.m_springs);
                }
            };

            // Turning
            m_turning = UIFloatField.CreateField("Turning:", labelWidth, this, false);
            m_turning.panel.relativePosition = nextFreeRelPos;
            nextFreeRelPos.y += 30;
            m_turning.textField.eventTextChanged += (c, text) =>
            {
                if(m_vehicle != null)
                {
                    m_turning.FloatFieldHandler(ref m_vehicle.m_turning);
                }
            };

            // Button
            m_applyToAll = UIUtils.CreateButton(this);
            m_applyToAll.text = "Apply to all";
            m_applyToAll.tooltip = "Applies the settings, excluding offsets, to all vehicles";
            m_applyToAll.eventClicked += (c, p) =>
            {
                if(m_vehicle != null)
                {
                    ApplyToAll();
                }
            };
            m_applyToAll.relativePosition = new Vector3((WIDTH - m_applyToAll.width) / 2, HEIGHT - 40);
            m_applyToAll.width += 20;
        }

        private void RefreshFields()
        {
            if(m_vehicle != null)
            {
                m_offsetFront.SetValue(m_vehicle.m_attachOffsetFront);
                m_offsetBack.SetValue(m_vehicle.m_attachOffsetBack);
                m_acceleration.SetValue(m_vehicle.m_acceleration);
                m_braking.SetValue(m_vehicle.m_braking);
                m_nod.SetValue(m_vehicle.m_nodMultiplier);
                m_lean.SetValue(m_vehicle.m_leanMultiplier);
                m_springs.SetValue(m_vehicle.m_springs);
                m_dampers.SetValue(m_vehicle.m_dampers);
                m_turning.SetValue(m_vehicle.m_turning);
            }
        }

        private void ApplyToAll()
        {
            if(m_leadVehicle != null)
            {
                // apply to engine
                m_acceleration.FloatFieldHandler(ref m_leadVehicle.m_acceleration);
                m_braking.FloatFieldHandler(ref m_leadVehicle.m_braking);
                m_nod.FloatFieldHandler(ref m_leadVehicle.m_nodMultiplier);
                m_lean.FloatFieldHandler(ref m_leadVehicle.m_leanMultiplier);
                m_dampers.FloatFieldHandler(ref m_leadVehicle.m_dampers);
                m_springs.FloatFieldHandler(ref m_leadVehicle.m_springs);
                m_turning.FloatFieldHandler(ref m_leadVehicle.m_turning);

                // apply to trailers
                if(m_leadVehicle.m_trailers != null)
                {
                    foreach(var trailer in m_leadVehicle.m_trailers)
                    {
                        m_acceleration.FloatFieldHandler(ref trailer.m_info.m_acceleration);
                        m_braking.FloatFieldHandler(ref trailer.m_info.m_braking);
                        m_nod.FloatFieldHandler(ref trailer.m_info.m_nodMultiplier);
                        m_lean.FloatFieldHandler(ref trailer.m_info.m_leanMultiplier);
                        m_dampers.FloatFieldHandler(ref trailer.m_info.m_dampers);
                        m_springs.FloatFieldHandler(ref trailer.m_info.m_springs);
                        m_turning.FloatFieldHandler(ref trailer.m_info.m_turning);
                    }
                }
            }
        }
    }
}
