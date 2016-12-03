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
                SnapshotTool tool = ToolsModifierControl.GetTool<SnapshotTool>();
                if(tool != null)
                {
                    tool.StartShot(644, 360, () => {
                        Debug.Log("Snapshot taken");
                    });
                }
            }
        }
    }
}
