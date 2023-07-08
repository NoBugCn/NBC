using System;
using System.Diagnostics;
using System.Globalization;

namespace NBC.Asset
{
    public abstract class ProviderBase : NTask, IRecyclable
    {
        /// <summary>
        /// 是否已经销毁
        /// </summary>
        public bool IsDestroyed { get; set; }

        /// <summary>
        /// 是否可以销毁
        /// </summary>
        public bool CanDestroy => IsDone && RefCount <= 0;

        protected bool IsWaitForAsyncComplete { private set; get; } = false;

        public void SetStatus(TaskStatus status, string info = "")
        {
            Status = status;
            _errorMsg = info;
        }

        internal virtual void Run()
        {
            Retain();
            if (!IsDone && !IsRunning)
            {
                Run(TaskRunner.ProviderRunner);
            }
        }

        #region RefCounter

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { get; private set; }

        public void Retain()
        {
            RefCount++;
        }

        public void Release(bool force = false)
        {
            RefCount--;
            if (force) RefCount = 0;
            if (RefCount > 0) return;
            //释放资源
            Recycler.Add(this);
        }

        #endregion

        /// <summary>
        /// 销毁资源对象
        /// </summary>
        public virtual void Destroy()
        {
            IsDestroyed = true;
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            IsWaitForAsyncComplete = true;
            Process();
        }

        #region Debug

#if DEBUG
        public string LoadScene = string.Empty;
        public string LoadTime = string.Empty;
        public long LoadTotalTime { protected set; get; }

        // 加载耗时统计
        private bool _isRecording;
        private Stopwatch _watch;

        internal void InitDebugInfo()
        {
            LoadScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            LoadTime = DateTime.Now.ToString("hh:mm:ss");
        }

        protected void DebugRecord()
        {
            if (_isRecording == false)
            {
                _isRecording = true;
                _watch = Stopwatch.StartNew();
            }

            if (_watch == null) return;
            if (!IsDone) return;
            LoadTotalTime = _watch.ElapsedMilliseconds;
            _watch = null;
        }
#endif

        #endregion
    }
}