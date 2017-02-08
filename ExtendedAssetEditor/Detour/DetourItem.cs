using System.Reflection;

namespace ExtendedAssetEditor.Detour
{
    public class DetourItem
    {
        string name;
        MethodInfo original;
        MethodInfo detour;
        RedirectCallsState state;
        bool deployed;

        public bool Deployed { get { return deployed; } }

        public DetourItem(string name, MethodInfo original, MethodInfo detour)
        {
            this.name = name;
            this.original = original;
            this.detour = detour;
        }

        public void Deploy()
        {
            if(deployed || original == null || detour == null)
            {
                UnityEngine.Debug.LogWarning("Detour not possible for " + name);
                return;
            }

            deployed = true;
            state = RedirectionHelper.RedirectCalls(original, detour);

            UnityEngine.Debug.Log("DetourItem: Detoured " + name);
        }

        public void Revert()
        {
            if(!deployed || original == null)
                return;

            deployed = false;
            RedirectionHelper.RevertRedirect(original, state);

            UnityEngine.Debug.Log("DetourItem: Reverted " + name);
        }
    }
}
