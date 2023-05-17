using System;

namespace NBC.Asset
{
    public class RunFunctionTask : NTask
    {
        private readonly Action _action;

        public RunFunctionTask(Action action)
        {
            _action = action;
        }

        protected override void OnStart()
        {
            _action?.Invoke();
            Finish();
        }
    }
}