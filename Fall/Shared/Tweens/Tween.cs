using OpenTK.Mathematics;

namespace Fall.Shared.Tweens
{
  public class tween : base_tween
  {
    private readonly animations.animation _animation;

    public tween(animations.animation animation, float duration, bool repeating)
    {
      LastActivation = Environment.TickCount;
      _animation = animation;
      Infinite = repeating;
      Duration = duration;
    }
    public override float Output()
    {
      return MathHelper.Clamp(
        Infinite
          ? _animation(Duration, (Environment.TickCount - LastActivation) % Duration)
          : _animation(Duration, Environment.TickCount - LastActivation), 0, 1);
    }

    public override float OutputAt(float time)
    {
      if (time < LastActivation) return 0;

      if (time > LastActivation + Duration && !Infinite) return 1;

      return MathHelper.Clamp(
        Infinite
          ? _animation(Duration, (time - LastActivation) % Duration)
          : _animation(Duration, time - LastActivation), 0, 1);
    }

    public override bool Done()
    {
      return Environment.TickCount - LastActivation > Duration;
    }
  }
}