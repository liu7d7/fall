namespace Fall.Shared
{
    public static class ticker
    {
        public static float tickTime;
        public static float tickDelta;
        public static float lastFrame;
        private static long _start;
        private static float _prevTimeMs;

        public static void init()
        {
            _start = Environment.TickCount;
            tickTime = 50.0f;
        }

        public static int update()
        {
            float timeMillis = Environment.TickCount - _start;
            lastFrame = (timeMillis - _prevTimeMs) / tickTime;
            _prevTimeMs = timeMillis;
            tickDelta += lastFrame;
            int i = (int) tickDelta;
            tickDelta -= i;
            return i;
        }
    }
}