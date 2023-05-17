namespace NBC.Asset.Editor
{
    public static class Language
    {
        public const string AssetPackageName = "资源包";
        public const string AddPackage = "添加资源包";
        public const string EnableDefPackage = "设为默认包";
        public const string DisableDefPackage = "设为附加包";

        public const string Enable = "启用";
        public const string Disable = "禁用";

        public const string AssetGroupName = "资源组";
        public const string AddGroup = "添加资源组";

        public const string Del = "删除";
        public const string ReName = "重命名";
        public const string Success = "成功";
        public const string Tips = "提示";
        public const string Confirm = "确定";
        public const string Error = "错误";

        public const string AddressPath = "寻址地址";
        public const string FileSize = "文件大小";
        public const string Path = "真实地址";
        public const string CopyPath = "拷贝真实地址";
        public const string CopyAddressPath = "拷贝寻址地址";


        public const string NoSelectGroup = "没有选中的资源组";
        public const string GroupBundleMode = "打包模式";
        public const string GroupAddressMode = "寻址模式";
        public const string Tags = "资源标签";
        public const string Filter = "过滤器规则";
        public const string FilterCustom = "自定义过滤器规则";
        public const string CollectorTitle = "收集源{0}";

        public const string RepetitiveName = "重复的名字";

        public const string Save = "保存";
        public const string Build = "构建";
        public const string Tools = "工具";
        public const string Profiler = "资源监视";
        public const string Analyse = "资源分析";

        public const string CollectorWindowName = "资源收集器";
        public const string ProfilerWindowName = "资源监视器";
        public const string BuilderWindowName = "资源构建器";
        public const string HistoryWindowName = "历史记录操作器";

        public const string CollectorWindowMenuPath = "NBC/资源系统/资源收集器";
        public const string ProfilerWindowNameMenuPath = "NBC/资源系统/资源监视器";
        public const string BuilderWindowNameMenuPath = "NBC/资源系统/资源构建器";
        public const string HistoryWindowNameMenuPath = "NBC/资源系统/历史记录操作器";
        
        public const string MenuDownloadPath = "NBC/资源系统/目录/下载解压目录";
        public const string MenuBuildPath = "NBC/资源系统/目录/构建目录";

        public const string AddressModeNone = "不启用";
        public const string AddressModeFileName = "文件名";
        public const string AddressModeGroupAndFileName = "分组名+文件名";
        public const string AddressModeCollectorAndFileName = "收集源名称+文件名";
        public const string AddressModePackAndGroupAndFileName = "包名+组名+文件名";
        public const string AddressModePackAndFileName = "包名+文件名";
        public const string AddressModePackAndCollectorAndFileName = "包名+收集源名称+文件名";

        public const string BundleModeSingle = "打包为一个AB";
        public const string BundleModeFolder = "每个独立文件夹为一个AB";
        public const string BundleModeFolderParent = "每个源文件夹为一个AB";
        public const string BundleModeFile = "每个文件一个AB";
        public const string BundleModeWithoutSub = "打包为一个AB且忽略子文件夹";

        public const string LabelHintIdle = "输入筛选或添加新标签";
        public const string LabelHintSearchFoundIsEnabled = "<b>回车</b> 增加 '{0}'";
        public const string LabelHintSearchFoundIsDisabled = "<b>回车</b> 删除 '{0}'";

        public const string InspectorUIAddressPath = "寻址路径: ";
        public const string InspectorUIPath = "真实路径: ";
        public const string InspectorUITitle = "文件可寻址信息";
        public const string InspectorUITitleNotOpen = "文件可寻址信息(未开启)";
        public const string Copy = "复制";

        public const string ProfilerShowMode = "显示模式:";
        public const string ProfilerAssetMode = "资源";
        public const string ProfilerBundleMode = "资源包";
        public const string ImportProfiler = "导入";
        public const string ImportProfilerTips = "选择要导入文件";
        public const string BuildProfiler = "导出";
        public const string BuildProfilerTips = "选择要导出的目录";
        public const string Current = "采样";
        public const string Clear = "清理";
        public const string CurrentFrame = "当前帧：";
        public const string PrevFrame = "|上一帧";
        public const string NextFrame = "|下一帧";
        public const string ProfilerAssetPath = "资源路径";
        public const string ProfilerAssetType = "资源类型";
        public const string ProfilerLoadTime = "加载时间";
        public const string ProfilerStatus = "状态";
        public const string ProfilerLoadTotalTime = "加载耗时";
        public const string ProfilerLoadScene = "加载场景";
        public const string ProfilerRefCount = "引用数量";
        public const string ProfilerBundleName = "Bundle包名";
        public const string ProfilerDataMode = "数据源:";
        public const string ProfilerLocalData = "本地";
        public const string ProfilerRemoteData = "远程";
        public const string ProfilerRemoteUrl = "远程连接地址:";
        public const string ProfilerRemoteUrlIsNull = "远程连接地址未设置";


        public const string BuildBundleName = "构建AB包";
        public const string BuildStart = "立即构建";
        public const string BuildSuccessTips = "构建成功。热更内容大小：{0}，新增Bundle：{1}，修改Bunlde：{2}，删除Bundle：{3}";

        public const string HistoryVersionName = "历史打包版本";
        public const string HistoryBundleName = "资源包名";
        public const string HistoryHash = "资源哈希";
        public const string HistorySize = "大小";
        public const string HistoryDelete = "删除记录";
        public const string HistorySelectCompare = "选中为比较版本";
        public const string HistoryUnSelectCompare = "取消比较选中";
        public const string HistoryCompareBuild = "保存对比结果清单";
        public const string NoSelectHistoryCompareVersion = "无法显示比较信息：没有选中需要比较的旧版本，请在左侧侧栏中右键菜单选择要进行比较的旧版本";
        public const string NoSelectHistoryCompareOneVersion = "选中版本与当前版本一致";
        public const string HistoryBundleChangeTitle = "总数：{0} ({1})";
        public const string HistoryAddBundleCount = "新增的Bundle数";
        public const string HistoryChangeBundleCount = "修改的Bundle数";
        public const string HistoryRemoveBundleCount = "删除的Bundle包";
        public const string HistoryDownloadSize = "更新下载大小";
        public const string HistoryUse = "拷贝该版本到StreamingAssets";
        public const string HistoryCopyToFolder = "导出该版本到文件夹";
        public const string Refresh = "刷新";
    }
}