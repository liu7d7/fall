using OpenTK.Graphics.OpenGL4;

namespace Fall.Engine
{
  public class vao
  {
    public enum attrib
    {
      Float1 = 1,
      Float2 = 2,
      Float3 = 3,
      Float4 = 4
    }

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
      GL.BindVertexArray(_handle);
    }

    public static void Unbind()
    {
      GL.BindVertexArray(0);
    }
  }
}