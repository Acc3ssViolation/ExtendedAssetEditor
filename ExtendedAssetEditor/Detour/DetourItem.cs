using System.Reflection;

namespace ExtendedAssetEditor.Detour
{
    public class DetourItem
    {
        private readonly string _name;
        private readonly MethodInfo _original;
        private readonly MethodInfo _detour;
        private RedirectCallsState _state;
        private bool _deployed;

        public bool Deployed { get { return _deployed; } }

        public DetourItem(string name, MethodInfo original, MethodInfo detour)
        {
            _name = name;
            _original = original;
            _detour = detour;
        }

        public void Deploy()
        {
            if(_deployed || _original == null || _detour == null)
            {
                Util.LogWarning("Detour not possible for " + _name);
                return;
            }

            _deployed = true;
            _state = RedirectionHelper.RedirectCalls(_original, _detour);

            Util.Log("DetourItem: Detoured " + _name);
        }

        public void Revert()
        {
            if(!_deployed || _original == null)
                return;

            _deployed = false;
            RedirectionHelper.RevertRedirect(_original, _state);

            Util.Log("DetourItem: Reverted " + _name);
        }
    }
}
