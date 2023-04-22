namespace ExtendedAssetEditor
{
    public class DisplayOptions
    {
        private static DisplayOptions _activeOptions;
        public static DisplayOptions ActiveOptions
        {
            get
            {
                if(_activeOptions == null)
                {
                    _activeOptions = new DisplayOptions();
                }
                return _activeOptions;
            }
            set
            {
                _activeOptions = value;
            }
        }

        private bool _reversed;
        private bool _showDoors = true;
        private bool _showEmergency = false;
        private bool _showEmergency2 = false;
        private bool _showLanding = false;
        private bool _showTakeOff = false;
        private bool _showSettings = true;
        private int _gateIndex = 0;
        private bool _useGateIndex = false;

        public bool Reversed
        {
            get
            {
                return _reversed;
            }
            set
            {
                _reversed = value;
                EventChanged?.Invoke();
            }
        }
        public bool ShowDoors
        {
            get
            {
                return _showDoors;
            }
            set
            {
                _showDoors = value;
                EventChanged?.Invoke();
            }
        }

        public bool ShowEmergency
        {
            get
            {
                return _showEmergency;
            }
            set
            {
                _showEmergency = value;
                EventChanged?.Invoke();
            }
        }

        public bool ShowEmergency2
        {
            get
            {
                return _showEmergency2;
            }
            set
            {
                _showEmergency2 = value;
                EventChanged?.Invoke();
            }
        }

        public bool ShowLanding
        {
            get
            {
                return _showLanding;
            }
            set
            {
                _showLanding = value;
                EventChanged?.Invoke();
            }
        }

        public bool ShowTakeOff
        {
            get
            {
                return _showTakeOff;
            }
            set
            {
                _showTakeOff = value;
                EventChanged?.Invoke();
            }
        }

        public bool ShowSettingsPanel
        {
            get
            {
                return _showSettings;
            }
            set
            {
                _showSettings = value;
                EventChanged?.Invoke();
            }
        }

        public int GateIndex
        {
            get
            {
                return _gateIndex;
            }
            set
            {
                _gateIndex = value;
                EventChanged?.Invoke();
            }
        }

        public bool UseGateIndex
        {
            get => _useGateIndex;
            set
            {
                _useGateIndex = value;
                EventChanged?.Invoke();
            }
        }


        public delegate void OnOptionsChanged();
        public event OnOptionsChanged EventChanged;
    }
}
