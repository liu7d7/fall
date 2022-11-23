using Fall.Shared;
using Fall.Shared.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Fall.Engine
{
  public static class render_system
  {
    public const float THRESHOLD = 0.033f;
    private const float _depthThreshold = 0.8f;
    private static readonly shader _john = new("Resource/Shader/john.vert", "Resource/Shader/john.frag");
    public static readonly shader BASIC = new("Resource/Shader/basic.vert", "Resource/Shader/basic.frag");
    private static readonly shader _cubemap = new("Resource/Shader/cubemap.vert", "Resource/Shader/cubemap.frag");

    private static readonly shader
      _pixel = new("Resource/Shader/postprocess.vert", "Resource/Shader/pixelate.frag");

    private static readonly shader _fxaa = new("Resource/Shader/fxaa.vert", "Resource/Shader/fxaa.frag");
    private static readonly shader _line = new("Resource/Shader/lines.vert", "Resource/Shader/basic.frag");

    private static readonly shader
      _outline = new("Resource/Shader/postprocess.vert", "Resource/Shader/outline.frag");

    private static Matrix4 _projection;
    private static Matrix4 _lookAt;
    private static readonly Matrix4[] _model = new Matrix4[7];
    private static int _modelIdx;
    public static bool RenderingRed;
    public static readonly texture RECT = texture.load_from_file("Resource/Texture/rect.png");

    public static readonly mesh MESH = new(mesh.draw_mode.TRIANGLE, _john, false, vao.attrib.FLOAT3,
      vao.attrib.FLOAT3,
      vao.attrib.FLOAT2, vao.attrib.FLOAT4);

    public static readonly mesh LINE = new(mesh.draw_mode.LINE, _line, false, vao.attrib.FLOAT3,
      vao.attrib.FLOAT4);

    private static readonly mesh _post = new(mesh.draw_mode.TRIANGLE, null, false, vao.attrib.FLOAT2);
    public static readonly fbo FRAME = new(1, 1, true);
    public static readonly fbo SWAP = new(1, 1, true);
    public static bool Rendering3d;
    private static float_pos _camera;

    static render_system()
    {
      Array.Fill(_model, Matrix4.Identity);
    }

    public static ref Matrix4 Model => ref _model[_modelIdx];

    public static Vector2i Size => fall.Instance.Size;

    public static void Resize()
    {
      _post.Begin();
      _post.Quad(
        _post.Float2(0, 0).Next(),
        _post.Float2(Size.X, 0).Next(),
        _post.Float2(Size.X, Size.Y).Next(),
        _post.Float2(0, Size.Y).Next()
      );
      _post.End();
    }

    public static void Push()
    {
      _model[_modelIdx + 1] = Model;
      _modelIdx++;
    }

    public static void Pop()
    {
      _modelIdx--;
    }

    public static void Translate(float x, float y, float z)
    {
      Model.Translate(x, y, z);
    }

    public static void Translate(Vector3 vec)
    {
      Model.Translate(vec);
    }

    public static void Rotate(float angle, float x, float y, float z)
    {
      Model.Rotate(angle, x, y, z);
    }

    public static void Rotate(float angle, Vector3 vec)
    {
      Model.Rotate(angle, vec);
    }

    public static void Scale(float x, float y, float z)
    {
      Model.Scale(x, y, z);
    }

    public static void Scale(Vector3 vec)
    {
      Model.Scale(vec);
    }

    public static void Scale(float scale)
    {
      Model.Scale(scale);
    }

    public static Vector4 to_vector4(this Color4 color)
    {
      return (color.R, color.G, color.B, color.A);
    }

    public static Vector4 to_vector4(this uint val)
    {
      return (((val >> 16) & 0xff) / 255f, ((val >> 8) & 0xff) / 255f, (val & 0xff) / 255f,
        ((val >> 24) & 0xff) / 255f);
    }

    public static uint to_uint(this Color4 color)
    {
      return (uint)color.ToArgb();
    }

    public static void set_defaults(this shader shader)
    {
      shader.set_int("_renderingRed", RenderingRed ? 1 : 0);
      shader.set_int("_rendering3d", Rendering3d ? 1 : 0);
      shader.set_int("doLighting", 0);
      shader.set_vector2("_screenSize", (Size.X, Size.Y));
      shader.set_vector3("lightPos", (_camera.X + 5, _camera.Y + 12, _camera.Z + 5));
      shader.set_matrix4("_proj", _projection);
      shader.set_matrix4("_lookAt", _lookAt);
      shader.set_float("_time", Environment.TickCount / 1000f % (MathF.PI * 2f));
    }

    public static void render_pixelation(float pixWidth, float pixHeight)
    {
      FRAME.clear_color();
      FRAME.clear_depth();
      FRAME.Bind();
      _pixel.Bind();
      SWAP.bind_color(TextureUnit.Texture0);
      _pixel.set_int("_tex0", 0);
      _pixel.set_vector2("_screenSize", (Size.X, Size.Y));
      _pixel.set_vector2("_pixSize", (pixWidth, pixHeight));
      _post.Render();
      shader.Unbind();
    }

    public static void render_fxaa(fbo fbo)
    {
      fbo.Blit(SWAP.Handle);
      SWAP.Bind();
      _fxaa.Bind();
      fbo.bind_color(TextureUnit.Texture0);
      _fxaa.set_int("_tex0", 0);
      _fxaa.set_float("SpanMax", 8);
      _fxaa.set_float("ReduceMul", 0.125f);
      _fxaa.set_float("SubPixelShift", 0.25f);
      _fxaa.set_vector2("_screenSize", (Size.X, Size.Y));
      _post.Render();
      shader.Unbind();
      SWAP.Blit(fbo.Handle);
    }

    public static void render_outline()
    {
      FRAME.clear_color();
      FRAME.clear_depth();
      FRAME.Bind();
      _outline.Bind();
      SWAP.bind_color(TextureUnit.Texture0);
      _outline.set_int("_tex0", 0);
      SWAP.bind_depth(TextureUnit.Texture1);
      _outline.set_int("_tex1", 1);
      _outline.set_int("_abs", 1);
      _outline.set_int("_glow", 1);
      _outline.set_int("_blackAndWhite", 1);
      _outline.set_float("_width", 1f);
      _outline.set_float("_threshold", THRESHOLD);
      _outline.set_float("_depthThreshold", _depthThreshold);
      _outline.set_vector2("_screenSize", (Size.X, Size.Y));
      _outline.set_vector4("_outlineColor", fall.PINK.to_vector4());
      _outline.set_vector4("_otherColor", Color4.White.to_vector4());
      _post.Render();
      shader.Unbind();
    }

    public static void update_projection()
    {
      if (Rendering3d)
      {
        Matrix4.CreatePerspectiveFieldOfView(camera.FOV, Size.X / (float)Size.Y, camera.NEAR, camera.FAR,
          out _projection);
        return;
      }

      Matrix4.CreateOrthographic(Size.X, Size.Y, -1000, 3000, out _projection);
    }

    public static void update_look_at(fall_obj cameraObj, bool rendering3d = true)
    {
      if (!cameraObj.Has<float_pos>()) return;

      _camera = cameraObj.Get<float_pos>();
      Rendering3d = rendering3d;
      if (!Rendering3d)
      {
        _lookAt = Matrix4.Identity;
        return;
      }

      camera comp = cameraObj.Get<camera>();
      _lookAt = comp.get_camera_matrix();
    }
  }
}