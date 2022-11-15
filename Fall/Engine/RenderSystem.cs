using OpenTK.Mathematics;
using Fall.Shared;
using Fall.Shared.Components;
using OpenTK.Graphics.OpenGL4;

namespace Fall.Engine
{
    public static class render_system
    {
        private static readonly shader _john = new("Resource/Shader/john.vert", "Resource/Shader/john.frag");
        public static readonly shader basic = new("Resource/Shader/basic.vert", "Resource/Shader/basic.frag");
        private static readonly shader _cubemap = new("Resource/Shader/cubemap.vert", "Resource/Shader/cubemap.frag");
        private static readonly shader _pixel = new("Resource/Shader/postprocess.vert", "Resource/Shader/pixelate.frag");
        private static readonly shader _fxaa = new("Resource/Shader/fxaa.vert", "Resource/Shader/fxaa.frag");
        private static readonly shader _line = new("Resource/Shader/lines.vert", "Resource/Shader/basic.frag");

        private static readonly shader
            _outline = new("Resource/Shader/postprocess.vert", "Resource/Shader/outline.frag");

        private static Matrix4 _projection;
        private static Matrix4 _lookAt;
        private static readonly Matrix4[] _model = new Matrix4[7];
        public static ref Matrix4 model => ref _model[_modelIdx];
        private static int _modelIdx;
        public static bool renderingRed;
        public static readonly font font = new(File.ReadAllBytes("Resource/Font/Dank Mono Italic.otf"), 20);
        public static readonly texture rect = texture.load_from_file("Resource/Texture/rect.png");

        public static readonly mesh mesh = new(mesh.draw_mode.triangle, _john, false, vao.attrib.float3, vao.attrib.float3,
            vao.attrib.float2, vao.attrib.float4);
        public static readonly mesh line = new(mesh.draw_mode.line, _line, false, vao.attrib.float3, vao.attrib.float4);

        private static readonly mesh _post = new(mesh.draw_mode.triangle, null, false, vao.attrib.float2);
        public static readonly fbo frame = new(size.X, size.Y, true);
        public static readonly fbo swap = new(size.X, size.Y, true);
        public static bool rendering3d;
        private static float_pos _camera;
        
        static render_system()
        {
            Array.Fill(_model, Matrix4.Identity);
            resize();
        }

        public static void resize()
        {
            _post.begin();
            _post.quad(
                _post.float2(0, 0).next(), 
                _post.float2(size.X, 0).next(), 
                _post.float2(size.X, size.Y).next(),
                _post.float2(0, size.Y).next()
            );
            _post.end();
        }

        public static void push()
        {
            _model[_modelIdx + 1] = model;
            _modelIdx++;
        }

        public static void pop()
        {
            _modelIdx--;
        }
        
        public static void translate(float x, float y, float z)
        {
            model.translate(x, y, z);
        }
        
        public static void translate(Vector3 vec)
        {
            model.translate(vec);
        }
        
        public static void rotate(float angle, float x, float y, float z)
        {
            model.rotate(angle, x, y, z);
        }
        
        public static void rotate(float angle, Vector3 vec)
        {
            model.rotate(angle, vec);
        }
        
        public static void scale(float x, float y, float z)
        {
            model.scale(x, y, z);
        }
        
        public static void scale(Vector3 vec)
        {
            model.scale(vec);
        }
        
        public static void scale(float scale)
        {
            model.scale(scale);
        }

        public static Vector2i size => fall.instance.Size;

        public static Vector4 to_vector4(this Color4 color)
        {
            return (color.R, color.G, color.B, color.A);
        }

        public static uint to_uint(this Color4 color)
        {
            return (uint)color.ToArgb();
        }

        public static void set_defaults(this shader shader)
        {
            shader.set_int("_renderingRed", renderingRed ? 1 : 0);
            shader.set_int("_rendering3d", rendering3d ? 1 : 0);
            shader.set_int("doLighting", 0);
            shader.set_vector2("_screenSize", (size.X, size.Y));
            shader.set_vector3("lightPos", (_camera.x + 5, _camera.y + 12, _camera.z + 5));
            shader.set_matrix4("_proj", _projection);
            shader.set_matrix4("_lookAt", _lookAt);
            shader.set_float("_time", Environment.TickCount / 1000f % (MathF.PI * 2f));
        }

        public static void render_pixelation(float pixWidth, float pixHeight)
        {
            frame.clear_color();
            frame.clear_depth();
            frame.bind();
            _pixel.bind();
            swap.bind_color(TextureUnit.Texture0);
            _pixel.set_int("_tex0", 0);
            _pixel.set_vector2("_screenSize", (size.X, size.Y));
            _pixel.set_vector2("_pixSize", (pixWidth, pixHeight));
            _post.render();
            shader.unbind();
        }

        public static void render_fxaa(fbo fbo)
        {
            fbo.blit(swap.handle);
            swap.bind();
            _fxaa.bind();
            fbo.bind_color(TextureUnit.Texture0);
            _fxaa.set_int("_tex0", 0);
            _fxaa.set_float("SpanMax", 8);
            _fxaa.set_float("ReduceMul", 0.125f);
            _fxaa.set_float("SubPixelShift", 0.25f);
            _fxaa.set_vector2("_screenSize", (size.X, size.Y));
            _post.render();
            shader.unbind();
            swap.blit(fbo.handle);
        }

        private const float threshold = 0.033f;
        private const float depthThreshold = 0.8f;

        public static void render_outline()
        {
            frame.clear_color();
            frame.clear_depth();
            frame.bind();
            _outline.bind();
            swap.bind_color(TextureUnit.Texture0);
            _outline.set_int("_tex0", 0);
            swap.bind_depth(TextureUnit.Texture1);
            _outline.set_int("_tex1", 1);
            _outline.set_int("_abs", 1);
            _outline.set_int("_glow", 0);
            _outline.set_int("_diffDepthCol", 0);
            _outline.set_int("_blackAndWhite", 1);
            _outline.set_float("_width", 0.75f);
            _outline.set_float("_threshold", threshold);
            _outline.set_float("_depthThreshold", depthThreshold);
            _outline.set_vector2("_screenSize", (size.X, size.Y));
            _outline.set_vector4("_outlineColor", Color4.HotPink.to_vector4());
            _outline.set_vector4("_otherColor", Color4.White.to_vector4());
            _post.render();
            shader.unbind();
        }

        public static void update_projection()
        {
            if (rendering3d)
            {
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), size.X / (float)size.Y, 0.1f, 128,
                    out _projection);
                return;
            }
            Matrix4.CreateOrthographic(size.X, size.Y, -1000, 3000, out _projection);
        }

        public static void update_look_at(fall_obj cameraObj, bool rendering3d = true)
        {
            if (!cameraObj.has<float_pos>())
            {
                return;
            }

            _camera = cameraObj.get<float_pos>();
            render_system.rendering3d = rendering3d;
            if (!render_system.rendering3d)
            {
                _lookAt = Matrix4.Identity;
                return;
            }

            camera comp = cameraObj.get<camera>();
            _lookAt = comp.get_camera_matrix();
        }
    }
}