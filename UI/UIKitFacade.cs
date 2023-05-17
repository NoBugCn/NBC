namespace NBC
{
    public class UI 
    {
        public static UIManager _inst;

        public static UIManager Inst
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new UIManager();
                    _inst.Start();
                }

                return _inst;
            }
        }
    }
}