using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Fall
{
  public static class program
  {
    // ReSharper disable once InconsistentNaming
    [STAThread]
    public static void Main(string[] args)
    {
      NativeWindowSettings nativeWindowSettings = new()
      {
        Size = new Vector2i(1152, 720),
        Title = "Fall",
        Flags = ContextFlags.ForwardCompatible,
        NumberOfSamples = 4
      };

      GameWindowSettings gameWindowSettings = new()
      {
        RenderFrequency = 0,
        UpdateFrequency = 144
      };

      GLFW.Init();
      GLFW.WindowHint(WindowHintInt.Samples, 4);
      using fall window = new(gameWindowSettings, nativeWindowSettings);
      window.Run();
    }
  }
}