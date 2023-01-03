using OpenTK.Graphics.OpenGL4;

namespace Fall.Engine
{
  public class fbo
  {
    private static readonly Dictionary<int, fbo> _frames = new();
    private readonly bool _multisample;
    private readonly bool _useDepth;
    private int _colorAttachment;
    private int _depthAttachment;
    private int _height;
    private int _width;
    public int Handle;

    public fbo(int width, int height, bool useDepth)
    {
      _width = width;
      _height = height;
      _useDepth = useDepth;
      Handle = -1;
      Init();
      _frames[Handle] = this;
    }

    public fbo(int width, int height, bool useDepth, bool multisample)
    {
      _width = width;
      _height = height;
      _useDepth = useDepth;
      _multisample = multisample;
      Handle = -1;
      Init();
      _frames[Handle] = this;
    }

    private void Dispose()
    {
      GL.DeleteFramebuffer(Handle);
      GL.DeleteTexture(_colorAttachment);
      if (_useDepth) GL.DeleteTexture(_depthAttachment);
    }

    private void Init()
    {
      if (Handle != -1) Dispose();

      Handle = GL.GenFramebuffer();
      Bind();
      _colorAttachment = GL.GenTexture();
      if (_multisample)
      {
        GL.BindTexture(TextureTarget.Texture2DMultisample, _colorAttachment);
        GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, 4,
          PixelInternalFormat.Rgba8, _width, _height, true);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
          TextureTarget.Texture2DMultisample, _colorAttachment, 0);
      }
      else
      {
        GL.BindTexture(TextureTarget.Texture2D, _colorAttachment);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
          (int)TextureWrapMode.MirroredRepeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
          (int)TextureWrapMode.MirroredRepeat);
        GL.TexStorage2D(TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba12, _width, _height);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
          TextureTarget.Texture2D, _colorAttachment, 0);
      }

      if (_useDepth)
      {
        _depthAttachment = GL.GenTexture();
        if (_multisample)
        {
          GL.BindTexture(TextureTarget.Texture2DMultisample, _depthAttachment);
          GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, 4,
            PixelInternalFormat.DepthComponent24, _width, _height, true);
          GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            TextureTarget.Texture2DMultisample, _depthAttachment, 0);
        }
        else
        {
          GL.BindTexture(TextureTarget.Texture2D, _depthAttachment);
          GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Nearest);
          GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);
          GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode,
            (int)TextureCompareMode.None);
          GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
          GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
          GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, _width, _height, 0,
            PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
          GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            TextureTarget.Texture2D, _depthAttachment, 0);
        }
      }

      FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
      if (status != FramebufferErrorCode.FramebufferComplete)
        throw new Exception(
          $"Incomplete Framebuffer! {status} should be {FramebufferErrorCode.FramebufferComplete}");

      Unbind();
    }

    private void _resize(int width, int height)
    {
      _width = width;
      _height = height;
      Init();
    }

    public static void Resize(int width, int height)
    {
      foreach (KeyValuePair<int, fbo> frame in _frames) frame.Value._resize(width, height);
    }

    public void BindColor(TextureUnit unit)
    {
      image2d.Bind(_colorAttachment, unit);
    }

    public void BindColor(int unit)
    {
      image2d.Bind(_colorAttachment, TextureUnit.Texture0 + unit);
    }

    public void BindDepth(TextureUnit unit)
    {
      if (!_useDepth)
        throw new Exception("Trying to bind depth texture of a framebuffer without depth!");

      image2d.Bind(_depthAttachment, unit);
    }

    public void BindDepth(int unit)
    {
      if (!_useDepth)
        throw new Exception("Trying to bind depth texture of a framebuffer without depth!");

      image2d.Bind(_depthAttachment, TextureUnit.Texture0 + unit);
    }

    public void ClearColor()
    {
      Bind();
      GL.Clear(ClearBufferMask.ColorBufferBit);
      Unbind();
    }

    public void ClearDepth()
    {
      Bind();
      GL.Clear(ClearBufferMask.DepthBufferBit);
      Unbind();
    }

    public void Clear()
    {
      ClearColor();
      if (_useDepth)
        ClearDepth();
    }

    public void Bind()
    {
      GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
    }

    public void Blit(int other = 0)
    {
      GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, Handle);
      GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, other);
      GL.BlitFramebuffer(0, 0, _width, _height, 0, 0, _width, _height, ClearBufferMask.ColorBufferBit,
        BlitFramebufferFilter.Linear);
      if (other == 0 || _frames[other]._useDepth)
        GL.BlitFramebuffer(0, 0, _width, _height, 0, 0, _width, _height, ClearBufferMask.DepthBufferBit,
          BlitFramebufferFilter.Nearest);

      Unbind();
    }

    public static void Unbind()
    {
      GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
  }
}