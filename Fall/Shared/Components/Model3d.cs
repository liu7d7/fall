using System.Globalization;
using Fall.Engine;
using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
  public class model3d
  {
    private static readonly Dictionary<string, model3d> _components = new();
    private readonly Vector2[] _texCoords;

    public readonly face[] Faces;

    public readonly Vector3[] Vertices;

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
            mat = Shared.colors.NextColor().to_uint();
            break;
          case "f":
          {
            string[] vt1 = parts[1].Split("/");
            string[] vt2 = parts[2].Split("/");
            string[] vt3 = parts[3].Split("/");
            face face = new();
            face[0] = new vertex_data(int.Parse(vt1[0]) - 1, int.Parse(vt1[1]) - 1);
            face[1] = new vertex_data(int.Parse(vt2[0]) - 1, int.Parse(vt2[1]) - 1);
            face[2] = new vertex_data(int.Parse(vt3[0]) - 1, int.Parse(vt3[1]) - 1);
            face[0].Color = face[1].Color = face[2].Color = mat;
            face.Normal = normals[int.Parse(vt1[2]) - 1];
            faces.Add(face);
            break;
          }
        }
      }

      _components[path + colors.ContentToString()] = this;
      Vertices = vertices.ToArray();
      _texCoords = texCoords.ToArray();
      Faces = faces.ToArray();
      for (int i = 0; i < Faces.Length; i++) calculate_normals(ref Faces[i]);
    }

    public void Scale(float scale)
    {
      for (int i = 0; i < Vertices.Length; i++)
      {
        Vertices[i].X *= scale;
        Vertices[i].Y *= scale;
        Vertices[i].Z *= scale;
      }
    }

    public void Scale(float x, float y, float z)
    {
      for (int i = 0; i < Vertices.Length; i++)
      {
        Vertices[i].Y *= y;
        Vertices[i].X *= x;
        Vertices[i].Z *= z;
      }
    }

    public void flip_x()
    {
      for (int i = 0; i < Vertices.Length; i++) Vertices[i].X *= -1;

      for (int i = 0; i < Faces.Length; i++) calculate_normals(ref Faces[i]);
    }

    public void flip_y()
    {
      for (int i = 0; i < Vertices.Length; i++) Vertices[i].Y *= -1;

      for (int i = 0; i < Faces.Length; i++) calculate_normals(ref Faces[i]);
    }

    public void flip_z()
    {
      for (int i = 0; i < Vertices.Length; i++) Vertices[i].Z *= -1;

      for (int i = 0; i < Faces.Length; i++) calculate_normals(ref Faces[i]);
    }

    private float Fparse(string f)
    {
      return float.Parse(f, CultureInfo.InvariantCulture);
    }

    private void calculate_normals(ref face face)
    {
      Vector3 ab = Vertices[face[1].Pos] - Vertices[face[0].Pos];
      Vector3 ac = Vertices[face[2].Pos] - Vertices[face[0].Pos];
      Vector3 normal = Vector3.Cross(ab, ac);
      normal.Normalize();
      face.Normal = normal;
    }

    public void Render(Vector3 pos)
    {
      mesh mesh = render_system.MESH;
      foreach (face face in Faces)
      {
        Vector3 vt1 = Vertices[face[0].Pos];
        Vector3 vt2 = Vertices[face[1].Pos];
        Vector3 vt3 = Vertices[face[2].Pos];
        Vector2 uv1 = _texCoords[face[0].Uv];
        Vector2 uv2 = _texCoords[face[1].Uv];
        Vector2 uv3 = _texCoords[face[2].Uv];
        int i1 = mesh.Float3(render_system.Model, vt1.X + pos.X, vt1.Y + pos.Y, vt1.Z + pos.Z)
          .Float3(face.Normal).Float2(uv1).Float4(face[0].Color).Next();
        int i2 = mesh.Float3(render_system.Model, vt2.X + pos.X, vt2.Y + pos.Y, vt2.Z + pos.Z)
          .Float3(face.Normal).Float2(uv2).Float4(face[1].Color).Next();
        int i3 = mesh.Float3(render_system.Model, vt3.X + pos.X, vt3.Y + pos.Y, vt3.Z + pos.Z)
          .Float3(face.Normal).Float2(uv3).Float4(face[2].Color).Next();
        mesh.Tri(i1, i2, i3);
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

      public component(model3d model, func before, func after)
      {
        _model = model;
        _before = before;
        _after = after;
      }

      public component(model3d model, float rot)
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
        render_system.Translate(-objIn.Pos);
        render_system.Rotate(_rotation, 0, 1, 0);
        render_system.Translate(objIn.Pos);
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
    public readonly vertex_data[] Vertices;
    public Vector3 Normal;

    public face()
    {
      Vertices = new vertex_data[3];
    }

    public vertex_data this[int idx]
    {
      get => Vertices[idx];
      set => Vertices[idx] = value;
    }
  }
}