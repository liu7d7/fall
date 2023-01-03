using Fall.Shared;
using Fall.Shared.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Fall.Engine
{
  public static class glh
  {
    public const float THRESHOLD = 0.033f;
    private const float _depthThreshold = 0.8f;

    public static readonly shader BASIC = new("basic", "basic");
    private static readonly shader _john = new("john", "john");
    private static readonly shader _wideLines = new("widelines2", "widelines2", "widelines2");
    private static readonly shader _pixel = new("postprocess", "pixelate");
    private static readonly shader _fxaa = new("fxaa", "fxaa");
    private static readonly shader _outline = new("postprocess", "outline");
    private static readonly shader _blobs = new("blobs", "blobs");
    private static readonly shader _outlineWatercolor = new("sobel", "outlinewatercolor");
    private static readonly shader _outlineCombine = new("sobel", "outlinecombine");
    private static readonly shader _blur = new("sobel", "blur");

    private static Matrix4 _projection;
    private static Matrix4 _lookAt;
    private static readonly Matrix4[] _model = new Matrix4[7];
    private static int _modelIdx;

    public static readonly mesh MESH = new(mesh.draw_mode.TRIANGLE, _john, false, vao.attrib.Float3,
      vao.attrib.Float2, vao.attrib.Float4);

    public static readonly mesh LINE = new(mesh.draw_mode.LINE, _wideLines, false, vao.attrib.Float3,
      vao.attrib.Float4);

    private static readonly mesh _post = new(mesh.draw_mode.TRIANGLE, null, false, vao.attrib.Float2);

    public static readonly fbo FRAME0 = new(1, 1, true);
    public static readonly fbo FRAME1 = new(1, 1, true);
    public static readonly fbo FRAME2 = new(1, 1, true);
    public static bool Rendering3d;
    public static bool RenderingRed;
    private static float_pos _camera;

    static glh()
    {
      Array.Fill(_model, Matrix4.Identity);
    }

    public static ref Matrix4 Model => ref _model[_modelIdx];

    public static Vector2i Size => fall.Instance.Size;

    public static void Resize()
    {
      _post.Begin();
      _post.Quad(
        _post.Float2(-1, -1).Next(),
        _post.Float2(1, -1).Next(),
        _post.Float2(1, 1).Next(),
        _post.Float2(-1, 1).Next()
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

    public static void Translate(Vector3 vec)
    {
      Model.Translate(vec);
    }

    public static void Rotate(float angle, float x, float y, float z)
    {
      Model.Rotate(angle, x, y, z);
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
      shader.SetVector2("_radius", (2, 2));
    }

    public static void RenderPixelation(float pixWidth, float pixHeight)
    {
      FRAME1.ClearColor();
      FRAME1.ClearDepth();
      FRAME1.Bind();
      _pixel.Bind();
      FRAME0.BindColor(TextureUnit.Texture0);
      _pixel.SetInt("_tex0", 0);
      _pixel.SetVector2("_screenSize", (Size.X, Size.Y));
      _pixel.SetVector2("_pixSize", (pixWidth, pixHeight));
      _post.Render();
      FRAME1.Blit(FRAME0.Handle);
      shader.Unbind();
    }

    public static void RenderFxaa(fbo fbo)
    {
      fbo.Blit(FRAME1.Handle);
      FRAME1.Bind();
      _fxaa.Bind();
      fbo.BindColor(TextureUnit.Texture0);
      _fxaa.SetInt("_tex0", 0);
      _fxaa.SetFloat("SpanMax", 8);
      _fxaa.SetFloat("ReduceMul", 0.125f);
      _fxaa.SetFloat("SubPixelShift", 0.25f);
      _fxaa.SetVector2("_screenSize", (Size.X, Size.Y));
      _post.Render();
      shader.Unbind();
      FRAME1.Blit(fbo.Handle);
    }

    public static void RenderOutline()
    {
      FRAME0.ClearColor();
      FRAME0.ClearDepth();
      FRAME0.Bind();
      _outline.Bind();
      FRAME1.BindColor(TextureUnit.Texture0);
      _outline.SetInt("_tex0", 0);
      FRAME1.BindDepth(TextureUnit.Texture1);
      _outline.SetInt("_tex1", 1);
      _outline.SetInt("_abs", 1);
      _outline.SetInt("_glow", 0);
      _outline.SetInt("_blackAndWhite", 1);
      _outline.SetFloat("_width", 1f);
      _outline.SetFloat("_threshold", THRESHOLD);
      _outline.SetFloat("_depthThreshold", _depthThreshold);
      _outline.SetVector2("_screenSize", (Size.X, Size.Y));
      _outline.SetVector4("_outlineColor", fall.PINK0.ToVector4());
      _outline.SetVector4("_otherColor", Color4.White.ToVector4());
      _post.Render();
      shader.Unbind();
    }

    public static void RenderOutlineWaterColor()
    {
      FRAME1.Clear();
      _outlineWatercolor.Bind();
      _outlineWatercolor.SetFloat("_lumaRamp", 16f);
      _outlineWatercolor.SetVector2("_screenSize", (Size.X, Size.Y));
      FRAME1.Bind();
      FRAME0.BindColor(0);
      _outlineWatercolor.SetInt("_tex0", 0);
      _post.Render();
      FRAME1.Blit(FRAME0.Handle);
    }

    public static void RenderBokeh(float radius)
    {
      FRAME1.Clear();
      FRAME2.Clear();

      _blobs.Bind();
      _blobs.SetFloat("_radius", radius);
      _blobs.SetVector2("_screenSize", (Size.X, Size.Y));
      FRAME1.Bind();
      FRAME0.BindColor(0);
      _blobs.SetInt("_tex0", 0);
      _post.Render();

      _outlineWatercolor.Bind();
      _outlineWatercolor.SetFloat("_lumaRamp", 16f);
      _outlineWatercolor.SetVector2("_screenSize", (Size.X, Size.Y));
      FRAME0.Bind();
      FRAME1.BindColor(0);
      _outlineWatercolor.SetInt("_tex0", 0);
      _post.Render();

      _blur.Bind();
      _blur.SetVector2("_blurDir", (0f, 0.8f));
      _blur.SetVector2("_screenSize", (Size.X, Size.Y));
      _blur.SetFloat("_radius", radius * 2);
      FRAME2.Bind();
      FRAME0.BindColor(0);
      _blur.SetInt("_tex0", 0);
      _post.Render();

      _blur.Bind();
      _blur.SetVector2("_blurDir", (0.8f, 0f));
      _blur.SetVector2("_screenSize", (Size.X, Size.Y));
      _blur.SetFloat("_radius", radius * 2);
      FRAME0.Bind();
      FRAME2.BindColor(0);
      _blur.SetInt("_tex0", 0);
      _post.Render();

      _outlineCombine.Bind();
      _outlineCombine.SetVector2("_screenSize", (Size.X, Size.Y));
      FRAME2.Bind();
      FRAME1.BindColor(0);
      _outlineCombine.SetInt("_tex0", 0);
      FRAME0.BindColor(1);
      _outlineCombine.SetInt("_tex1", 1);
      _post.Render();

      FRAME2.Blit(FRAME0.Handle);
    }

    public static void Line(float x1, float y1, float x2, float y2, uint color)
    {
      LINE.Line(
        LINE.Float3(x1, y1, 0).Float4(color).Next(),
        LINE.Float3(x2, y2, 0).Float4(color).Next()
      );
    }

    public static void UpdateProjection()
    {
      if (Rendering3d)
      {
        Matrix4.CreatePerspectiveFieldOfView(camera.FOV, Size.X / (float)Size.Y, camera.NEAR, camera.Far,
          out _projection);
        return;
      }
  
      Matrix4.CreateOrthographicOffCenter(0, Size.X, Size.Y, 0, -1000, 3000, out _projection);
    }

    public static void UpdateLookAt(fall_obj cameraObj, bool rendering3d = true)
    {
      if (!cameraObj.Has(fall_obj.comp_type.FloatPos)) return;

      _camera = float_pos.Get(cameraObj);
      Rendering3d = rendering3d;
      if (!Rendering3d)
      {
        _lookAt = Matrix4.Identity;
        return;
      }

      camera comp = camera.Get(cameraObj);
      _lookAt = comp.get_camera_matrix();
    }
  }
}