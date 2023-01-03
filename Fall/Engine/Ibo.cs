using System.Buffers;
using OpenTK.Graphics.OpenGL4;

namespace Fall.Engine
{
  public class ibo
  {
    private static readonly byte[] _bitDest = new byte[4];
    private readonly int _handle;
    private readonly bool _static;
    private int _count;
    private byte[] _indices;

    public ibo(int initialCapacity, bool @static)
    {
      _handle = GL.GenBuffer();
      _indices = ArrayPool<byte>.Shared.Rent(initialCapacity);
      _static = @static;
    }

    public void Bind()
    {
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, _handle);
    }

    public static void Unbind()
    {
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    public void Put(int element)
    {
      BitConverter.TryWriteBytes(_bitDest, element);
      if (_count + 4 > _indices.Length)
      {
        ArrayPool<byte>.Shared.Return(_indices);
        byte[] prev = _indices;
        _indices = ArrayPool<byte>.Shared.Rent(_indices.Length * 2);
        Array.Copy(prev, _indices, _count);
      }
      _indices[_count] = _bitDest[0];
      _indices[_count + 1] = _bitDest[1];
      _indices[_count + 2] = _bitDest[2];
      _indices[_count + 3] = _bitDest[3];
      _count += 4;
    }

    public void Upload(bool unbindAfter = true)
    {
      Bind();
      GL.BufferData(BufferTarget.ElementArrayBuffer, _count, _indices,
        _static ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw);
      if (unbindAfter)
        Unbind();
    }

    public void Clear()
    {
      _count = 0;
      if (_static)
        Dispose();
      else
        Array.Clear(_indices, 0, _count);
    }

    private void Dispose()
    {
      _indices = null;
    }
  }
}