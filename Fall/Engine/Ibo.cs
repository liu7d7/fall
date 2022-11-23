using OpenTK.Graphics.OpenGL4;

namespace Fall.Engine
{
  public class ibo
  {
    private static int _active;
    private readonly byte[] _bitDest = new byte[4];
    private readonly int _handle;
    private readonly bool _static;
    private byte[] _indices;
    private int _count;

    public ibo(int initialCapacity, bool @static)
    {
      _handle = GL.GenBuffer();
      _indices = new byte[initialCapacity];
      _static = @static;
    }

    public void Bind()
    {
      if (_handle == _active) return;
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, _handle);
      _active = _handle;
    }

    public static void Unbind()
    {
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
      _active = 0;
    }

    public void Put(int element)
    {
      BitConverter.TryWriteBytes(_bitDest, element);
      if (_count + 4 > _indices.Length)
      {
        Array.Resize(ref _indices, _indices.Length * 2);
      }
      _indices[_count] = _bitDest[0];
      _indices[_count + 1] = _bitDest[1];
      _indices[_count + 2] = _bitDest[2];
      _indices[_count + 3] = _bitDest[3];
      _count += 4;
    }

    public void Upload(bool unbindAfter = true)
    {
      if (_active != _handle) Bind();
      GL.BufferData(BufferTarget.ElementArrayBuffer, _count, _indices, _static ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw);
      if (unbindAfter) Unbind();
    }

    public void Clear()
    {
      _count = 0;
      Array.Clear(_indices, 0, _count);
      if (_static) Dispose();
    }

    private void Dispose()
    {
      _indices = null;
    }
  }
}