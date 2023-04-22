namespace ExtendedAssetEditor
{
    internal static class PrefabInfoExtensions
    {
        private static PreviewCamera _previewCamera;

        /// <summary>
        /// Set the preview camera to use for GenerateThumbnails
        /// </summary>
        public static void SetPreviewCamera(PreviewCamera camera)
        {
            _previewCamera = camera;
        }

        public static void GenerateThumbnails(this PrefabInfo prefab)
        {
            Util.Log($"Generating thumbnails for '{prefab.name}' ({prefab.gameObject})");
            if (_previewCamera == null)
            {
                Util.LogError("Preview camera not set, cannot generate thumbnails");
                return;
            }
            _previewCamera.target = prefab.gameObject;
            prefab.m_Thumbnail = null;
            prefab.m_Atlas = null;
            // Right, in here we at some point have some bugged code that thinks calling _previewCamera.m_InfoRenderer.Render()
            // will render instantly, even though it doesn't. This results in it just putting whatever was previously in the
            // camera's texture into the thumbnail.
            // TODO: So we either need to wait a frame, or create our own thing that renders instantly.
            AssetImporterThumbnails.CreateThumbnails(prefab.gameObject, null, _previewCamera);
        }
    }
}
