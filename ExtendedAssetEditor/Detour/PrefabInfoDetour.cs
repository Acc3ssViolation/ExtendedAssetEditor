namespace ExtendedAssetEditor.Detour
{
    /// <summary>
    /// Detours PrefabInfo methods
    /// </summary>
    public class PrefabInfoDetour : IDetour
    {
        private readonly DetourItem _decorationAreaDetour;

        public PrefabInfoDetour()
        {
            var original = typeof(PrefabInfo).GetMethod("GetDecorationArea");
            var replacement = GetType().GetMethod("GetDecorationArea");
            _decorationAreaDetour = new DetourItem("PrefabInfo.GetDecorationArea", original, replacement);
        }

        public void Deploy()
        {
            _decorationAreaDetour.Deploy();
        }

        public void Revert()
        {
            _decorationAreaDetour.Revert();
        }

        public virtual void GetDecorationArea(out int width, out int length, out float offset)
        {
            // Give larger width and length to give the camera a bit more room to move around in
            width = 400;
            length = 400;
            offset = 0f;
        }
    }
}
