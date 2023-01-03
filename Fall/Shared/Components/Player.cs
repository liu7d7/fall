using Fall.Engine;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

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
      _icosphere = model3d.Read("icosphere", new Dictionary<string, uint> { { "fortnite", 1 } });
      _icosphere.Scale(0.66f);
      _capeShader = new shader("cape", "basic");
    }

    public player() : base(fall_obj.comp_type.Player)
    {
      _color = colors.NextColor();
      _cape = new mesh(mesh.draw_mode.TRIANGLE, _capeShader, true, vao.attrib.Float2);
      _cape.Begin();
      const int SEGMENTS = 16;
      for (float y = 6f; y > 1.66f; y -= 0.33f)
      for (int x = -SEGMENTS; x < SEGMENTS; x++)
        _cape.Quad(
          _cape.Float2(x, y).Next(),
          _cape.Float2(x + 1, y).Next(),
          _cape.Float2(x + 1, y - 0.5f).Next(),
          _cape.Float2(x, y - 0.5f).Next()
        );

      _cape.End();
    }

    public override void Update(fall_obj objIn)
    {
      base.Update(objIn);

      if (fall.IsPressed(Keys.Space))
      {
        fall_obj obj = new();
        float_pos pos = float_pos.Get(objIn);
        camera cam = camera.Get(objIn);
        obj.Add(new float_pos
        {
          X = pos.X, Y = pos.Y + 3.25f, Z = pos.Z, PrevX = pos.X, PrevY = pos.Y + 3.25f, PrevZ = pos.Z
        });
        obj.Add(new projectile(cam.Front, 2f));
        obj.Updates = true;
        fall.World.Objs.Add(obj);
      }
    }

    public override void Render(fall_obj objIn)
    {
      base.Render(objIn);

      Vector3 renderPos = objIn.LerpedPos;

      // render head
      _icosphere.Render(renderPos + (0f, 4.5f, 0f));

      float lyaw = float_pos.Get(objIn).LerpedYaw + 180;

      _capeShader.Bind();
      _capeShader.SetFloat("_yaw", lyaw);
      _capeShader.SetVector3("_translation",
        renderPos + (-0.15f * MathF.Cos(lyaw.Rad()), -2,
          -0.15f * MathF.Sin(lyaw.Rad())));
      _capeShader.SetVector4("_color", _color.ToVector4());
      _cape.Render();
    }
  }
}