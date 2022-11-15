namespace Fall.Shared
{
    public static class animations
    {
        public delegate float animation(float duration, float time);

        public static float ease_in_out(float duration, float time)
        {
            float x1 = time / duration;
            return 6 * MathF.Pow(x1, 5) - 15 * MathF.Pow(x1, 4) + 10 * MathF.Pow(x1, 3);
        }

        public static float decelerate(float duration, float time)
        {
            float x1 = time / duration;
            return 1 - (x1 - 1) * (x1 - 1);
        }
        
        public static float accelerate(float duration, float time)
        {
            float x1 = time / duration;
            return (x1 - 1) * (x1 - 1);
        }

        public static animation step(animation func, float step)
        {
            return (duration, time) => func(duration, (int) (time / step) * step);
        }
        
        public static animation up_and_down(animation func)
        {
            return (duration, time) => time > duration / 2 ? func(duration / 2, duration - time) : func(duration / 2, time);
        }

        public static animation back_half_full_front_half(animation func)
        {
            return (duration, time) => time < duration / 4f ? 1 - func(duration / 2f, time + duration / 4f) : time < duration * 3f / 4 ? func(duration / 2, time - duration / 4) : 1 - func(duration / 2, time - duration / 4 * 3);
        }
    }
}