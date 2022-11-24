using Fall.Shared;
using OpenTK.Graphics.OpenGL4;
using StbTrueTypeSharp;

namespace Fall.Engine
{
  public static class font
  {
    private const float _ipw = 1.0f / 2048f;
    private const float _iph = _ipw;
    private static readonly float _ascent;
    private static readonly StbTrueType.stbtt_packedchar[] _chars;

    public static int Height;
    public static texture Texture;

    // I have absolutely no idea how to use unsafe :((
    static unsafe font()
    {
      byte[] buffer = File.ReadAllBytes("Resource/Font/Dank Mono Italic.otf");
      int height = 20;
      Height = height;

      StbTrueType.stbtt_fontinfo fontInfo = StbTrueType.CreateFont(buffer, 0);

      _chars = new StbTrueType.stbtt_packedchar[256];
      StbTrueType.stbtt_pack_context packContext = new();

      byte[] bitmap = new byte[2048 * 2048];
      fixed (byte* dat = bitmap)
      {
        StbTrueType.stbtt_PackBegin(packContext, dat, 2048, 2048, 0, 1, null);
      }

      StbTrueType.stbtt_PackSetOversampling(packContext, 8, 8);
      fixed (byte* dat = buffer)
      {
        fixed (StbTrueType.stbtt_packedchar* c = _chars)
        {
          StbTrueType.stbtt_PackFontRange(packContext, dat, 0, height, 32, 256, c);
        }
      }

      StbTrueType.stbtt_PackEnd(packContext);

      int asc;
      StbTrueType.stbtt_GetFontVMetrics(fontInfo, &asc, null, null);
      _ascent = asc * StbTrueType.stbtt_ScaleForPixelHeight(fontInfo, height);

      Texture = texture.load_from_buffer(bitmap, 2048, 2048, PixelFormat.Red, PixelInternalFormat.R8,
        TextureMinFilter.NearestMipmapNearest, TextureMagFilter.Nearest);
    }

    public static void Bind()
    {
      Texture.Bind(TextureUnit.Texture0);
    }

    public static void Unbind()
    {
      texture.Unbind();
    }

    public static void Draw(mesh mesh, string text, float x, float y, uint color, bool shadow, float scale = 1.0f)
    {
      int length = text.Length;
      float drawX = x;
      float drawY = y - _ascent * scale;
      float alpha = ((color >> 24) & 0xFF) / 255.0f;
      float red = ((color >> 16) & 0xFF) / 255.0f;
      float green = ((color >> 8) & 0xFF) / 255.0f;
      float blue = (color & 0xFF) / 255.0f;
      string lower = text.ToLower();
      for (int i = 0; i < length; i++)
      {
        char charCode = text[i];
        char previous = i > 0 ? text[i - 1] : ' ';
        if (previous == '\u00a7') continue;

        if (charCode == '\u00a7' && i < length - 1)
        {
          char next = lower[i + 1];
          if (Shared.fmt.VALUES.TryGetValue(next, out fmt fmt))
          {
            uint newColor = fmt.Color;
            red = ((newColor >> 16) & 0xFF) / 255.0f;
            green = ((newColor >> 8) & 0xFF) / 255.0f;
            blue = (newColor & 0xFF) / 255.0f;
          }

          continue;
        }

        if (charCode < 32 || charCode > 32 + 256) charCode = ' ';

        StbTrueType.stbtt_packedchar c = _chars[charCode - 32];

        float dxs = drawX + c.xoff * scale;
        float dys = drawY - c.yoff * scale;
        float dx1S = drawX + c.xoff2 * scale;
        float dy1S = drawY - c.yoff2 * scale;

        if (shadow)
        {
          int j1 = mesh.Float3(dxs + 1, dys - 1, 1).Float3(0, 0, 1).Float2(c.x0 * _ipw, c.y0 * _iph)
            .Float4(red * 0.125f, green * 0.125f, blue * 0.125f, alpha).Next();
          int j2 = mesh.Float3(dxs + 1, dy1S - 1, 1).Float3(0, 0, 1).Float2(c.x0 * _ipw, c.y1 * _iph)
            .Float4(red * 0.125f, green * 0.125f, blue * 0.125f, alpha).Next();
          int j3 = mesh.Float3(dx1S + 1, dy1S - 1, 1).Float3(0, 0, 1).Float2(c.x1 * _ipw, c.y1 * _iph)
            .Float4(red * 0.125f, green * 0.125f, blue * 0.125f, alpha).Next();
          int j4 = mesh.Float3(dx1S + 1, dys - 1, 1).Float3(0, 0, 1).Float2(c.x1 * _ipw, c.y0 * _iph)
            .Float4(red * 0.125f, green * 0.125f, blue * 0.125f, alpha).Next();
          mesh.Quad(j1, j2, j3, j4);
        }

        int k1 = mesh.Float3(dxs, dys, 0).Float3(0, 0, 1).Float2(c.x0 * _ipw, c.y0 * _iph)
          .Float4(red, green, blue, alpha).Next();
        int k2 = mesh.Float3(dxs, dy1S, 0).Float3(0, 0, 1).Float2(c.x0 * _ipw, c.y1 * _iph)
          .Float4(red, green, blue, alpha).Next();
        int k3 = mesh.Float3(dx1S, dy1S, 0).Float3(0, 0, 1).Float2(c.x1 * _ipw, c.y1 * _iph)
          .Float4(red, green, blue, alpha).Next();
        int k4 = mesh.Float3(dx1S, dys, 0).Float3(0, 0, 1).Float2(c.x1 * _ipw, c.y0 * _iph)
          .Float4(red, green, blue, alpha).Next();
        mesh.Quad(k1, k2, k3, k4);

        drawX += c.xadvance * scale;
        drawX -= 0.4f * scale;
      }
    }

    public static float GetWidth(string text, float scale = 1.0f)
    {
      int length = text.Length;
      float width = 0;
      for (int i = 0; i < length; i++)
      {
        char charCode = text[i];
        char previous = i > 0 ? text[i - 1] : ' ';
        if (previous == '\u00a7') continue;

        if (charCode < 32 || charCode > 32 + 256) charCode = ' ';

        StbTrueType.stbtt_packedchar c = _chars[charCode - 32];

        width += c.xadvance * scale;
        width -= 0.4f * scale;
      }

      width += 0.4f * scale;

      return width;
    }

    public static float GetHeight(float scale = 1.0f)
    {
      return _ascent * scale;
    }
  }
}