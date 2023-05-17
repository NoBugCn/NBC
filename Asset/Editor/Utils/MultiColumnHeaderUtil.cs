using UnityEditor.IMGUI.Controls;

namespace NBC.Asset.Editor
{
    public static class MultiColumnHeaderUtil
    {
        /// <summary>
        /// 自动大小
        /// </summary>
        /// <param name="headerState"></param>
        /// <param name="maxWidth">最大宽</param>
        /// <param name="index">自由大小的序号</param>
        public static void AutoWidth(this MultiColumnHeaderState headerState, float maxWidth, int index = 0)
        {
            var columns = headerState.columns;
            if (columns == null) return;
            var residue = maxWidth - 16;
            for (int i = 0; i < columns.Length; i++)
            {
                var column = columns[i];
                if (i != index)
                {
                    residue -= column.width;
                }
            }

            if (residue < 100) residue = 100;
            columns[index].width = residue;
        }
    }
}