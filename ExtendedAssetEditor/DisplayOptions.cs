using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtendedAssetEditor
{
    public class DisplayOptions
    {
        public static DisplayOptions activeOptions;

        private bool m_reversed;
        private bool m_showDoors = true;
        private bool m_showEmergency = false;
        private bool m_showEmergency2 = false;
        private bool m_showLanding = false;
        private bool m_showTakeOff = false;

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

        public delegate void OnOptionsChanged();
        public event OnOptionsChanged eventChanged;

        public DisplayOptions()
        {
            if(activeOptions == null)
            {
                activeOptions = this;
            }
        }
    }
}
