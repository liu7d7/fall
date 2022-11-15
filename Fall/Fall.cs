using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Fall.Engine;
using Fall.Shared;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Fall.Shared.Components;

namespace Fall
{
    public class fall : GameWindow
    {
        private static int _ticks;
        public static fall instance;
        public static model3d model;
        public static fall_obj player;
        public static float mouseDx, mouseDy, mouseX, mouseY;
        public static world world;

        public fall(GameWindowSettings windowSettings, NativeWindowSettings nativeWindowSettings) : base(windowSettings, nativeWindowSettings)
        {
            instance = this;
            model = model3d.read("tree", new Dictionary<string, uint>());
            model.scale(2f);
            player = new fall_obj();
            player.add(new player());
            player.add(new float_pos
            {
                x = 0, z = 0, prevX = 0, prevZ = 0
            });
            player.add(new camera());
            player.add(new float_pos());
            float_pos pos = player.get<float_pos>();
            pos.yaw = pos.prevYaw = 180;
            pos.x = pos.prevX = pos.z = pos.prevZ = -1;
            pos.y = pos.prevY = 25;
            player.update();
            world = new world();
            world.update();
        }
        
        protected override void OnLoad()
        {
            base.OnLoad();

            GLFW.SwapInterval(0);
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.DepthFunc(DepthFunction.Lequal);
            gl_state_manager.enable_blend();

            ticker.init();
            
            CursorState = CursorState.Grabbed;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            
            if (e.Size == Vector2i.Zero)
                return;

            render_system.update_projection();
            render_system.resize();
            GL.Viewport(new Rectangle(0, 0, Size.X, Size.Y));
            fbo.resize(Size.X, Size.Y);
        }

        private bool _outline;
        private int _memUsage;
        private float _mspf;
        private float _fps;
        private int _lastInquiry;

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            
            player.get<camera>().update_camera_vectors();

            render_system.frame.clear_color();
            render_system.frame.clear_depth();
            render_system.frame.bind();
            
            render_system.update_look_at(player);
            render_system.update_projection();
            
            world.render();

            render_system.rect.bind(TextureUnit.Texture0);
            render_system.mesh.begin();
            model.render((0, -1, 0));
            player.render();
            render_system.mesh.render();
            texture.unbind();
            
            render_system.frame.bind();

            render_system.frame.blit(render_system.swap.handle);
            if (_outline)
                render_system.render_outline();
            render_system.render_fxaa(render_system.frame);
            
            fbo.unbind();

            render_system.frame.blit();

            render_system.update_look_at(player, false);
            render_system.update_projection();
            render_system.font.bind();
            render_system.renderingRed = true;
            render_system.mesh.begin();
            if (Environment.TickCount - _lastInquiry > 1000)
            {
                _mspf = (float)args.Time;
                _fps = 1 / _mspf;
                _lastInquiry = Environment.TickCount;
                _memUsage = (int)(GC.GetTotalMemory(false) / (1024 * 1024));
            }
            render_system.font.draw(render_system.mesh, $"mspf: {_mspf:C4} | fps: {_fps:C2}"
                , -Size.X / 2f + 11, Size.Y / 2f - 8, Color4.HotPink.to_uint(), true);
            render_system.font.draw(render_system.mesh, $"_time: {Environment.TickCount / 1000f % (MathF.PI * 2f):C2}", -Size.X / 2f + 11, Size.Y / 2f - 28, Color4.HotPink.to_uint(), true);
            render_system.font.draw(render_system.mesh, $"xyz: {player.pos}", -Size.X / 2f + 11, Size.Y / 2f - 48, Color4.HotPink.to_uint(), true);
            render_system.font.draw(render_system.mesh, $"heap: {_memUsage}M", -Size.X / 2f + 11, Size.Y / 2f - 68, Color4.HotPink.to_uint(), true);
            render_system.mesh.render();
            render_system.renderingRed = false;
            font.unbind();
            
            SwapBuffers();
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            mouseDx += e.DeltaX;
            mouseDy += e.DeltaY;
            mouseX = e.X;
            mouseY = e.Y;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            int i = ticker.update();
            
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                player.get<camera>().firstMouse = true;
                CursorState = CursorState.Normal;
            }

            if (MouseState.WasButtonDown(MouseButton.Left))
            {
                CursorState = CursorState.Grabbed;
            }

            if (KeyboardState.IsKeyPressed(Keys.O))
            {
                _outline = !_outline;
            }
            
            world.update();
            
            for (int j = 0; j < Math.Min(i, 10); j++)
            {
                _ticks++;

                player.update();
                mouseDx = mouseDy = 0;
            }
        }
    }
}