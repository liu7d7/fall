namespace Fall.Shared.Tweens
{
    public abstract class base_tween
    {
        public float duration;
        public float lastActivation = Environment.TickCount;
        public bool infinite = false;
        
        public abstract float output();
        public abstract float output_at(float time);
        public abstract bool done();
    }
}