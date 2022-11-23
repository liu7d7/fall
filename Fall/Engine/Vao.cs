using OpenTK.Graphics.OpenGL4;

namespace Fall.Engine
{
  public class vao
  {
    public enum attrib
    {
      FLOAT1 = 1,
      FLOAT2 = 2,
      FLOAT3 = 3,
      FLOAT4 = 4
    }

    private static int _active;
    private readonly int _handle;

    public vao(params attrib[] attribs)
    {
      _handle = GL.GenVertexArray();
      Bind();
      int stride = attribs.Sum(attrib => (int)attrib);
      int offset = 0;
      for (int i = 0; i < attribs.Length; i++)
      {
        GL.EnableVertexAttribArray(i);
        GL.VertexAttribPointer(i, (int)attribs[i], VertexAttribPointerType.Float, false, stride * sizeof(float),
          offset);
        offset += (int)attribs[i] * sizeof(float);
      }

      Unbind();
    }

    public void Bind()
    {
      if (_handle == _active) return;
      GL.BindVertexArray(_handle);
      _active = _handle;
    }

    public static void Unbind()
    {
      GL.BindVertexArray(0);
      _active = 0;
    }
  }
}