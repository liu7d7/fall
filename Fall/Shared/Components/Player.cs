using Fall.Engine;
using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
    public class player : fall_obj.component
    {
        private static readonly model3d _icosphere;
        private readonly mesh _cape;
        private static readonly shader _capeShader;

        static player()
        {
            _icosphere = model3d.read("icosphere", new Dictionary<string, uint>());
            _capeShader = new shader("Resource/Shader/cape.vert", "Resource/Shader/basic.frag");
        }

        private readonly Color4 _color;

        public player()
        {
            _color = colors.get_random_color4();
            _cape = new mesh(mesh.draw_mode.triangle, _capeShader, false, vao.attrib.float3, vao.attrib.float2);
            make_cape(0);
            
        }

        private void make_cape(float yaw)
        {
            _cape.begin();
            const int segments = 16;
            for (float y = 5.66f; y > 1.66f; y -= 0.33f)
            {
                for (int x = -segments; x < segments; x++)
                {
                    const float inc = 40f / segments;
                    float angle = x * inc + yaw;
                    const float rad = 0.33f;
                    float dx = MathF.Cos(angle.to_radians()) * rad;
                    float dz = MathF.Sin(angle.to_radians()) * rad;
                    float dx1 = MathF.Cos((angle + inc).to_radians()) * rad;
                    float dz1 = MathF.Sin((angle + inc).to_radians()) * rad;
                    _cape.quad(
                        _cape.float3(dx, y, dz).float2(x, y).next(),
                        _cape.float3(dx1, y, dz1).float2(x + 1, y).next(),
                        _cape.float3(dx1, y - 0.5f, dz1).float2(x + 1, y - 0.5f).next(),
                        _cape.float3(dx, y - 0.5f, dz).float2(x, y - 0.5f).next()
                    );
                }
            }

            _cape.end();
        }

        public override void render(fall_obj objIn)
        {
            base.render(objIn);

            float lyaw = objIn.get<float_pos>().lerped_yaw + 180;
            if (Math.Abs(objIn.get<float_pos>().prevYaw - objIn.get<float_pos>().yaw) > 0.0003)
            {
                make_cape(lyaw);
            }

            Vector3 offset = (-1, -2, -1);
            Vector3 renderPos = objIn.lerped_pos + offset;

            // render head
            render_system.push();
            render_system.translate(-renderPos - (0f, 5.25f, 0f));
            render_system.scale(0.66f);
            render_system.translate(renderPos + (0f, 5.25f, 0f));
            _icosphere.render(renderPos + (0f, 5.25f, 0f));
            render_system.pop();

            _capeShader.bind();
            _capeShader.set_vector3("_translation",
                renderPos + (1, 0, 1) + (-0.15f * MathF.Cos(lyaw.to_radians()), 0, -0.15f * MathF.Sin(lyaw.to_radians())));
            _capeShader.set_vector4("_color", _color.to_vector4());
            _cape.render();
        }
    }
}