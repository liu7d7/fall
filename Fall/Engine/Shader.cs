using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Fall.Engine
{
  // taken from https://github.com/opentk/LearnOpenTK/blob/master/Common/Shader.cs
  public class shader
  {
    private static int _active;
    private readonly int _handle;
    private readonly Dictionary<string, int> _uniformLocations;

    public shader(string vertPath, string fragPath)
    {
      string shaderSource = File.ReadAllText(vertPath);

      int vertexShader = GL.CreateShader(ShaderType.VertexShader);

      GL.ShaderSource(vertexShader, shaderSource);

      CompileShader(vertexShader);

      shaderSource = File.ReadAllText(fragPath);
      int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
      GL.ShaderSource(fragmentShader, shaderSource);
      CompileShader(fragmentShader);

      _handle = GL.CreateProgram();

      GL.AttachShader(_handle, vertexShader);
      GL.AttachShader(_handle, fragmentShader);

      LinkProgram(_handle);

      GL.DetachShader(_handle, vertexShader);
      GL.DetachShader(_handle, fragmentShader);
      GL.DeleteShader(fragmentShader);
      GL.DeleteShader(vertexShader);

      GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);

      _uniformLocations = new Dictionary<string, int>();

      for (int i = 0; i < numberOfUniforms; i++)
      {
        string key = GL.GetActiveUniform(_handle, i, out _, out _);
        int location = GL.GetUniformLocation(_handle, key);
        _uniformLocations.Add(key, location);
      }
    }

    private static void CompileShader(int shader)
    {
      GL.CompileShader(shader);

      GL.GetShader(shader, ShaderParameter.CompileStatus, out int code);
      if (code == (int)All.True) return;
      string infoLog = GL.GetShaderInfoLog(shader);
      throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
    }

    private static void LinkProgram(int program)
    {
      GL.LinkProgram(program);

      GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int code);
      if (code == (int)All.True) return;
      string infoLog = GL.GetProgramInfoLog(program);
      throw new Exception($"Error occurred whilst linking Program({program}) \n\n{infoLog}");
    }

    public void Bind()
    {
      if (_handle == _active) return;
      GL.UseProgram(_handle);
      _active = _handle;
    }

    public static void Unbind()
    {
      GL.UseProgram(0);
      _active = 0;
    }

    public int GetAttribLocation(string attribName)
    {
      return GL.GetAttribLocation(_handle, attribName);
    }

    /// <summary>
    ///   Set a uniform int on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void SetInt(string name, int data)
    {
      if (!_uniformLocations.ContainsKey(name)) return;
      Bind();
      GL.Uniform1(_uniformLocations[name], data);
    }

    /// <summary>
    ///   Set a uniform float on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void SetFloat(string name, float data)
    {
      if (!_uniformLocations.ContainsKey(name)) return;
      Bind();
      GL.Uniform1(_uniformLocations[name], data);
    }

    /// <summary>
    ///   Set a uniform Matrix4 on this shader
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    /// <remarks>
    ///   <para>
    ///     The matrix is transposed before being sent to the shader.
    ///   </para>
    /// </remarks>
    public void SetMatrix4(string name, Matrix4 data)
    {
      if (!_uniformLocations.ContainsKey(name)) return;
      Bind();
      GL.UniformMatrix4(_uniformLocations[name], true, ref data);
    }

    /// <summary>
    ///   Set a uniform Vector3 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void SetVector3(string name, Vector3 data)
    {
      if (!_uniformLocations.ContainsKey(name)) return;
      Bind();
      GL.Uniform3(_uniformLocations[name], data);
    }

    /// <summary>
    ///   Set a uniform Vector2 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void SetVector2(string name, Vector2 data)
    {
      if (!_uniformLocations.ContainsKey(name)) return;
      Bind();
      GL.Uniform2(_uniformLocations[name], data);
    }

    /// <summary>
    ///   Set a uniform Vector2 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void SetVector4(string name, Vector4 data)
    {
      if (!_uniformLocations.ContainsKey(name)) return;
      Bind();
      GL.Uniform4(_uniformLocations[name], data);
    }
  }
}