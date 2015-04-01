namespace Test.Server
{
    public static class DataStore
    {
        private static int _n = 0;

        public static void SetNumber(int n)
        {
            _n = n;
        }

        public static int GetNumber()
        {
            return _n;
        }
    }
}
