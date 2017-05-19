using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExtendedAssetEditor
{
    public class SnapshotBehaviour : MonoBehaviour
    {
        void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.S))
            {
                DoTheThing(SnapshotTool.SnapshotMode.Snapshot);
            }
            else if(Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.T))
            {
                DoTheThing(SnapshotTool.SnapshotMode.Thumbnail);
            }
            else if(Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.I))
            {
                DoTheThing(SnapshotTool.SnapshotMode.Infotooltip);
            }
        }

        void DoTheThing(SnapshotTool.SnapshotMode mode)
        {
            var currentTool = ToolsModifierControl.toolController.CurrentTool;
            SnapshotTool tool = ToolsModifierControl.SetTool<SnapshotTool>();
            if(tool != null)
            {
                var crhelper = CoroutineHelper.Create(() =>
                {
                    tool.Mode = mode;
                    int w = 644;
                    int h = 360;
                    if(mode == SnapshotTool.SnapshotMode.Thumbnail)
                    {
                        w = AssetImporterThumbnails.thumbWidth;
                        h = AssetImporterThumbnails.thumbHeight;
                    }
                    else if(mode == SnapshotTool.SnapshotMode.Infotooltip)
                    {
                        w = SnapshotTool.tooltipWidth;
                        h = SnapshotTool.tooltipHeight;
                    }
                    tool.StartShot(w, h, () => {
                        ToolsModifierControl.toolController.CurrentTool = currentTool;
                        Debug.Log("Snapshot taken");
                    });
                });

                crhelper.Run(0.5f);
            }
        } 
    }
}
