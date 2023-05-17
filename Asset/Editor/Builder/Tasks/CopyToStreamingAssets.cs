using System.IO;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace NBC.Asset.Editor
{
    [Id(TaskId.CopyToStreamingAssets)]
    public class CopyToStreamingAssets : BuildTask
    {
        public override void Run(BuildContext context)
        {
            var last = HistoryUtil.GetLastVersionHistory();
            if (last != null)
            {
                last.CopyToStreamingAssets();
            }
            else
            {
                Debug.LogError("copy version is null,version history is null");
            }
        }
    }
}