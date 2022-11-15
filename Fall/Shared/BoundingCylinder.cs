using OpenTK.Mathematics;

namespace Fall.Shared
{
    public class bounding_cylinder
    {
        public float radius;
        public float height;
        public Vector3 bottomCenter;

        public int intersects(bounding_cylinder other)
        {
            float diffLen = (other.bottomCenter - bottomCenter).Length;
            // dist to move out of collision @ other top
            float top = other.bottomCenter.Y + other.height - bottomCenter.Y;
            // dist to move out of collision @ other bottom
            float bottom = bottomCenter.Y + height - other.bottomCenter.Y;
            // dist to move out of collision @ other center
            float center = radius + other.radius - diffLen;
            // does not collide
            if (diffLen > radius + other.radius)
            {
                return 0;
            }
            // collides horizontally
            if (center < top && center < bottom)
            {
                return 1;
            }
            // collides vertically
            // collides at top
            if (top < bottom)
            {
                return 2;
            }
            // collides at bottom
            return 3;
        }
    }
}