namespace NBC.Asset.Editor
{
    public enum BundleMode
    {
        /// <summary>
        /// 一个ab
        /// </summary>
        [DisplayName(Language.BundleModeSingle)]
        Single,

        /// <summary>
        /// 按文件夹（每个独立文件夹为一个ab包）
        /// </summary>
        [DisplayName(Language.BundleModeFolder)]
        Folder,

        /// <summary>
        /// 父级文件夹
        /// </summary>
        [DisplayName(Language.BundleModeFolderParent)]
        FolderParent,

        /// <summary>
        /// 每个文件一个ab
        /// </summary>
        [DisplayName(Language.BundleModeFile)]
        File,

        /// <summary>
        /// 忽略子文件夹
        /// </summary>
        [DisplayName(Language.BundleModeWithoutSub)]
        WithoutSub
    }
}