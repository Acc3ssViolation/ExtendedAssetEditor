using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtendedAssetEditor
{
    public class DisplayOptions
    {
        private static DisplayOptions m_activeOptions;
        public static DisplayOptions activeOptions
        {
            get
            {
                if(m_activeOptions == null)
                {
                    m_activeOptions = new DisplayOptions();
                }
                return m_activeOptions;
            }
            set
            {
                m_activeOptions = value;
            }
        }

        private bool m_reversed;
        private bool m_showDoors = true;
        private bool m_showEmergency = false;
        private bool m_showEmergency2 = false;
        private bool m_showLanding = false;
        private bool m_showTakeOff = false;
        private bool m_showSettings = true;
        private int m_gateIndex = 0;
        private bool m_useGateIndex = false;

        private CoroutineHelper m_helper;

        public bool Reversed
        {
            get
            {
                return m_reversed;
            }
            set
            {
                m_reversed = value;
                eventChanged?.Invoke();
            }
        }
        public bool ShowDoors
        {
            get
            {
                return m_showDoors;
            }
            set
            {
                m_showDoors = value;
                eventChanged?.Invoke();
            }
        }

        public bool ShowEmergency
        {
            get
            {
                return m_showEmergency;
            }
            set
            {
                m_showEmergency = value;
                eventChanged?.Invoke();
            }
        }

        public bool ShowEmergency2
        {
            get
            {
                return m_showEmergency2;
            }
            set
            {
                m_showEmergency2 = value;
                eventChanged?.Invoke();
            }
        }

        public bool ShowLanding
        {
            get
            {
                return m_showLanding;
            }
            set
            {
                m_showLanding = value;
                eventChanged?.Invoke();
            }
        }

        public bool ShowTakeOff
        {
            get
            {
                return m_showTakeOff;
            }
            set
            {
                m_showTakeOff = value;
                eventChanged?.Invoke();
            }
        }

        public bool ShowSettingsPanel
        {
            get
            {
                return m_showSettings;
            }
            set
            {
                m_showSettings = value;
                eventChanged?.Invoke();
            }
        }

        public int GateIndex
        {
            get
            {
                return m_gateIndex;
            }
            set
            {
                m_gateIndex = value;
                eventChanged?.Invoke();
            }
        }

        public bool UseGateIndex
        {
            get => m_useGateIndex;
            set
            {
                m_useGateIndex = value;
                eventChanged?.Invoke();
            }
        }


        public delegate void OnOptionsChanged();
        public event OnOptionsChanged eventChanged;
    }
}
