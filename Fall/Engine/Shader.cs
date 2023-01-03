using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Fall.Engine
{
  // taken from https://github.com/opentk/LearnOpenTK/blob/master/Common/Shader.cs
  public class shader
  {
    private readonly Dictionary<string, int> _uniformLocations;
    public readonly int Handle;

    public shader(string vertPath, string fragPath, string geomPath = null)
    {
      string shaderSource = File.ReadAllText($"Resource/Shader/{vertPath}.vert");
      int vertexShader = GL.CreateShader(ShaderType.VertexShader);
      GL.ShaderSource(vertexShader, shaderSource);
      CompileShader(vertexShader);

      shaderSource = File.ReadAllText($"Resource/Shader/{fragPath}.frag");
      int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
      GL.ShaderSource(fragmentShader, shaderSource);
      CompileShader(fragmentShader);

      int geometryShader = -1;
      if (geomPath != null)
      {
        shaderSource = File.ReadAllText($"Resource/Shader/{geomPath}.geom");
        geometryShader = GL.CreateShader(ShaderType.GeometryShader);
        GL.ShaderSource(geometryShader, shaderSource);
        CompileShader(geometryShader);
      }

      Handle = GL.CreateProgram();

      GL.AttachShader(Handle, vertexShader);
      GL.AttachShader(Handle, fragmentShader);
      if (geomPath != null) GL.AttachShader(Handle, geometryShader);

      LinkProgram(Handle);

      GL.DetachShader(Handle, vertexShader);
      GL.DeleteShader(vertexShader);
      GL.DetachShader(Handle, fragmentShader);
      GL.DeleteShader(fragmentShader);
      if (geomPath != null)
      {
        GL.DetachShader(Handle, geometryShader);
        GL.DeleteShader(geometryShader);
      }

      GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);

      _uniformLocations = new Dictionary<string, int>();

      for (int i = 0; i < numberOfUniforms; i++)
      {
        string key = GL.GetActiveUniform(Handle, i, out _, out _);
        int location = GL.GetUniformLocation(Handle, key);
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
      GL.UseProgram(Handle);
    }

    public static void Unbind()
    {
      GL.UseProgram(0);
    }

    public void SetInt(string name, int data)
    {
      if (!_uniformLocations.ContainsKey(name)) return;
      Bind();
      GL.Uniform1(_uniformLocations[name], data);
    }

    public void SetFloat(string name, float data)
    {
      if (!_uniformLocations.ContainsKey(name)) return;
      Bind();
      GL.Uniform1(_uniformLocations[name], data);
    }

    public void SetMatrix4(string name, Matrix4 data)
    {
      if (!_uniformLocations.ContainsKey(name)) return;
      Bind();
      GL.UniformMatrix4(_uniformLocations[name], true, ref data);
    }

    public void SetVector3(string name, Vector3 data)
    {
      if (!_uniformLocations.ContainsKey(name)) return;
      Bind();
      GL.Uniform3(_uniformLocations[name], data);
    }

    public void SetVector2(string name, Vector2 data)
    {
      if (!_uniformLocations.ContainsKey(name)) return;
      Bind();
      GL.Uniform2(_uniformLocations[name], data);
    }

    public void SetVector4(string name, Vector4 data)
    {
      if (!_uniformLocations.ContainsKey(name)) return;
      Bind();
      GL.Uniform4(_uniformLocations[name], data);
    }
  }
}