
using OpenTK.Graphics.OpenGL4;

namespace Fall.Engine
{
    public class ibo
    {
        private static int _active;
        private readonly int _handle;
        private readonly MemoryStream _indices;
        private readonly byte[] _bitDest = new byte[4];
        private readonly bool _static;

        public ibo(int initialCapacity, bool static_)
        {
            _handle = GL.GenBuffer();
            _indices = new MemoryStream(initialCapacity);
            _static = static_;
        }

        public void bind()
        {
            if (_handle == _active)
            {
                return;
            }
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _handle);
            _active = _handle;
        }

        public static void unbind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            _active = 0;
        }

        public void put(int element)
        {
            BitConverter.TryWriteBytes(_bitDest, element);
            _indices.Write(_bitDest);
        }

        public void upload(bool unbindAfter = true)
        {
            if (_active != _handle)
            {
                bind();
            }
            GL.BufferData(BufferTarget.ElementArrayBuffer, (int) _indices.Length, _indices.GetBuffer(), _static ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw);
            if (unbindAfter)
            {
                unbind();
            }
        }

        public void clear()
        {
            _indices.SetLength(0);
            if (_static)
            {
                dispose();
            }
        }

        private void dispose()
        {
            _indices.Dispose();
        }
    }
}