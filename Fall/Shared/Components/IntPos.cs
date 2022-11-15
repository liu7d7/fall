using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
    public class int_pos : fall_obj.component
    {
        public int x;
        public int y;
        public int z;

        public int_pos()
        {
            
        }
        
        public int_pos(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public int_pos(Vector3i pos)
        {
            x = pos.X;
            y = pos.Y;
            z = pos.Z;
        }
    }
}