using Fall.Shared;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Fall.Engine
{
    public class mesh
    {
        private readonly vao _vao;
        private readonly vbo _vbo;
        private readonly ibo _ibo;
        private readonly draw_mode _drawMode;
        private readonly shader _shader;
        private int _vertex;
        private int _index;
        private bool _building;
        private readonly bool _static;

        public mesh(draw_mode drawMode, shader shader, bool static_, params vao.attrib[] attribs)
        {
            _drawMode = drawMode;
            _shader = shader;
            int stride = attribs.Sum(attrib => (int) attrib * sizeof(float));
            _vbo = new vbo(stride * drawMode.size * 64 * sizeof(float), static_);
            _vbo.bind();
            _ibo = new ibo(drawMode.size * 128 * sizeof(float), static_);
            _ibo.bind();
            _vao = new vao(attribs);
            vbo.unbind();
            ibo.unbind();
            vao.unbind();
            _static = static_;
        }

        public int next()
        {
            return _vertex++;
        }

        public mesh float1(float p0)
        {
            _vbo.put(p0);
            return this;
        }

        public mesh float2(float p0, float p1)
        {
            _vbo.put(p0);
            _vbo.put(p1);
            return this;
        }
        
        public mesh float2(Vector2 p0)
        {
            _vbo.put(p0.X);
            _vbo.put(p0.Y);
            return this;
        }

        public mesh float3(float p0, float p1, float p2)
        {
            _vbo.put(p0);
            _vbo.put(p1);
            _vbo.put(p2);
            return this;
        }

        public mesh float3(Matrix4 transform, float p0, float p1, float p2)
        {
            Vector4 pos = new(p0, p1, p2, 1);
            pos.transform(transform);
            _vbo.put(pos.X);
            _vbo.put(pos.Y);
            _vbo.put(pos.Z);
            return this;
        }
        
        public mesh float3(Vector3 p0)
        {
            _vbo.put(p0.X);
            _vbo.put(p0.Y);
            _vbo.put(p0.Z);
            return this;
        }

        public mesh float4(float p0, float p1, float p2, float p3)
        {
            _vbo.put(p0);
            _vbo.put(p1);
            _vbo.put(p2);
            _vbo.put(p3);
            return this;
        }

        public mesh float4(uint color)
        {
            return float4(((color >> 16) & 0xff) * 0.003921569f, ((color >> 8) & 0xff) * 0.003921569f, (color & 0xff) * 0.003921569f, ((color >> 24) & 0xff) * 0.003921569f);
        }

        public mesh float4(uint color, float alpha)
        {
            return float4(((color >> 16) & 0xff) * 0.003921569f, ((color >> 8) & 0xff) * 0.003921569f, (color & 0xff) * 0.003921569f, alpha);
        }

        public void single(int p0)
        {
            _ibo.put(p0);
            _index++;
        }
        
        public void line(int p0, int p1)
        {
            _ibo.put(p0);
            _ibo.put(p1);
            _index += 2;
        }

        public void tri(int p0, int p1, int p2)
        {
            _ibo.put(p0);
            _ibo.put(p1);
            _ibo.put(p2);
            _index += 3;
        }
        
        public void quad(int p0, int p1, int p2, int p3)
        {
            _ibo.put(p0);
            _ibo.put(p1);
            _ibo.put(p2);
            _ibo.put(p2);
            _ibo.put(p3);
            _ibo.put(p0);
            _index += 6;
        }

        public void begin()
        {
            if (_building)
            {
                throw new Exception("Already building");
            }
            if (!_static)
            {
                _vbo.clear();
                _ibo.clear();
            }
            _vertex = 0;
            _index = 0;
            _building = true;
        }

        public void end()
        {
            if (!_building)
            {
                throw new Exception("Not building");
            }

            if (_index > 0)
            {
                _vbo.upload();
                _ibo.upload();
            }

            if (_static)
            {
                _vbo.clear();
                _ibo.clear();
            }

            _building = false;
        }

        public void render()
        {
            if (_building)
            {
                end();
            }

            if (_index <= 0) return;
            gl_state_manager.save_state();
            gl_state_manager.enable_blend();
            if (render_system.rendering3d)
            {
                gl_state_manager.enable_depth();
            }
            else
            {
                gl_state_manager.disable_depth();
            }
            _shader?.bind();
            _shader?.set_defaults();
            _vao.bind();
            _ibo.bind();
            _vbo.bind();
            GL.DrawElements(_drawMode.as_gl(), _index, DrawElementsType.UnsignedInt, 0);
            ibo.unbind();
            vbo.unbind();
            vao.unbind();
            gl_state_manager.restore_state();
        }

        public sealed class draw_mode
        {
            private readonly BeginMode _mode;
            public readonly int size;

            private draw_mode(int size, BeginMode mode)
            {
                this.size = size;
                _mode = mode;
            }

            public override bool Equals(object obj)
            {
                if (obj is draw_mode mode)
                {
                    return _mode == mode._mode;
                }

                return false;
            }
        
            public override int GetHashCode()
            {
                return _mode.GetHashCode();
            }

            public BeginMode as_gl()
            {
                return _mode;
            }

            public static readonly draw_mode line = new(2, BeginMode.Lines);
            public static readonly draw_mode triangle = new(3, BeginMode.Triangles);
            public static readonly draw_mode triangleStrip = new(2, BeginMode.TriangleStrip);
        }
    }
}