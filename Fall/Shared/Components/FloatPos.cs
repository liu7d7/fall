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

    public float_pos() : base(type.FLOAT_POS)
    {
      X = PrevX = Y = PrevY = Z = PrevZ = Yaw = PrevYaw = Pitch = PrevPitch = 0;
    }

    public float LerpedX => util.Lerp(PrevX, X, ticker.TickDelta);
    public float LerpedY => util.Lerp(PrevY, Y, ticker.TickDelta);
    public float LerpedZ => util.Lerp(PrevZ, Z, ticker.TickDelta);
    public float LerpedYaw => util.Lerp(PrevYaw, Yaw, ticker.TickDelta);
    public float LerpedPitch => util.Lerp(PrevPitch, Pitch, ticker.TickDelta);

    public Vector3 to_vector3()
    {
      return (X, Y, Z);
    }

    public void set_vector3(Vector3 pos)
    {
      (X, Y, Z) = pos;
    }

    public Vector3 to_lerped_vector3(float xOff = 0, float yOff = 0, float zOff = 0)
    {
      return (LerpedX + xOff, LerpedY + yOff, LerpedZ + zOff);
    }

    public void set_prev()
    {
      PrevX = X;
      PrevY = Y;
      PrevZ = Z;
      PrevYaw = Yaw;
      PrevPitch = Pitch;
    }
  }
}