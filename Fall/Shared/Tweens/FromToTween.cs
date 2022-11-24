using OpenTK.Mathematics;

namespace Fall.Shared.Tweens
{
  public class from_to_tween : base_tween
  {
    public animations.animation Animation;
    public float From;
    public float To;

    public from_to_tween(animations.animation animation, float from, float to, float duration)
    {
      Animation = animation;
      LastActivation = Environment.TickCount;
      From = from;
      To = to;
      Duration = duration;
    }

    public override float Output()
    {
      if (Environment.TickCount < LastActivation) return From;

      if (Environment.TickCount > LastActivation + Duration) return To;

      return MathHelper.Lerp(From, To, Animation(Duration, Environment.TickCount - LastActivation));
    }

    public override float OutputAt(float time)
    {
      if (time < LastActivation) return From;

      if (time > LastActivation + Duration) return To;

      return MathHelper.Lerp(From, To, Animation(Duration, time - LastActivation));
    }

    public override bool Done()
    {
      return Environment.TickCount - LastActivation > Duration;
    }
  }
}