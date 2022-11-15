using OpenTK.Mathematics;

namespace Fall.Engine
{
    public class vertex_data
    {
        public Vector3 pos;
        public Vector3 normal;
        public Vector2 uv;
        public uint color = 0xffffffff;
        
        public vertex_data(Vector3 pos, Vector2 uv)
        {
            this.pos = pos;
            this.uv = uv;
        }
    }
}