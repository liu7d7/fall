using OpenTK.Graphics.OpenGL4;

namespace Fall.Engine
{
  public static class gl_state_manager
  {
    private static bool _depthEnabled;
    private static bool _blendEnabled;
    private static bool _cullEnabled;
    private static bool _depthMask;

    private static bool _depthSaved;
    private static bool _blendSaved;
    private static bool _cullSaved;

    public static void SaveState()
    {
      _depthSaved = _depthEnabled;
      _blendSaved = _blendEnabled;
      _cullSaved = _cullEnabled;
    }

    public static void RestoreState()
    {
      if (_depthSaved)
        EnableDepth();
      else
        DisableDepth();
      if (_blendSaved)
        EnableBlend();
      else
        DisableBlend();
      if (_cullSaved)
        EnableCull();
      else
        DisableCull();
    }

    public static void EnableDepth()
    {
      if (_depthEnabled) return;
      _depthEnabled = true;
      GL.Enable(EnableCap.DepthTest);
    }

    public static void DisableDepth()
    {
      if (!_depthEnabled) return;
      _depthEnabled = false;
      GL.Disable(EnableCap.DepthTest);
    }

    public static void DepthMask(bool depthMask)
    {
      if (_depthMask == depthMask) return;
      _depthMask = depthMask;
      GL.DepthMask(depthMask);
    }

    public static void EnableBlend()
    {
      if (_blendEnabled) return;
      _blendEnabled = true;
      GL.Enable(EnableCap.Blend);
      GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    public static void DisableBlend()
    {
      if (_blendEnabled)
      {
        _blendEnabled = false;
        GL.Disable(EnableCap.Blend);
      }
    }

    public static void EnableCull()
    {
      if (_cullEnabled) return;
      _cullEnabled = true;
      GL.Enable(EnableCap.CullFace);
    }

    public static void DisableCull()
    {
      if (!_cullEnabled) return;
      _cullEnabled = false;
      GL.Disable(EnableCap.CullFace);
    }
  }
}