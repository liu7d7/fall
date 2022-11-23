using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
  public class collision : fall_obj.component
  {
    public bounding_cylinder Cylinder;
    public bool Movable;

    public override void Collide(fall_obj objIn, fall_obj other)
    {
      collision ocol = other.Get<collision>();
      Cylinder.BottomCenter = objIn.Pos;
      ocol.Cylinder.BottomCenter = other.Pos;
      switch (Cylinder.Intersects(ocol.Cylinder))
      {
        case 0:
          break;
        case 1:
        {
          Vector2 ratio = objIn.Pos.Xz - other.Pos.Xz;
          float diffLen = (objIn.Pos.Xz - other.Pos.Xz).Length;
          float targetLen = Cylinder.Radius + ocol.Cylinder.Radius;
          float amt = targetLen - diffLen;
          ratio.Normalize();
          if (Movable && ocol.Movable)
          {
            amt /= 2;
            objIn.Get<float_pos>().X += ratio.X * amt;
            objIn.Get<float_pos>().Z += ratio.Y * amt;
            other.Get<float_pos>().X -= ratio.X * amt;
            other.Get<float_pos>().Z -= ratio.Y * amt;
          }
          else if (Movable)
          {
            objIn.Get<float_pos>().X += ratio.X * amt;
            objIn.Get<float_pos>().Z += ratio.Y * amt;
          }
          else if (ocol.Movable)
          {
            other.Get<float_pos>().X -= ratio.X * amt;
            other.Get<float_pos>().Z -= ratio.Y * amt;
          }

          break;
        }
        // collides with other's top
        case 2:
        {
          float diffLen = other.Pos.Y + ocol.Cylinder.Height - objIn.Pos.Y;
          float targetLen = ocol.Cylinder.Height;
          float amt = targetLen - diffLen;
          if (Movable && ocol.Movable)
          {
            amt /= 2;
            objIn.Get<float_pos>().Y += amt;
            other.Get<float_pos>().Y -= amt;
          }
          else if (Movable)
          {
            objIn.Get<float_pos>().Y += amt;
          }
          else if (ocol.Movable)
          {
            other.Get<float_pos>().Y -= amt;
          }

          break;
        }
        // collides with other's bottom
        case 3:
        {
          float diffLen = objIn.Pos.Y + Cylinder.Height - other.Pos.Y;
          float targetLen = Cylinder.Height;
          float amt = targetLen - diffLen;
          if (Movable && ocol.Movable)
          {
            amt /= 2;
            objIn.Get<float_pos>().Y -= amt;
            other.Get<float_pos>().Y += amt;
          }
          else if (Movable)
          {
            objIn.Get<float_pos>().Y -= amt;
          }
          else if (ocol.Movable)
          {
            other.Get<float_pos>().Y += amt;
          }

          break;
        }
      }
    }
  }
}