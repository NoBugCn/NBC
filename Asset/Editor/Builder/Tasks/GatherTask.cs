using System;
using System.Linq;

namespace NBC.Asset.Editor
{
    [Id(TaskId.Gather)]
    public class GatherTask : BuildTask
    {
        private BuildContext _context;

        public override void Run(BuildContext context)
        {
            _context = context;
            GatherAllAssets();
            _context.SaveAssets();
        }

        public void GatherAllAssets()
        {
            var packages = CollectorSetting.Instance.Packages;

            foreach (var package in packages)
            {
                if (!package.Enable) continue;
                foreach (var group in package.Groups)
                {
                    if (group.FilterEnum != FilterEnum.Custom)
                    {
                        group.Filter = group.FilterEnum == FilterEnum.All ? "*" : $"t:{group.FilterEnum}";
                    }
                    else if (string.IsNullOrEmpty(group.Filter))
                    {
                        group.Filter = "*";
                    }

                    if (!group.Enable) continue;
                    var assets = GatherGroupAssets(package, group);
                    _context.Add(group, assets.ToList());
                }
            }

            foreach (var package in packages)
            {
                foreach (var group in package.Groups)
                {
                    var tag = group.Tags;
                    if (string.IsNullOrEmpty(tag)) continue;
                    var arr = tag.Split(",");
                    foreach (var s in arr)
                    {
                        if (!string.IsNullOrEmpty(s))
                        {
                            Defs.AddTag(s);
                        }
                    }
                }
            }
        }

        private BuildAsset[] GatherGroupAssets(PackageConfig packageConfig, GroupConfig groupConfig)
        {
            if (groupConfig.Collectors == null || groupConfig.Collectors.Count == 0) return Array.Empty<BuildAsset>();
            return Builder.GatherAsset(packageConfig, groupConfig);
        }
    }
}