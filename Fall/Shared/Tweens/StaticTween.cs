namespace Fall.Shared.Tweens
{
  public class static_tween : base_tween
  {
    private readonly float _output;

    public static_tween(float output, float duration)
    {
      _output = output;
      Duration = duration;
    }

    public override float Output()
    {
      return _output;
    }

    public override float output_at(float time)
    {
      return _output;
    }

    public override bool Done()
    {
      return Environment.TickCount - LastActivation > Duration;
    }
  }
}