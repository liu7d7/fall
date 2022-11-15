using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
    public class float_pos : fall_obj.component
    {
        public float x;
        public float prevX;
        public float lerped_x => util.lerp(prevX, x, ticker.tickDelta);
        public float y;
        public float prevY;
        public float lerped_y => util.lerp(prevY, y, ticker.tickDelta);
        public float z;
        public float prevZ;
        public float lerped_z => util.lerp(prevZ, z, ticker.tickDelta);
        public float yaw;
        public float prevYaw;
        public float lerped_yaw => util.lerp(prevYaw, yaw, ticker.tickDelta);
        public float pitch;
        public float prevPitch;
        public float lerped_pitch => util.lerp(prevPitch, pitch, ticker.tickDelta);
        
        public float_pos()
        {
            x = prevX = y = prevY = z = prevZ = yaw = prevYaw = pitch = prevPitch = 0;
        }

        public Vector3 to_vector3()
        {
            return (x, y, z);
        }
        
        public void set_vector3(Vector3 pos)
        {
            (x, y, z) = pos;
        }

        public Vector3 to_lerped_vector3(float xOff = 0, float yOff = 0, float zOff = 0)
        {
            return (lerped_x + xOff, lerped_y + yOff, lerped_z + zOff);
        }

        public void set_prev()
        {
            prevX = x;
            prevY = y;
            prevZ = z;
            prevYaw = yaw;
            prevPitch = pitch;
        }
    }
}