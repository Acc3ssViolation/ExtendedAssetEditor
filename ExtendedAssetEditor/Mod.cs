using ICities;

namespace ExtendedAssetEditor
{
    public class Mod : IUserMod
    {
        public const string name = "Extended Asset Editor";
        public const string versionString = "0.3";

        public string Description
        {
            get
            {
                return "Version " + versionString + ". Adds various features and fixes for dealing with vehicles in the Asset Editor.";
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public static bool IsValidLoadMode(LoadMode mode)
        {
            return (mode == LoadMode.LoadAsset || mode == LoadMode.NewAsset);
        }
    }
}
