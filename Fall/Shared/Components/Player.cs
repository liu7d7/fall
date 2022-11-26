using Fall.Engine;
using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
  public class player : fall_obj.component
  {
    private static readonly model3d _icosphere;
    private static readonly shader _capeShader;
    private readonly mesh _cape;

    private readonly Color4 _color;

    static player()
    {
      _icosphere = model3d.Read("icosphere", new Dictionary<string, uint>());
      _icosphere.Scale(0.66f);
      _capeShader = new shader("Resource/Shader/cape.vert", "Resource/Shader/basic.frag");
    }

    public player() : base(type.PLAYER)
    {
      _color = colors.NextColor();
      _cape = new mesh(mesh.draw_mode.TRIANGLE, _capeShader, true, vao.attrib.FLOAT2);
      _cape.Begin();
      const int SEGMENTS = 16;
      for (float y = 5.66f; y > 1.66f; y -= 0.33f)
      for (int x = -SEGMENTS; x < SEGMENTS; x++)
        _cape.Quad(
          _cape.Float2(x, y).Next(),
          _cape.Float2(x + 1, y).Next(),
          _cape.Float2(x + 1, y - 0.5f).Next(),
          _cape.Float2(x, y - 0.5f).Next()
        );

      _cape.End();
    }

    public override void Render(fall_obj objIn)
    {
      base.Render(objIn);

      Vector3 offset = (-1, -2, -1);
      Vector3 renderPos = objIn.LerpedPos + offset;

      // render head
      _icosphere.Render(renderPos - offset + (0f, 4.25f, 0f));

      float lyaw = objIn.Get<float_pos>(type.FLOAT_POS).LerpedYaw + 180;

      _capeShader.Bind();
      _capeShader.SetFloat("_yaw", lyaw);
      _capeShader.SetVector3("_translation",
        renderPos + (1, 0, 1) + (-0.15f * MathF.Cos(lyaw.Rad()), 0,
          -0.15f * MathF.Sin(lyaw.Rad())));
      _capeShader.SetVector4("_color", _color.ToVector4());
      _cape.Render();
    }
  }
}