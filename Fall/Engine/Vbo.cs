using OpenTK.Graphics.OpenGL4;

namespace Fall.Engine
{
  public class vbo
  {
    private static int _active;
    private readonly byte[] _bitDest = new byte[4];
    private readonly int _handle;
    private readonly bool _static;
    private int _count;
    private byte[] _vertices;

    public vbo(int initialCapacity, bool @static)
    {
      _handle = GL.GenBuffer();
      _vertices = new byte[initialCapacity];
      _static = @static;
    }

    public void Bind()
    {
      if (_handle == _active) return;
      GL.BindBuffer(BufferTarget.ArrayBuffer, _handle);
      _active = _handle;
    }

    public static void Unbind()
    {
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      _active = 0;
    }

    public void Put(float element)
    {
      BitConverter.TryWriteBytes(_bitDest, element);
      if (_count + 4 > _vertices.Length) Array.Resize(ref _vertices, _vertices.Length * 2);
      _vertices[_count] = _bitDest[0];
      _vertices[_count + 1] = _bitDest[1];
      _vertices[_count + 2] = _bitDest[2];
      _vertices[_count + 3] = _bitDest[3];
      _count += 4;
    }

    public void Upload(bool unbindAfter = true)
    {
      if (_active != _handle) Bind();
      GL.BufferData(BufferTarget.ArrayBuffer, _count, _vertices,
        _static ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw);
      if (unbindAfter) Unbind();
    }

    public void Clear()
    {
      _count = 0;
      Array.Clear(_vertices, 0, _count);
      if (_static) Dispose();
    }

    private void Dispose()
    {
      _vertices = null;
    }
  }
}