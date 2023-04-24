using System.Collections;
using UnityEngine;

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

        public static IEnumerator GenerateThumbnailsCoroutine(this PrefabInfo prefab)
        {
            Util.Log($"Generating thumbnails for '{prefab.name}' ({prefab.gameObject})");
            if (_previewCamera == null)
            {
                Util.LogError("Preview camera not set, cannot generate thumbnails");
                yield break;
            }
            // Set up the preview camera
            _previewCamera.target = prefab.gameObject;
            // Wait until the end of the frame. The camera will have rendered the target in LateUpdate().
            yield return new WaitForEndOfFrame();
            // Wait another frame so we are sure that the previous target is not also rendered to the image.
            yield return new WaitForEndOfFrame();

            // Now clean the existing thumbnails (should not be necessary, but will be a good indicator if creating new ones failed)
            prefab.m_Thumbnail = null;
            prefab.m_Atlas = null;
            // Right, in here we at some point have some bugged code that thinks calling _previewCamera.m_InfoRenderer.Render()
            // will render instantly, even though it doesn't. This results in it just putting whatever was previously in the
            // camera's texture into the thumbnail.
            // Because we have already ensured that the preview camera rendered our desired target everything should be OK.
            AssetImporterThumbnails.CreateThumbnails(prefab.gameObject, null, _previewCamera);
        }
    }
}
