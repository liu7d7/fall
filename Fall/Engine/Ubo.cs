using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Fall.Engine
{
  public class ubo
  {
    private readonly int _handle;
    private readonly int[] _offsets;

    public ubo(shader shdr, string blkName, params string[] names)
    {
      _handle = GL.GenBuffer();
      int blockIndex = GL.GetUniformBlockIndex(shdr.Handle, blkName);
      GL.GetActiveUniformBlock(shdr.Handle, blockIndex,
        ActiveUniformBlockParameter.UniformBlockDataSize, out int blockSize);
      int[] indices = new int[names.Length];
      GL.GetUniformIndices(shdr.Handle, names.Length, names, indices);
      _offsets = new int[names.Length];
      GL.GetActiveUniforms(shdr.Handle, names.Length, indices,
        ActiveUniformParameter.UniformOffset, _offsets);
      Bind();
      GL.BufferData(BufferTarget.UniformBuffer, blockSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
      Unbind();
    }

    public void Bind()
    {
      GL.BindBuffer(BufferTarget.UniformBuffer, _handle);
    }

    public static void Unbind()
    {
      GL.BindBuffer(BufferTarget.UniformBuffer, 0);
    }

    public void PutAll(ref Matrix4[] mats, int size, int offset)
    {
      Bind();
      GL.BufferSubData(BufferTarget.UniformBuffer, _offsets[offset], size * Marshal.SizeOf<Matrix4>(),
        ref mats[0].Row0.X);
    }

    public void BindTo(int bindingPoint)
    {
      GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, _handle);
    }
  }
}