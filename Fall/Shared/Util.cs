namespace Fall.Shared
{
    public static class util
    {
        public static readonly float sqrt2 = MathF.Sqrt(2);
        
        public static float lerp(float start, float end, float delta)
        {
            return start + (end - start) * delta;
        }

        public static void clamp(ref int val, int start, int end)
        {
            val = Math.Min(Math.Max(val, start), end);
        }
    }
}