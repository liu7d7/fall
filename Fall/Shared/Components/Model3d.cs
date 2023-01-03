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
    private readonly list<Matrix4> _models = new();

    private readonly Vector3[] _vertices;

    static model3d()
    {
      _shader = new shader("instanced", "john");
      _ubo = new ubo(_shader, "_instanceInfo", "_model");
    }

    private model3d(string path, Dictionary<string, uint> colors)
    {
      Color4 mat = new(1f, 1f, 1f, 1f);
      list<face> faces = new();
      list<Vector3> vertices = new();
      foreach (string line in File.ReadAllLines($"Resource/Model/{path}.obj"))
      {
        if (line.StartsWith("#")) continue;

        string[] parts = line.Split(' ');
        switch (parts[0])
        {
          case "v":
          {
            vertices.Add((ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3])));
            break;
          }
          case "o":
            mat = Shared.colors.NextColor();
            break;
          case "f":
          {
            string[] vt1 = parts[1].Split("/");
            string[] vt2 = parts[2].Split("/");
            string[] vt3 = parts[3].Split("/");
            face face = new()
            {
              [0] = new vertex_data(int.Parse(vt1[0]) - 1),
              [1] = new vertex_data(int.Parse(vt2[0]) - 1),
              [2] = new vertex_data(int.Parse(vt3[0]) - 1)
            };
            face[0].Color = face[1].Color = face[2].Color = (mat.R, mat.A);
            faces.Add(face);
            break;
          }
        }
      }

      _components[path + colors.ContentToString()] = this;
      _vertices = vertices.ToArray();
      _faces = faces.ToArray();

      _mesh = new mesh(
        mesh.draw_mode.TRIANGLE,
        _shader,
        false,
        vao.attrib.Float3, vao.attrib.Float2
      );
      ToMesh((0, 0, 0));
    }

    public void Scale(float scale)
    {
      Span<Vector3> vtx = _vertices;
      for (int i = 0; i < _vertices.Length; i++)
      {
        vtx[i].X *= scale;
        vtx[i].Y *= scale;
        vtx[i].Z *= scale;
      }

      ToMesh((0, 0, 0));
    }

    private float ParseFloat(string f)
    {
      return float.Parse(f, CultureInfo.InvariantCulture);
    }

    private void ToMesh(Vector3 pos)
    {
      _mesh.Begin();
      Span<face> faces = _faces;
      for (int i = 0; i < _faces.Length; i++)
      {
        Vector3 vt1 = _vertices[faces[i][0].Pos];
        Vector3 vt2 = _vertices[faces[i][1].Pos];
        Vector3 vt3 = _vertices[faces[i][2].Pos];
        int i1 = _mesh.Float3(vt1.X + pos.X, vt1.Y + pos.Y, vt1.Z + pos.Z).Float2(faces[i][0].Color).Next();
        int i2 = _mesh.Float3(vt2.X + pos.X, vt2.Y + pos.Y, vt2.Z + pos.Z).Float2(faces[i][1].Color).Next();
        int i3 = _mesh.Float3(vt3.X + pos.X, vt3.Y + pos.Y, vt3.Z + pos.Z).Float2(faces[i][2].Color).Next();
        _mesh.Tri(i1, i2, i3);
      }

      _mesh.End();
    }

    public void Render(Vector3 pos)
    {
      _models.Add(Matrix4.CreateTranslation(pos) * glh.Model);
    }

    public static void Draw()
    {
      foreach (KeyValuePair<string, model3d> pair in _components)
      {
        model3d model = pair.Value;
        if (model._models.Count == 0) continue;
        mesh mesh = model._mesh;

        Matrix4[] models = model._models.Items;

        int count = Math.Min(1024, model._models.Count);

        _shader.Bind();
        _ubo.PutAll(ref models, count, 0);
        _ubo.BindTo(0);
        mesh.RenderInstanced(count);

        shader.Unbind();
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
      private readonly model3d _model;
      private readonly float _rotation;

      public component(model3d model, float rot) : base(fall_obj.comp_type.Model3D)
      {
        _model = model;
        _rotation = rot;
      }

      public override void Render(fall_obj objIn)
      {
        base.Render(objIn);

        glh.Push();
        glh.Translate(-objIn.LerpedPos);
        glh.Rotate(_rotation, 0, 1, 0);
        glh.Translate(objIn.LerpedPos);
        _model.Render(objIn.LerpedPos);
        glh.Pop();
      }
    }
  }

  public class vertex_data
  {
    public Vector2 Color;
    public readonly int Pos;

    public vertex_data(int pos)
    {
      Pos = pos;
    }
  }

  public class face
  {
    private readonly vertex_data[] _vertices;

    public face()
    {
      _vertices = new vertex_data[3];
    }

    public vertex_data this[int idx]
    {
      get => _vertices[idx];
      init => _vertices[idx] = value;
    }
  }
}