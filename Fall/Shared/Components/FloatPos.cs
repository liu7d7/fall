using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
  public class float_pos : fall_obj.component
  {
    public float Pitch;
    public float PrevPitch;
    public float PrevX;
    public float PrevY;
    public float PrevYaw;
    public float PrevZ;
    public float X;
    public float Y;
    public float Yaw;
    public float Z;

    public float_pos() : base(fall_obj.comp_type.FloatPos)
    {
      X = PrevX = Y = PrevY = Z = PrevZ = Yaw = PrevYaw = Pitch = PrevPitch = 0;
    }

    public float LerpedX => math.Lerp(PrevX, X, ticker.TickDelta);
    public float LerpedY => math.Lerp(PrevY, Y, ticker.TickDelta);
    public float LerpedZ => math.Lerp(PrevZ, Z, ticker.TickDelta);
    public float LerpedYaw => math.Lerp(PrevYaw, Yaw, ticker.TickDelta);
    public float LerpedPitch => math.Lerp(PrevPitch, Pitch, ticker.TickDelta);

    public static float_pos Get(fall_obj obj)
    {
      return obj.Get<float_pos>(fall_obj.comp_type.FloatPos);
    }

    public Vector3 ToVec3()
    {
      return (X, Y, Z);
    }

    public void SetVec3(Vector3 pos)
    {
      (X, Y, Z) = pos;
    }

    public void IncVec3(Vector3 inc)
    {
      X += inc.X;
      Y += inc.Y;
      Z += inc.Z;
    }

    public Vector3 ToLerpedVec3(float xOff = 0, float yOff = 0, float zOff = 0)
    {
      return (LerpedX + xOff, LerpedY + yOff, LerpedZ + zOff);
    }

    public void SetPrev()
    {
      PrevX = X;
      PrevY = Y;
      PrevZ = Z;
      PrevYaw = Yaw;
      PrevPitch = Pitch;
    }
  }
}