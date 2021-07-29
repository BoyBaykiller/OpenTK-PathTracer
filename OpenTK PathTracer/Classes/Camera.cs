using System;

using OpenTK;
using OpenTK.Input;

namespace OpenTK_PathTracer
{
    class Camera
    {
        public Vector3 Position;
        public Vector3 ViewDir;
        public Vector3 Up;
        public Vector3 Velocity;
        public float MovmentSpeed;
        public float MouseSensitivity;
        public Matrix4 View { get; private set; }
        public Camera(Vector3 position, Vector3 viewDir, Vector3 up, float mouseSensitivity = 0.1f, float speed = 10, float resistance = 0.911f)
        {
            View = Matrix4.LookAt(position, position + viewDir, up);
            Position = position;
            ViewDir = viewDir;
            Up = up;
            MovmentSpeed = speed;
            MouseSensitivity = mouseSensitivity;

            
            LookXY.Y = MathHelper.RadiansToDegrees(MathF.Asin(viewDir.Y));
            LookXY.X = 360 - MathHelper.RadiansToDegrees(MathF.Acos(viewDir.X / MathF.Cos(MathHelper.DegreesToRadians(LookXY.Y))));
        }

        private Vector2 LookXY = new Vector2();

        public MouseState lastMouseState = new MouseState();
        public void ProcessInputs(float dT, KeyboardState keyboardState, MouseState mouseState, out bool frameChanged)
        {
            frameChanged = false;

            Vector2 mouseDelta = new Vector2(mouseState.X - lastMouseState.X, mouseState.Y - lastMouseState.Y);
            if (mouseDelta.X != 0 || mouseDelta.Y != 0)
                frameChanged = true;

            LookXY.X += mouseDelta.X * MouseSensitivity;
            LookXY.Y -= mouseDelta.Y * MouseSensitivity;

            if (LookXY.Y >= 90)
                LookXY.Y = 89.999f;

            if (LookXY.Y <= -90)
                LookXY.Y = -89.999f;

            Vector3 viewDir;
            viewDir.X = MathF.Cos(MathHelper.DegreesToRadians(LookXY.X)) * MathF.Cos(MathHelper.DegreesToRadians(LookXY.Y));
            viewDir.Y = MathF.Sin(MathHelper.DegreesToRadians(LookXY.Y));
            viewDir.Z = MathF.Sin(MathHelper.DegreesToRadians(LookXY.X)) * MathF.Cos(MathHelper.DegreesToRadians(LookXY.Y));
            ViewDir = viewDir.Normalized();


            Vector3 acceleration = Vector3.Zero;
            if (keyboardState.IsKeyDown(Key.W))
                acceleration += ViewDir;
            
            if (keyboardState.IsKeyDown(Key.S))
                acceleration -= ViewDir;
            
            if (keyboardState.IsKeyDown(Key.D))
                acceleration += Vector3.Cross(ViewDir, Up).Normalized();

            if (keyboardState.IsKeyDown(Key.A))
                acceleration -= Vector3.Cross(ViewDir, Up).Normalized();

            Velocity += acceleration;
            Position += Velocity * dT;
            if (Vector3.Dot(Velocity, Velocity) < 0.1f)
                Velocity = Vector3.Zero;
            else
                frameChanged = true;

            Velocity *= 0.95f;
            View = Matrix4.LookAt(Position, Position + ViewDir, Up);
            
            lastMouseState = mouseState;
        }
    }
}
