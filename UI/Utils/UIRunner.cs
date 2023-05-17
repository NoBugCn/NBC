namespace NBC
{
    internal static class UIRunner
    {
        #region Static

        public static readonly Runner Def = new Runner();
        
        public static void Update()
        {
            Def.Process();
        }

        #endregion
    }
}