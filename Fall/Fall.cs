using System.Drawing;
using System.Runtime.InteropServices;
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
    public static uint Ticks;
    public static uint Frames; 
    public static int InView;
    public static bool FarCamera;

    private static readonly DebugProc _debugProcCallback = DebugCallback;
    private static GCHandle _debugProcCallbackHandle;
    private readonly Color4 _c = colors.NextColor();
    private readonly rolling_avg _mspf = new(180);
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

      _debugProcCallbackHandle = GCHandle.Alloc(_debugProcCallback);
      GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
      GL.Enable(EnableCap.DebugOutput);
      GL.Enable(EnableCap.DebugOutputSynchronous);
    }

    private static void DebugCallback(DebugSource source, DebugType type, int id,
      DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
    {
      string messageString = Marshal.PtrToStringAnsi(message, length);
      Console.WriteLine($"{severity} {type} | {messageString}");

      if (type == DebugType.DebugTypeError)
        throw new Exception(messageString);
    }

    private static void CreateWorld()
    {
      void placeTrees()
      {
        model3d[] models = new model3d[5];
        {
          int i = 0;
          foreach (string str in new[]
                     { "large_tree", "large_tree_1", "large_tree_3", "large_tree_4", "large_tree_5" })
          {
            model3d model = model3d.Read(str, new Dictionary<string, uint>());
            model.Scale(16f);
            models[i] = model;
            i++;
          }
        }
        for (int i = -100; i <= 100; i++)
        for (int j = -100; j <= 100; j++)
        {
          fall_obj obj = new()
          {
            Updates = true
          };
          model3d.component comp = new(models[rand.Next(0, 5)], rand.NextFloat() * 180);
          float_pos pos = new()
          {
            X = i * 50 + (rand.NextFloat() - 0.5f) * 50,
            Z = j * 50 + (rand.NextFloat() - 0.5f) * 50
          };

          pos.Y = world.HeightAt((pos.X, pos.Z)) - 2f;
          pos.PrevX = pos.X;
          pos.PrevY = pos.Y;
          pos.PrevZ = pos.Z;
          obj.Add(comp);
          obj.Add(pos);
          obj.Add(new tree());
          obj.Add(new tag(i == 0 && j == 0 ? 0 : rand.Next()));
          World.Objs.Add(obj);
        }
      }

      void placeBushes()
      {
        model3d[] models = new model3d[3];
        {
          int i = 0;
          foreach (string str in new[]
                     { "bush1", "bush2", "bush3" })
          {
            model3d model = model3d.Read(str, new Dictionary<string, uint>());
            model.Scale(16f);
            models[i] = model;
            i++;
          }
        }
        for (int i = -50; i <= 50; i++)
        for (int j = -50; j <= 50; j++)
        {
          if (rand.Next(0, 3) != 0) continue;
          float ipos = i * 100 + rand.NextFloat(-40, 40);
          float jpos = j * 100 + rand.NextFloat(-40, 40);

          for (int k = 0; k < 3; k++)
          for (int l = 0; l < 3; l++)
          {
            if (rand.Next(0, 3) != 0) continue;
            fall_obj obj = new();
            model3d.component comp = new(models[rand.Next(0, 3)], rand.NextFloat() * 180);
            obj.Add(comp);
            float_pos pos = new()
            {
              X = ipos + rand.NextFloat(-24, 24),
              Z = jpos + rand.NextFloat(-24, 24)
            };
            pos.Y = world.HeightAt((pos.X, pos.Z)) - 2f;
            pos.PrevX = pos.X;
            pos.PrevY = pos.Y;
            pos.PrevZ = pos.Z;
            obj.Add(pos);
            World.Objs.Add(obj);
          }
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
        float_pos pos = Player.Get<float_pos>(fall_obj.component.type.FLOAT_POS);
        pos.Yaw = pos.PrevYaw = 180;
        pos.X = pos.PrevX = pos.Z = pos.PrevZ = -1;
        pos.Y = pos.PrevY = 25;
        Player.Update();
      }

      makePlayer();
      World = new world();
      World.Objs.Add(Player);

      placeTrees();
      placeBushes();

      World.Update();
    }

    protected override void OnLoad()
    {
      base.OnLoad();

      GLFW.SwapInterval(0);
      GL.DepthFunc(DepthFunction.Lequal);
      gl_state_manager.EnableBlend();

      ticker.Init();

      CursorState = CursorState.Grabbed;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
      base.OnResize(e);

      if (e.Size == Vector2i.Zero)
        return;

      render_system.UpdateProjection();
      render_system.Resize();
      GL.Viewport(new Rectangle(0, 0, Size.X, Size.Y));
      fbo.Resize(Size.X, Size.Y);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
      base.OnRenderFrame(args);

      Player.Get<camera>(fall_obj.component.type.CAMERA).update_camera_vectors();

      fbo.Unbind();
      GL.ClearColor(_c);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
      render_system.FRAME.ClearColor();
      render_system.FRAME.ClearDepth();
      render_system.FRAME.Bind();
      GL.ClearColor(0f, 0f, 0f, 0f);

      render_system.UpdateLookAt(Player);
      render_system.UpdateProjection();

      World.Render();
      model3d.Draw();

      render_system.FRAME.Bind();

      render_system.FRAME.Blit(render_system.SWAP.Handle);
      if (_outline)
        render_system.RenderOutline();
      render_system.RenderFxaa(render_system.FRAME);

      fbo.Unbind();

      render_system.FRAME.Blit();

      render_system.UpdateLookAt(Player, false);
      render_system.UpdateProjection();
      font.Bind();
      render_system.RenderingRed = true;
      render_system.MESH.Begin();
      if (Frames % 2 == 0)
      {
        _mspf.Add(args.Time);
      }
      
      if (Environment.TickCount - _lastInquiry > 1000)
      {
        _lastInquiry = Environment.TickCount;
        _memUsage = (int)(GC.GetTotalMemory(false) / (1024 * 1024));
      }
      font.Draw(render_system.MESH, $"mspf: {_mspf.Average:N4} | fps: {1f / _mspf.Average:N2}"
        , 11, 38, PINK, false);
      font.Draw(render_system.MESH, $"_time: {Environment.TickCount / 1000f % (MathF.PI * 2f):N2}",
        11, 58, PINK, false);
      font.Draw(render_system.MESH, $"xyz: {Player.Pos.X:N2}; {Player.Pos.Y:N2}; {Player.Pos.Z:N2}",
        11, 78,
        PINK, false);
      font.Draw(render_system.MESH, $"heap: {_memUsage}M", 11, 98,
        PINK, false);
      render_system.MESH.Render();
      render_system.RenderingRed = false;

      render_system.LINE.Begin();
      {
        double p = 0;
        int x = 0;
        foreach (double f in _mspf.Values)
        {
          if (x != 0)
          {
            render_system.Line(11 + x * 2 - 2, Size.Y - 41 - 30 * ((float)p - 0.005f) * 100, 11 + x * 2,
              Size.Y - 41 - 30 * ((float)f - 0.005f) * 100 + 1, PINK);
          }
          p = f;
          x++;
        }
      }
      render_system.LINE.Render();
      font.Unbind();

      SwapBuffers();
      Frames++;
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
        Player.Get<camera>(fall_obj.component.type.CAMERA).FirstMouse = true;
        CursorState = CursorState.Normal;
      }

      if (MouseState.WasButtonDown(MouseButton.Left)) CursorState = CursorState.Grabbed;

      if (KeyboardState.IsKeyPressed(Keys.O)) _outline = !_outline;
      if (KeyboardState.IsKeyPressed(Keys.C)) FarCamera = !FarCamera;
      if (KeyboardState.IsKeyPressed(Keys.F11)) WindowState = WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;
      

      for (int j = 0; j < Math.Min(i, 10); j++)
      {
        Ticks++;

        World.Update();
        MouseDx = MouseDy = 0;
      }
    }
  }
}