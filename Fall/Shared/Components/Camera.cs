using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Fall.Shared.Components
{
    public class camera : fall_obj.component
    {
        private Vector3 _front;
        private Vector3 _right;
        private readonly Vector3 _up;
        private float_pos _pos;

        public camera()
        {
            _front = Vector3.Zero;
            _right = Vector3.Zero;
            _up = Vector3.UnitY;
            _lastX = 0;
        }

        public void update_camera_vectors()
        {
            _front = new Vector3(MathF.Cos(_pos.lerped_pitch.to_radians()) * MathF.Cos(_pos.lerped_yaw.to_radians()),
                MathF.Sin(_pos.lerped_pitch.to_radians()),
                MathF.Cos(_pos.lerped_pitch.to_radians()) * MathF.Sin(_pos.lerped_yaw.to_radians())).Normalized();
            _right = Vector3.Cross(_front, _up).Normalized();
        }

        private Vector3 _velocity;

        public override void update(fall_obj objIn)
        {
            base.update(objIn);

            _pos ??= objIn.get<float_pos>();

            _pos.set_prev();

            on_mouse_move();

            int forwards = 0;
            int rightwards = 0;
            KeyboardState kb = fall.instance.KeyboardState;
            if (kb.IsKeyDown(Keys.W)) forwards++;
            if (kb.IsKeyDown(Keys.S)) forwards--;
            if (kb.IsKeyDown(Keys.A)) rightwards--;
            if (kb.IsKeyDown(Keys.D)) rightwards++;
            Vector3 current = _pos.to_vector3();
            _velocity += _front * forwards * (1, 0, 1);
            _velocity += _right * rightwards;
            _velocity.Y -= 0.2f;
            current += _velocity;
            float height = world.height_at((_pos.x, _pos.z));
            if (current.Y < height)
            {
                current.Y = height;
                _velocity.Y = 0;
            }
            _velocity.Xz *= 0.5f;
            _pos.set_vector3(current);
        }

        public bool firstMouse = true;
        private float _lastX;
        private float _lastY;
        
        public void on_mouse_move()
        {
            if (fall.instance.CursorState != CursorState.Grabbed || !fall.instance.IsFocused) return;
            float xPos = fall.mouseX;
            float yPos = fall.mouseY;

            if (firstMouse)
            {
                _lastX = xPos;
                _lastY = yPos;
                firstMouse = false;
            }

            float xOffset = xPos - _lastX;
            float yOffset = _lastY - yPos;
            _lastX = xPos;
            _lastY = yPos;

            const float sensitivity = 0.1f;
            xOffset *= sensitivity;
            yOffset *= sensitivity;

            _pos.yaw += xOffset;
            _pos.pitch += yOffset;

            if (_pos.pitch > 89.0f)
                _pos.pitch = 89.0f;
            if (_pos.pitch < -89.0f)
                _pos.pitch = -89.0f;
        }

        public Matrix4 get_camera_matrix()
        {
            if (_pos == null)
            {
                return Matrix4.Identity;
            }

            Vector3 pos = new(_pos.lerped_x, _pos.lerped_y, _pos.lerped_z);
            Matrix4 lookAt = Matrix4.LookAt(pos - _front * 25, pos, _up);
            return lookAt;
        }
    }
}