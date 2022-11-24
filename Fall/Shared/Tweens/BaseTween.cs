namespace Fall.Shared.Tweens
{
  public abstract class base_tween
  {
    public float Duration;
    public bool Infinite = false;
    public float LastActivation = Environment.TickCount;

    public abstract float Output();
    public abstract float OutputAt(float time);
    public abstract bool Done();
  }
}