using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
  public class float_pos_static : fall_obj.component
  {
    public float X;
    public float Y;
    public float Z;

    public float_pos_static() : base(fall_obj.comp_type.FloatPosStatic)
    {
    }

    public static float_pos_static Get(fall_obj obj)
    {
      return obj.Get<float_pos_static>(fall_obj.comp_type.FloatPosStatic);
    }

    public Vector3 ToVec3()
    {
      return (X, Y, Z);
    }
  }
}