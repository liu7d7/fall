namespace Fall.Shared.Tweens
{
    public class static_tween : base_tween
    {
        private float _output;

        public static_tween(float output, float duration)
        {
            _output = output;
            base.duration = duration;
        }
        
        public override float output()
        {
            return _output;
        }

        public override float output_at(float time)
        {
            return _output;
        }

        public override bool done()
        {
            return Environment.TickCount - lastActivation > duration;
        }
    }
}