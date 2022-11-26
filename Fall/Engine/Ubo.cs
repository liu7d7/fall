﻿using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Fall.Engine
{
  public class ubo
  {
    private static int _active;
    private readonly int _blockIndex;
    private readonly int _blockSize;
    private readonly int _handle;
    private readonly int[] _offsets;

    public ubo(shader shdr, string blkName, params string[] names)
    {
      _handle = GL.GenBuffer();
      _blockIndex = GL.GetUniformBlockIndex(shdr.Handle, blkName);
      GL.GetActiveUniformBlock(shdr.Handle, _blockIndex,
        ActiveUniformBlockParameter.UniformBlockDataSize, out _blockSize);
      int[] indices = new int[names.Length];
      GL.GetUniformIndices(shdr.Handle, names.Length, names, indices);
      _offsets = new int[names.Length];
      GL.GetActiveUniforms(shdr.Handle, names.Length, indices,
        ActiveUniformParameter.UniformOffset, _offsets);
      Bind();
      GL.BufferData(BufferTarget.UniformBuffer, _blockSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
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
      GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)_offsets[offset], size * Marshal.SizeOf<Matrix4>(),
        ref mats[0]);
    }

    public void PutAll(ref int[] ints, int size, int offset)
    {
      if (_active != _handle) Bind();
      GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)_offsets[offset], size * sizeof(int), ref ints[0]);
    }

    public void PutAll(ref Vector3[] vecs, int size, int offset)
    {
      if (_active != _handle) Bind();
      GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)_offsets[offset], size * Marshal.SizeOf<Vector3>(),
        ref vecs[0]);
    }

    public void PutAll(ref Vector4[] vecs, int size, int offset)
    {
      if (_active != _handle) Bind();
      GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)_offsets[offset], size * Marshal.SizeOf<Vector4>(),
        ref vecs[0]);
    }

    public void BindTo(int bindingPoint)
    {
      GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, _handle);
    }

    public void Clear()
    {
      if (_active != _handle) Bind();
      GL.ClearBufferData(BufferTarget.UniformBuffer, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float,
        IntPtr.Zero);
    }
  }
}