using OpenTK.Mathematics;

namespace Fall.Shared
{
  public class bounding_cylinder
  {
    public Vector3 BottomCenter;
    public float Height;
    public float Radius;

    public int Intersects(bounding_cylinder other)
    {
      float diffLen = (other.BottomCenter - BottomCenter).Length;
      // dist to move out of collision @ other top
      float top = other.BottomCenter.Y + other.Height - BottomCenter.Y;
      // dist to move out of collision @ other bottom
      float bottom = BottomCenter.Y + Height - other.BottomCenter.Y;
      // dist to move out of collision @ other center
      float center = Radius + other.Radius - diffLen;
      // does not collide
      if (diffLen > Radius + other.Radius) return 0;
      // collides horizontally
      if (center < top && center < bottom) return 1;
      // collides vertically
      // collides at top
      if (top < bottom) return 2;
      // collides at bottom
      return 3;
    }
  }
}