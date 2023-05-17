using System.Collections.Generic;

namespace NBC.Asset
{
    public interface IRecyclable
    {
        bool IsDestroyed { get; }
        bool CanDestroy { get; }
        void Destroy();
    }

    /// <summary>
    /// 资源回收器
    /// </summary>
    public static class Recycler
    {
        /// <summary>
        /// 当前运行的回收任务
        /// </summary>
        static readonly List<IRecyclable> Coroutines = new List<IRecyclable>();

        /// <summary>
        /// 准备要运行的回收任务
        /// </summary>
        static readonly List<IRecyclable> ReadyTask = new List<IRecyclable>();


        public static void Add(IRecyclable recyclable)
        {
            ReadyTask.Add(recyclable);
        }

        /// <summary>
        /// 取消回收
        /// </summary>
        /// <param name="recyclable"></param>
        public static void Cancel(IRecyclable recyclable)
        {
            ReadyTask.Remove(recyclable);
        }

        public static void Update()
        {
            //正在加载时，不卸载资源
            if (TaskRunner.ProviderRunner.RunningTaskNum > 0) return;
            for (var i = 0; i < ReadyTask.Count; i++)
            {
                var task = ReadyTask[i];
                if (!task.CanDestroy) continue;
                ReadyTask.RemoveAt(i);
                Coroutines.Add(task);
                i--;
            }

            for (var i = 0; i < Coroutines.Count; i++)
            {
                var task = Coroutines[i];
                Coroutines.RemoveAt(i);
                i--;
                if (task.IsDestroyed) continue;
                if (task.CanDestroy) task.Destroy();
            }
        }
    }
}