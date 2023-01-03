namespace Fall.Shared
{
  public static class math
  {
    public static float FastAtan2(float y, float x)
    {
      float t3 = MathF.Abs(x);
      float t1 = MathF.Abs(y);
      float t0 = MathF.Max(t3, t1);
      t1 = MathF.Min(t3, t1);
      t3 = 1.0f / t0 * t1;

      float t4 = t3 * t3;
      t0 = -0.013480470f;
      t0 = MathF.FusedMultiplyAdd(t0, t4, 0.057477314f);
      t0 = MathF.FusedMultiplyAdd(t0, t4, -0.121239071f);
      t0 = MathF.FusedMultiplyAdd(t0, t4, 0.195635925f);
      t0 = MathF.FusedMultiplyAdd(t0, t4, -0.332994597f);
      t0 = MathF.FusedMultiplyAdd(t0, t4, 0.999995630f);
      t3 = t0 * t3;

      if (MathF.Abs(y) > MathF.Abs(x)) t3 = MathF.FusedMultiplyAdd(MathF.PI, 0.5f, -t3);

      if (x < 0) t3 = MathF.PI - t3;

      if (y < 0) return -t3;

      return t3;
    }

    public static float CalcAngle(float v, float h)
    {
      return FastAtan2(v, h).Deg();
    }

    public static float CalcAngleXz(fall_obj eye, fall_obj target)
    {
      return CalcAngle(target.Pos.Z - eye.Pos.Z, target.Pos.X - eye.Pos.X);
    }

    public static float WrapDegrees(float degrees)
    {
      float f = degrees % 360f;
      if (f >= 180f)
        f -= 360f;
      else if (f < -180f) f += 360f;
      return f;
    }

    public static readonly float SQRT2 = MathF.Sqrt(2);

    public static float Lerp(float start, float end, float delta)
    {
      return start + (end - start) * delta;
    }

    public static void Clamp(ref int val, int start, int end)
    {
      val = Math.Min(Math.Max(val, start), end);
    }
  }
}