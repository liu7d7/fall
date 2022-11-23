using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
  public class int_pos : fall_obj.component
  {
    public int X;
    public int Y;
    public int Z;

    public int_pos()
    {
    }

    public int_pos(int x, int y, int z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public int_pos(Vector3i pos)
    {
      X = pos.X;
      Y = pos.Y;
      Z = pos.Z;
    }
  }
}