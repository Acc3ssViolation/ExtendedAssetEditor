using ICities;

namespace ExtendedAssetEditor
{
    public class Mod : IUserMod
    {
        public const string ModName = "Extended Asset Editor";
        public const string VersionString = "0.7.4-beta";
        public const string HarmonyPackage = "com.github.accessviolation.eae";

        public string Description
        {
            get
            {
                return "Version " + VersionString + ". Adds various features for dealing with vehicles in the Asset Editor.";
            }
        }

        public string Name
        {
            get
            {
                return ModName;
            }
        }

        public static bool IsValidLoadMode(LoadMode mode)
        {
            return (mode == LoadMode.LoadAsset || mode == LoadMode.NewAsset);
        }
    }
}
