using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Fall.Shared.Components
{
  public class camera : fall_obj.component
  {
    public const float FOV = 45 * MathF.PI / 180f;
    public const float NEAR = 0.1f;
    public static float Far => fall.FarCamera ? 1024f : 128f;
    private readonly Vector3 _up;
    private Vector3 _front;
    private float _lastX;
    private float _lastY;
    private float_pos _pos;
    private Vector3 _right;

    private Vector3 _velocity;

    public bool FirstMouse = true;

    public camera() : base(type.CAMERA)
    {
      _front = Vector3.Zero;
      _right = Vector3.Zero;
      _up = Vector3.UnitY;
      _lastX = 0;
    }

    public void update_camera_vectors()
    {
      _front = new Vector3(MathF.Cos(_pos.LerpedPitch.Rad()) * MathF.Cos(_pos.LerpedYaw.Rad()),
        MathF.Sin(_pos.LerpedPitch.Rad()),
        MathF.Cos(_pos.LerpedPitch.Rad()) * MathF.Sin(_pos.LerpedYaw.Rad())).Normalized();
      _right = Vector3.Cross(_front, _up).Normalized();
    }

    public override void Update(fall_obj objIn)
    {
      base.Update(objIn);

      _pos ??= objIn.Get<float_pos>(type.FLOAT_POS);

      _pos.set_prev();

      OnMouseMove();

      int forwards = 0;
      int rightwards = 0;
      KeyboardState kb = fall.Instance.KeyboardState;
      if (kb.IsKeyDown(Keys.W)) forwards++;
      if (kb.IsKeyDown(Keys.S)) forwards--;
      if (kb.IsKeyDown(Keys.A)) rightwards--;
      if (kb.IsKeyDown(Keys.D)) rightwards++;
      Vector3 current = _pos.to_vector3();
      _velocity += _front * forwards * (1, 0, 1);
      _velocity += _right * rightwards;
      _velocity.Y -= 0.2f;
      current += _velocity;
      float height = world.HeightAt((_pos.X, _pos.Z));
      if (current.Y < height)
      {
        current.Y = height;
        _velocity.Y = 0;
      }

      _velocity.Xz *= 0.5f;
      _pos.set_vector3(current);
    }

    private void OnMouseMove()
    {
      if (fall.Instance.CursorState != CursorState.Grabbed || !fall.Instance.IsFocused) return;
      float xPos = fall.MouseX;
      float yPos = fall.MouseY;

      if (FirstMouse)
      {
        _lastX = xPos;
        _lastY = yPos;
        FirstMouse = false;
      }

      float xOffset = xPos - _lastX;
      float yOffset = _lastY - yPos;
      _lastX = xPos;
      _lastY = yPos;

      const float SENSITIVITY = 0.1f;
      xOffset *= SENSITIVITY;
      yOffset *= SENSITIVITY;

      _pos.Yaw += xOffset;
      _pos.Pitch += yOffset;

      if (_pos.Pitch > 89.0f)
        _pos.Pitch = 89.0f;
      if (_pos.Pitch < -89.0f)
        _pos.Pitch = -89.0f;
    }

    public Matrix4 get_camera_matrix()
    {
      if (_pos == null) return Matrix4.Identity;

      Vector3 pos = new(_pos.LerpedX, _pos.LerpedY, _pos.LerpedZ);
      pos.Y += 4f;
      Vector3 eye = pos - _front * (fall.FarCamera ? 625f : 25f);
      eye.Y = Math.Max(eye.Y, world.HeightAt((eye.X, eye.Z)) + 0.33f);
      Matrix4 lookAt = Matrix4.LookAt(eye, pos, _up);
      return lookAt;
    }
  }
}