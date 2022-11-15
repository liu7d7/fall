using System.Globalization;
using Fall.Engine;
using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
    public class model3d
    {
        private static readonly Dictionary<string, model3d> _components = new();

        private readonly face[] _faces;

        public class component : fall_obj.component
        {
            private readonly model3d _model;
            private readonly func _before = empty;
            private readonly func _after = empty;

            public delegate void func(fall_obj objIn);

            private static void empty(fall_obj obj)
            {
            }

            public component(model3d model, func before, func after)
            {
                _model = model;
                _before = before;
                _after = after;
            }
            
            public component(model3d model)
            {
                _model = model;
            }

            public override void render(fall_obj objIn)
            {
                base.render(objIn);

                _before(objIn);
                _model.render(objIn.get<float_pos>().to_vector3());
                _after(objIn);
            }
        }

        public void scale(float scale)
        {
            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i].X *= scale;
                _vertices[i].Y *= scale;
                _vertices[i].Z *= scale;
            }
        }
        
        public void scale(float x, float y, float z)
        {
            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i].Y *= y;
                _vertices[i].X *= x;
                _vertices[i].Z *= z;
            }
        }

        public void flip_x()
        {
            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i].X *= -1;
            }

            for (int i = 0; i < _faces.Length; i++)
            {
                calculate_normals(ref _faces[i]);
            }
        }
        
        public void flip_y()
        {
            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i].Y *= -1;
            }
            
            for (int i = 0; i < _faces.Length; i++)
            {
                calculate_normals(ref _faces[i]);
            }
        }
        
        public void flip_z()
        {
            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i].Z *= -1;
            }
            
            for (int i = 0; i < _faces.Length; i++)
            {
                calculate_normals(ref _faces[i]);
            }
        }

        private float fparse(string f)
        {
            return float.Parse(f, CultureInfo.InvariantCulture);
        }
        
        private readonly Vector3[] _vertices;
        private readonly Vector2[] _texCoords;

        private model3d(string path, Dictionary<string, uint> colors)
        {
            int mat = 0;
            List<face> faces = new();
            List<Vector3> vertices = new();
            List<Vector3> normals = new();
            List<Vector2> texCoords = new();
            foreach (string line in File.ReadAllLines($"Resource/Model/{path}.obj"))
            {
                if (line.StartsWith("#"))
                {
                    continue;
                }

                string[] parts = line.Split(' ');
                switch (parts[0])
                {
                    case "v":
                    {
                        vertices.Add((fparse(parts[1]), fparse(parts[2]), fparse(parts[3])));
                        break;
                    }
                    
                    case "vn":
                    { 
                        normals.Add((fparse(parts[1]), fparse(parts[2]), fparse(parts[3])));
                        break;
                    }
                    case "vt":
                    {
                        texCoords.Add((fparse(parts[1]), fparse(parts[2])));
                        break;
                    }
                    case "o":
                        mat = deterministic_random.next_int(parts[1], 100000);
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
                        uint val = Fall.Shared.colors.get_random_color4(mat).to_uint();
                        face[0].color = face[1].color = face[2].color = val;
                        face.normal = normals[int.Parse(vt1[2]) - 1];
                        faces.Add(face);
                        break;
                    }
                }
            }
            _components[path + colors.content_to_string()] = this;
            _vertices = vertices.ToArray();
            _texCoords = texCoords.ToArray();
            _faces = faces.ToArray();
            for (int i = 0; i < _faces.Length; i++)
            {
                calculate_normals(ref _faces[i]);
            }
        }

        private void calculate_normals(ref face face)
        {
            Vector3 ab = _vertices[face[1].pos] - _vertices[face[0].pos];
            Vector3 ac = _vertices[face[2].pos] - _vertices[face[0].pos];
            Vector3 normal = Vector3.Cross(ab, ac);
            normal.Normalize();
            face.normal = normal;
        }

        public void render(Vector3 pos)
        {
            mesh mesh = render_system.mesh;
            foreach (face face in _faces)
            {
                Vector3 vt1 = _vertices[face[0].pos];
                Vector3 vt2 = _vertices[face[1].pos];
                Vector3 vt3 = _vertices[face[2].pos];
                Vector2 uv1 = _texCoords[face[0].uv];
                Vector2 uv2 = _texCoords[face[1].uv];
                Vector2 uv3 = _texCoords[face[2].uv];
                int i1 = mesh.float3(render_system.model, vt1.X + pos.X, vt1.Y + pos.Y, vt1.Z + pos.Z).float3(face.normal).float2(uv1).float4(face[0].color).next();
                int i2 = mesh.float3(render_system.model, vt2.X + pos.X, vt2.Y + pos.Y, vt2.Z + pos.Z).float3(face.normal).float2(uv2).float4(face[1].color).next();
                int i3 = mesh.float3(render_system.model, vt3.X + pos.X, vt3.Y + pos.Y, vt3.Z + pos.Z).float3(face.normal).float2(uv3).float4(face[2].color).next();
                mesh.tri(i1, i2, i3);
            }
        }

        public static model3d read(string path, Dictionary<string, uint> colors)
        {
            return _components.ContainsKey(path + colors.content_to_string()) ? _components[path + colors.content_to_string()] : new model3d(path, colors);
        }
    }
    
    public class vertex_data
    {
        public int pos;
        public int uv;
        public uint color = 0xffffffff;
        
        public vertex_data(int pos, int uv)
        {
            this.pos = pos;
            this.uv = uv;
        }
    }

    public class face
    {
        public Vector3 normal;
        public vertex_data[] vertices;

        public vertex_data this[int idx]
        {
            get => vertices[idx];
            set => vertices[idx] = value;
        }

        public face()
        {
            vertices = new vertex_data[3];
        }
    }
}