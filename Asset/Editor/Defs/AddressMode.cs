namespace NBC.Asset.Editor
{
    public enum AddressMode
    {
        /// <summary>
        /// 不启用寻址
        /// </summary>
        [DisplayName(Language.AddressModeNone)] None,

        /// <summary>
        /// 文件名
        /// </summary>
        [DisplayName(Language.AddressModeFileName)]
        FileName,

        /// <summary>
        /// 分组名+文件名
        /// </summary>
        [DisplayName(Language.AddressModeGroupAndFileName)]
        GroupAndFileName,

        /// <summary>
        /// 收集源名称+文件名
        /// </summary>
        [DisplayName(Language.AddressModeCollectorAndFileName)]
        CollectorAndFileName,

        /// <summary>
        /// 包名+组名+文件名
        /// </summary>
        [DisplayName(Language.AddressModePackAndGroupAndFileName)]
        PackAndGroupAndFileName,

        /// <summary>
        /// 包名+文件名
        /// </summary>
        [DisplayName(Language.AddressModePackAndFileName)]
        PackAndFileName,

        /// <summary>
        /// 包名+收集源名称+文件名
        /// </summary>
        [DisplayName(Language.AddressModePackAndCollectorAndFileName)]
        PackAndCollectorAndFileName,
    }
}