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

    public static Vector4 ToVector4(this Color4 color)
    {
      return (color.R, color.G, color.B, color.A);
    }

    public static Vector4 ToVector4(this uint val)
    {
      return (((val >> 16) & 0xff) / 255f, ((val >> 8) & 0xff) / 255f, (val & 0xff) / 255f,
        ((val >> 24) & 0xff) / 255f);
    }

    public static uint ToUInt(this Color4 color)
    {
      return (uint)color.ToArgb();
    }

    public static void SetDefaults(this shader shader)
    {
      shader.SetInt("_renderingRed", RenderingRed ? 1 : 0);
      shader.SetInt("_rendering3d", Rendering3d ? 1 : 0);
      shader.SetInt("doLighting", 0);
      shader.SetVector2("_screenSize", (Size.X, Size.Y));
      shader.SetVector3("lightPos", (_camera.X + 5, _camera.Y + 12, _camera.Z + 5));
      shader.SetMatrix4("_proj", _projection);
      shader.SetMatrix4("_lookAt", _lookAt);
      shader.SetFloat("_time", Environment.TickCount / 1000f % (MathF.PI * 2f));
    }

    public static void RenderPixelation(float pixWidth, float pixHeight)
    {
      FRAME.ClearColor();
      FRAME.ClearDepth();
      FRAME.Bind();
      _pixel.Bind();
      SWAP.BindColor(TextureUnit.Texture0);
      _pixel.SetInt("_tex0", 0);
      _pixel.SetVector2("_screenSize", (Size.X, Size.Y));
      _pixel.SetVector2("_pixSize", (pixWidth, pixHeight));
      _post.Render();
      shader.Unbind();
    }

    public static void RenderFxaa(fbo fbo)
    {
      fbo.Blit(SWAP.Handle);
      SWAP.Bind();
      _fxaa.Bind();
      fbo.BindColor(TextureUnit.Texture0);
      _fxaa.SetInt("_tex0", 0);
      _fxaa.SetFloat("SpanMax", 8);
      _fxaa.SetFloat("ReduceMul", 0.125f);
      _fxaa.SetFloat("SubPixelShift", 0.25f);
      _fxaa.SetVector2("_screenSize", (Size.X, Size.Y));
      _post.Render();
      shader.Unbind();
      SWAP.Blit(fbo.Handle);
    }

    public static void RenderOutline()
    {
      FRAME.ClearColor();
      FRAME.ClearDepth();
      FRAME.Bind();
      _outline.Bind();
      SWAP.BindColor(TextureUnit.Texture0);
      _outline.SetInt("_tex0", 0);
      SWAP.BindDepth(TextureUnit.Texture1);
      _outline.SetInt("_tex1", 1);
      _outline.SetInt("_abs", 1);
      _outline.SetInt("_glow", 1);
      _outline.SetInt("_blackAndWhite", 1);
      _outline.SetFloat("_width", 1f);
      _outline.SetFloat("_threshold", THRESHOLD);
      _outline.SetFloat("_depthThreshold", _depthThreshold);
      _outline.SetVector2("_screenSize", (Size.X, Size.Y));
      _outline.SetVector4("_outlineColor", fall.PINK.ToVector4());
      _outline.SetVector4("_otherColor", Color4.White.ToVector4());
      _post.Render();
      shader.Unbind();
    }

    public static void UpdateProjection()
    {
      if (Rendering3d)
      {
        Matrix4.CreatePerspectiveFieldOfView(camera.FOV, Size.X / (float)Size.Y, camera.NEAR, camera.FAR,
          out _projection);
        return;
      }

      Matrix4.CreateOrthographic(Size.X, Size.Y, -1000, 3000, out _projection);
    }

    public static void UpdateLookAt(fall_obj cameraObj, bool rendering3d = true)
    {
      if (!cameraObj.Has(fall_obj.component.type.FLOAT_POS)) return;

      _camera = cameraObj.Get<float_pos>(fall_obj.component.type.FLOAT_POS);
      Rendering3d = rendering3d;
      if (!Rendering3d)
      {
        _lookAt = Matrix4.Identity;
        return;
      }

      camera comp = cameraObj.Get<camera>(fall_obj.component.type.CAMERA);
      _lookAt = comp.get_camera_matrix();
    }
  }
}