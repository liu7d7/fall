using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
    public class collision : fall_obj.component
    {
        public bounding_cylinder cylinder;
        public bool movable;

        public override void collide(fall_obj objIn, fall_obj other)
        {
            collision ocol = other.get<collision>();
            cylinder.bottomCenter = objIn.pos;
            ocol.cylinder.bottomCenter = other.pos;
            switch (cylinder.intersects(ocol.cylinder))
            {
                case 0:
                    break;
                case 1:
                {
                    Vector2 ratio = objIn.pos.Xz - other.pos.Xz;
                    float diffLen = (objIn.pos.Xz - other.pos.Xz).Length;
                    float targetLen = cylinder.radius + ocol.cylinder.radius;
                    float amt = targetLen - diffLen;
                    ratio.Normalize();
                    if (movable && ocol.movable)
                    {
                        amt /= 2;
                        objIn.get<float_pos>().x += ratio.X * amt;
                        objIn.get<float_pos>().z += ratio.Y * amt;
                        other.get<float_pos>().x -= ratio.X * amt;
                        other.get<float_pos>().z -= ratio.Y * amt;
                    }
                    else if (movable)
                    {
                        objIn.get<float_pos>().x += ratio.X * amt;
                        objIn.get<float_pos>().z += ratio.Y * amt;
                    }
                    else if (ocol.movable)
                    {
                        other.get<float_pos>().x -= ratio.X * amt;
                        other.get<float_pos>().z -= ratio.Y * amt;
                    }
                    break;
                }
                // collides with other's top
                case 2:
                {
                    float diffLen = other.pos.Y + ocol.cylinder.height - objIn.pos.Y;
                    float targetLen = ocol.cylinder.height;
                    float amt = targetLen - diffLen;
                    if (movable && ocol.movable)
                    {
                        amt /= 2;
                        objIn.get<float_pos>().y += amt;
                        other.get<float_pos>().y -= amt;
                    }
                    else if (movable)
                    {
                        objIn.get<float_pos>().y += amt;
                    }
                    else if (ocol.movable)
                    {
                        other.get<float_pos>().y -= amt;
                    }
                    break;
                }
                // collides with other's bottom
                case 3:
                {
                    float diffLen = objIn.pos.Y + cylinder.height - other.pos.Y;
                    float targetLen = cylinder.height;
                    float amt = targetLen - diffLen;
                    if (movable && ocol.movable)
                    {
                        amt /= 2;
                        objIn.get<float_pos>().y -= amt;
                        other.get<float_pos>().y += amt;
                    }
                    else if (movable)
                    {
                        objIn.get<float_pos>().y -= amt;
                    }
                    else if (ocol.movable)
                    {
                        other.get<float_pos>().y += amt;
                    }
                    break;
                }
            }
        }
    }
}