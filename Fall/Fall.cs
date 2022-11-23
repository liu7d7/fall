using System.Drawing;
using Fall.Engine;
using Fall.Shared;
using Fall.Shared.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Fall
{
  public class fall : GameWindow
  {
    public const uint PINK = 0xffff0094;
    public static fall Instance;
    public static fall_obj Player;
    public static world World;
    public static float MouseDx, MouseDy, MouseX, MouseY;
    public static int Ticks;
    public static int InView;

    private readonly Color4 c = colors.NextColor();
    private readonly rolling_avg _mspf = new(300);
    private int _lastInquiry;
    private int _memUsage;
    private bool _outline;

    public fall(GameWindowSettings windowSettings, NativeWindowSettings nativeWindowSettings) : base(windowSettings,
      nativeWindowSettings)
    {
      Instance = this;
      GL.Enable(EnableCap.Multisample);
      render_system.Resize();
      CreateWorld();
    }

    private static void CreateWorld()
    {
      void placeTrees()
      {
        model3d model = model3d.Read("tree", new Dictionary<string, uint>());
        model.Scale(4f);
        for (int i = -100; i <= 100; i++)
        for (int j = -100; j <= 100; j++)
        {
          fall_obj obj = new()
          {
            Updates = true
          };
          model3d.component comp = new(model, rand.NextFloat() * 180);
          float_pos pos = new()
          {
            X = i * 50 + (rand.NextFloat() - 0.5f) * 50,
            Z = j * 50 + (rand.NextFloat() - 0.5f) * 50
          };

          pos.Y = world.height_at((pos.X, pos.Z)) - 2f;
          pos.PrevX = pos.X;
          pos.PrevY = pos.Y;
          pos.PrevZ = pos.Z;
          obj.Add(comp);
          obj.Add(pos);
          obj.Add(new tree());
          World.Objs.Add(obj);
        }
      }

      void makePlayer()
      {
        Player = new fall_obj
        {
          Updates = true
        };
        Player.Add(new player());
        Player.Add(new float_pos());
        Player.Add(new camera());
        float_pos pos = Player.Get<float_pos>();
        pos.Yaw = pos.PrevYaw = 180;
        pos.X = pos.PrevX = pos.Z = pos.PrevZ = -1;
        pos.Y = pos.PrevY = 25;
        Player.Update();
      }

      makePlayer();
      World = new world();
      World.Objs.Add(Player);

      placeTrees();

      World.Update();
    }

    protected override void OnLoad()
    {
      base.OnLoad();

      GLFW.SwapInterval(0);
      GL.DepthFunc(DepthFunction.Lequal);
      gl_state_manager.enable_blend();

      ticker.Init();

      CursorState = CursorState.Grabbed;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
      base.OnResize(e);

      if (e.Size == Vector2i.Zero)
        return;

      render_system.update_projection();
      render_system.Resize();
      GL.Viewport(new Rectangle(0, 0, Size.X, Size.Y));
      fbo.Resize(Size.X, Size.Y);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
      base.OnRenderFrame(args);

      Player.Get<camera>().update_camera_vectors();
      
      fbo.Unbind();
      GL.ClearColor(c);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
      render_system.FRAME.clear_color();
      render_system.FRAME.clear_depth();
      render_system.FRAME.Bind();
      GL.ClearColor(0f, 0f, 0f, 0f);

      render_system.update_look_at(Player);
      render_system.update_projection();

      render_system.RECT.Bind(TextureUnit.Texture0);
      render_system.MESH.Begin();
      World.Render();
      render_system.MESH.Render();
      texture.Unbind();

      render_system.FRAME.Bind();

      render_system.FRAME.Blit(render_system.SWAP.Handle);
      if (_outline)
        render_system.render_outline();
      render_system.render_fxaa(render_system.FRAME);

      fbo.Unbind();

      render_system.FRAME.Blit();

      render_system.update_look_at(Player, false);
      render_system.update_projection();
      font.Bind();
      render_system.RenderingRed = true;
      render_system.MESH.Begin();
      _mspf.Add(args.Time);
      if (Environment.TickCount - _lastInquiry > 1000)
      {
        _lastInquiry = Environment.TickCount;
        _memUsage = (int)(GC.GetTotalMemory(false) / (1024 * 1024));
      }

      font.Draw(render_system.MESH, $"mspf: {_mspf.Average:N4} | fps: {1f / _mspf.Average:N2}"
        , -Size.X / 2f + 11, Size.Y / 2f - 8, PINK, false);
      font.Draw(render_system.MESH, $"_time: {Environment.TickCount / 1000f % (MathF.PI * 2f):N2}",
        -Size.X / 2f + 11, Size.Y / 2f - 28, PINK, false);
      font.Draw(render_system.MESH, $"xyz: {Player.Pos.X:N2}; {Player.Pos.Y:N2}; {Player.Pos.Z:N2}",
        -Size.X / 2f + 11, Size.Y / 2f - 48,
        PINK, false);
      font.Draw(render_system.MESH, $"heap: {_memUsage}M", -Size.X / 2f + 11, Size.Y / 2f - 68,
        PINK, false);
      render_system.MESH.Render();
      render_system.RenderingRed = false;
      font.Unbind();

      SwapBuffers();
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
      MouseDx += e.DeltaX;
      MouseDy += e.DeltaY;
      MouseX = e.X;
      MouseY = e.Y;
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
      base.OnUpdateFrame(args);

      int i = ticker.Update();

      if (KeyboardState.IsKeyDown(Keys.Escape))
      {
        Player.Get<camera>().FirstMouse = true;
        CursorState = CursorState.Normal;
      }

      if (MouseState.WasButtonDown(MouseButton.Left)) CursorState = CursorState.Grabbed;

      if (KeyboardState.IsKeyPressed(Keys.O)) _outline = !_outline;

      for (int j = 0; j < Math.Min(i, 10); j++)
      {
        Ticks++;

        World.Update();
        MouseDx = MouseDy = 0;
      }
    }
  }
}