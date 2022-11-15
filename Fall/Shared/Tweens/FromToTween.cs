using OpenTK.Mathematics;

namespace Fall.Shared.Tweens
{
    public class from_to_tween : base_tween
    {
        public animations.animation animation;
        public float from;
        public float to;

        public from_to_tween(animations.animation animation, float from, float to, float duration)
        {
            this.animation = animation;
            lastActivation = Environment.TickCount;
            this.from = from;
            this.to = to;
            base.duration = duration;
        }

        public override float output()
        {
            if (Environment.TickCount < lastActivation)
            {
                return from;
            }

            if (Environment.TickCount > lastActivation + duration)
            {
                return to;
            }
            
            return MathHelper.Lerp(from, to, animation(duration, Environment.TickCount - lastActivation));
        }

        public override float output_at(float time)
        {
            if (time < lastActivation)
            {
                return from;
            }

            if (time > lastActivation + duration)
            {
                return to;
            }

            return MathHelper.Lerp(from, to, animation(duration, time - lastActivation));
        }

        public override bool done()
        {
            return Environment.TickCount - lastActivation > duration;
        }
    }
}