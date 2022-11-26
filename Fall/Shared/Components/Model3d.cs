using System.Globalization;
using Fall.Engine;
using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
  public class model3d
  {
    private static readonly Dictionary<string, model3d> _components = new();
    private static readonly shader _shader;
    private static readonly ubo _ubo;
    private readonly face[] _faces;
    private readonly mesh _mesh;
    private readonly List<Matrix4> _models = new();

    private readonly Vector2[] _texCoords;
    private readonly Vector3[] _vertices;

    static model3d()
    {
      _shader = new shader("Resource/Shader/instanced.vert", "Resource/Shader/john.frag");
      _ubo = new ubo(_shader, "_instanceInfo", "_model");
    }

    private model3d(string path, Dictionary<string, uint> colors)
    {
      uint mat = 0;
      List<face> faces = new();
      List<Vector3> vertices = new();
      List<Vector3> normals = new();
      List<Vector2> texCoords = new();
      foreach (string line in File.ReadAllLines($"Resource/Model/{path}.obj"))
      {
        if (line.StartsWith("#")) continue;

        string[] parts = line.Split(' ');
        switch (parts[0])
        {
          case "v":
          {
            vertices.Add((Fparse(parts[1]), Fparse(parts[2]), Fparse(parts[3])));
            break;
          }

          case "vn":
          {
            normals.Add((Fparse(parts[1]), Fparse(parts[2]), Fparse(parts[3])));
            break;
          }
          case "vt":
          {
            texCoords.Add((Fparse(parts[1]), Fparse(parts[2])));
            break;
          }
          case "o":
            mat = Shared.colors.NextColor().ToUInt();
            break;
          case "f":
          {
            string[] vt1 = parts[1].Split("/");
            string[] vt2 = parts[2].Split("/");
            string[] vt3 = parts[3].Split("/");
            face face = new()
            {
              [0] = new(int.Parse(vt1[0]) - 1, int.Parse(vt1[1]) - 1),
              [1] = new(int.Parse(vt2[0]) - 1, int.Parse(vt2[1]) - 1),
              [2] = new(int.Parse(vt3[0]) - 1, int.Parse(vt3[1]) - 1)
            };
            face[0].Color = face[1].Color = face[2].Color = mat;
            face.Normal = normals[int.Parse(vt1[2]) - 1];
            faces.Add(face);
            break;
          }
        }
      }

      _components[path + colors.ContentToString()] = this;
      _vertices = vertices.ToArray();
      _texCoords = texCoords.ToArray();
      _faces = faces.ToArray();
      for (int i = 0; i < _faces.Length; i++) CalculateNormals(ref _faces[i]);

      _mesh = new mesh(
        mesh.draw_mode.TRIANGLE,
        _shader,
        false,
        vao.attrib.FLOAT3,
        vao.attrib.FLOAT3,
        vao.attrib.FLOAT2,
        vao.attrib.FLOAT4
      );
      ToMesh((0, 0, 0));
    }

    public void Scale(float scale)
    {
      for (int i = 0; i < _vertices.Length; i++)
      {
        _vertices[i].X *= scale;
        _vertices[i].Y *= scale;
        _vertices[i].Z *= scale;
      }

      ToMesh((0, 0, 0));
    }

    public void Scale(float x, float y, float z)
    {
      for (int i = 0; i < _vertices.Length; i++)
      {
        _vertices[i].Y *= y;
        _vertices[i].X *= x;
        _vertices[i].Z *= z;
      }

      ToMesh((0, 0, 0));
    }

    public void FlipX()
    {
      for (int i = 0; i < _vertices.Length; i++) _vertices[i].X *= -1;

      for (int i = 0; i < _faces.Length; i++) CalculateNormals(ref _faces[i]);
      ToMesh((0, 0, 0));
    }

    public void FlipY()
    {
      for (int i = 0; i < _vertices.Length; i++) _vertices[i].Y *= -1;

      for (int i = 0; i < _faces.Length; i++) CalculateNormals(ref _faces[i]);
      ToMesh((0, 0, 0));
    }

    public void FlipZ()
    {
      for (int i = 0; i < _vertices.Length; i++) _vertices[i].Z *= -1;

      for (int i = 0; i < _faces.Length; i++) CalculateNormals(ref _faces[i]);
      ToMesh((0, 0, 0));
    }

    private float Fparse(string f)
    {
      return float.Parse(f, CultureInfo.InvariantCulture);
    }

    private void CalculateNormals(ref face face)
    {
      Vector3 ab = _vertices[face[1].Pos] - _vertices[face[0].Pos];
      Vector3 ac = _vertices[face[2].Pos] - _vertices[face[0].Pos];
      Vector3 normal = Vector3.Cross(ab, ac);
      normal.Normalize();
      face.Normal = normal;
    }

    private void ToMesh(Vector3 pos)
    {
      _mesh.Begin();
      foreach (face face in _faces)
      {
        Vector3 vt1 = _vertices[face[0].Pos];
        Vector3 vt2 = _vertices[face[1].Pos];
        Vector3 vt3 = _vertices[face[2].Pos];
        Vector2 uv1 = _texCoords[face[0].Uv];
        Vector2 uv2 = _texCoords[face[1].Uv];
        Vector2 uv3 = _texCoords[face[2].Uv];
        int i1 = _mesh.Float3(vt1.X + pos.X, vt1.Y + pos.Y, vt1.Z + pos.Z).Float3(face.Normal).Float2(uv1)
          .Float4(face[0].Color).Next();
        int i2 = _mesh.Float3(vt2.X + pos.X, vt2.Y + pos.Y, vt2.Z + pos.Z).Float3(face.Normal).Float2(uv2)
          .Float4(face[1].Color).Next();
        int i3 = _mesh.Float3(vt3.X + pos.X, vt3.Y + pos.Y, vt3.Z + pos.Z).Float3(face.Normal).Float2(uv3)
          .Float4(face[2].Color).Next();
        _mesh.Tri(i1, i2, i3);
      }

      _mesh.End();
    }

    public void Render(Vector3 pos)
    {
      _models.Add(Matrix4.CreateTranslation(pos) * render_system.Model);
    }

    public static void Draw()
    {
      foreach (KeyValuePair<string, model3d> pair in _components)
      {
        model3d model = pair.Value;
        if (model._models.Count == 0) continue;
        mesh mesh = model._mesh;

        Matrix4[] models = model._models.GetInternalArray();

        int count = Math.Min(1024, model._models.Count);

        _shader.Bind();
        _ubo.PutAll(ref models, count, 0);

        _ubo.BindTo(0);
        mesh.RenderInstanced(count);

        shader.Unbind();
        _ubo.Clear();
        model._models.Clear();
      }
    }

    public static model3d Read(string path, Dictionary<string, uint> colors)
    {
      return _components.ContainsKey(path + colors.ContentToString())
        ? _components[path + colors.ContentToString()]
        : new model3d(path, colors);
    }

    public class component : fall_obj.component
    {
      public delegate void func(fall_obj objIn);

      private readonly func _after = Empty;
      private readonly func _before = Empty;
      private readonly model3d _model;
      private readonly float _rotation;

      public component(model3d model, func before, func after) : base(type.MODEL_3D)
      {
        _model = model;
        _before = before;
        _after = after;
      }

      public component(model3d model, float rot) : base(type.MODEL_3D)
      {
        _model = model;
        _rotation = rot;
      }

      private static void Empty(fall_obj obj)
      {
      }

      public override void Render(fall_obj objIn)
      {
        base.Render(objIn);

        _before(objIn);
        render_system.Push();
        render_system.Translate(-objIn.LerpedPos);
        render_system.Rotate(_rotation, 0, 1, 0);
        render_system.Translate(objIn.LerpedPos);
        _model.Render(objIn.LerpedPos);
        render_system.Pop();
        _after(objIn);
      }
    }
  }

  public class vertex_data
  {
    public uint Color = 0xffffffff;
    public int Pos;
    public int Uv;

    public vertex_data(int pos, int uv)
    {
      Pos = pos;
      Uv = uv;
    }
  }

  public class face
  {
    private readonly vertex_data[] _vertices;
    public Vector3 Normal;

    public face()
    {
      _vertices = new vertex_data[3];
    }

    public vertex_data this[int idx]
    {
      get => _vertices[idx];
      set => _vertices[idx] = value;
    }
  }
}