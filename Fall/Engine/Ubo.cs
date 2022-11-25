using System.Runtime.InteropServices;
using System.Text;
using Fall.Shared;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Fall.Engine
{
  public class ubo
  {
    private static int _active;
    private readonly int _handle;
    private int[] _offsets;
    private int _blkIdx;
    private int _blkSize;

    public ubo(shader shdr, string blkName, params string[] names)
    {
      _handle = GL.GenBuffer();
      _blkIdx = GL.GetUniformBlockIndex(shdr.Handle, blkName);
      GL.GetActiveUniformBlock(shdr.Handle, _blkIdx, 
        ActiveUniformBlockParameter.UniformBlockDataSize, out _blkSize);
      int[] indices = new int[names.Length];
      GL.GetUniformIndices(shdr.Handle, names.Length, names, indices);
      _offsets = new int[names.Length];
      GL.GetActiveUniforms(shdr.Handle, names.Length, indices, 
        ActiveUniformParameter.UniformOffset, _offsets);
      Bind();
      GL.BufferData(BufferTarget.UniformBuffer, _blkSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
      Unbind();
    }

    public void Bind()
    {
      if (_handle == _active) return;
      GL.BindBuffer(BufferTarget.UniformBuffer, _handle);
      _active = _handle;
    }

    public static void Unbind()
    {
      GL.BindBuffer(BufferTarget.UniformBuffer, 0);
      _active = 0;
    }
    
    public void PutAll(ref Matrix4[] mats, int size, int offset)
    {
      if (_active != _handle) Bind();
      GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)_offsets[offset], size * Marshal.SizeOf<Matrix4>(), ref mats[0]);
      Unbind();
    }
    
    public void PutAll(ref int[] ints, int size, int offset)
    {
      if (_active != _handle) Bind();
      GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)_offsets[offset], size * sizeof(int), ref ints[0]);
      Unbind();
    }
    
    public void PutAll(ref Vector3[] vecs, int size, int offset)
    {
      if (_active != _handle) Bind();
      GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)_offsets[offset], size * Marshal.SizeOf<Vector3>(), ref vecs[0]);
      Unbind();
    }
    
    public void BindTo(int bindingPoint)
    {
      GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, _handle);
    }
    
    public void Write(object key)
    {
      float[] dat = new float[_blkSize / sizeof(float)];
      Bind();
      GL.GetBufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, _blkSize, dat);
      StringBuilder sb = new();
      for (int i = 0; i < _blkSize / sizeof(float); i++)
      {
        sb.Append(dat[i] + " ");
      }
      File.WriteAllText($"ubo{deterministic_random.NextInt(key, int.MaxValue)}.txt", sb.ToString());
    }
    
    public void Clear()
    {
      if (_active != _handle) Bind();
      GL.ClearBufferData(BufferTarget.UniformBuffer, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
    }
  }
}