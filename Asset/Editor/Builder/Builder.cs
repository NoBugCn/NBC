using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NBC.Asset.Editor
{
    public class Builder
    {
        private static readonly Dictionary<BundleMode, Type> _gatherTypes = new Dictionary<BundleMode, Type>();
        private static readonly Dictionary<int, Type> _buildTaskTypes = new Dictionary<int, Type>();

        #region 内部方法

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            FindGathers();
            FindTasks();
        }

        private static void FindGathers()
        {
            var types = EditUtil.FindAllSubclass<GatherBase>();
            foreach (var t in types)
            {
                var attributes = t.GetCustomAttributes(typeof(BindAttribute), true);
                foreach (var attribute in attributes)
                {
                    if (attribute is BindAttribute bindAttribute && bindAttribute.BindObject is BundleMode mode)
                    {
                        _gatherTypes[mode] = t;
                    }
                }
            }
        }

        private static void FindTasks()
        {
            var types = EditUtil.FindAllSubclass<BuildTask>();
            foreach (var t in types)
            {
                var attributes = t.GetCustomAttributes(typeof(IdAttribute), true);
                foreach (var attribute in attributes)
                {
                    if (attribute is IdAttribute idAttribute && idAttribute.Id > 0)
                    {
                        _buildTaskTypes[idAttribute.Id] = t;
                    }
                }
            }
        }

        private static Type GetGatherType(BundleMode mode)
        {
            return _gatherTypes.TryGetValue(mode, out var t) ? t : null;
        }

        private static Type GetTaskType(int id)
        {
            return _buildTaskTypes.TryGetValue(id, out var t) ? t : null;
        }

        #endregion

        internal static BuildAsset[] GatherAsset(PackageConfig packageConfig, GroupConfig groupConfig)
        {
            var type = GetGatherType(groupConfig.BundleMode);
            if (type != null)
            {
                if (Activator.CreateInstance(type) is GatherBase instance)
                {
                    return instance.Run(packageConfig, groupConfig);
                }
            }

            return Array.Empty<BuildAsset>();
        }

        public static void Gather()
        {
            RunBuildTasks(TaskId.Gather);
        }
        
        
        public static void Build()
        {
            RunBuildTasks(
                TaskId.Gather,
                TaskId.BuildBundle,
                TaskId.GenPackageData,
                TaskId.GenVersionData,
                TaskId.CopyVersionBundle,
                TaskId.CopyToStreamingAssets
            );
            
            // TaskId.GenVersionData,
            // TaskId.CopyVersionBundle,
            // TaskId.CopyToStreamingAssets
        }

        public static void RunBuildTasks(params int[] ids)
        {
            if (ids.Length < 1)
            {
                throw new Exception("not set task id!");
            }

            List<Type> types = ids.Select(GetTaskType).Where(type => type != null).ToList();
            if (types.Count < 1)
            {
                throw new Exception("task id error!");
            }

            List<BuildTask> buildTasks = new List<BuildTask>();
            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is BuildTask task)
                {
                    buildTasks.Add(task);
                }
            }

            BuildContext buildContext = new BuildContext();
            foreach (var task in buildTasks)
            {
                var sw = new Stopwatch();
                sw.Start();
                task.Run(buildContext);
                sw.Stop();
                Debug.Log($"Run {task.GetType().Name} time={sw.ElapsedMilliseconds / 1000f}s");
            }
        }
    }
}