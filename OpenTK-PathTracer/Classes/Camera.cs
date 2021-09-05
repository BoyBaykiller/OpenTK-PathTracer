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
        public Camera(Vector3 position, Vector3 viewDir, Vector3 up, float mouseSensitivity = 0.1f, float speed = 10)
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
        public void ProcessInputs(float dT, out bool frameChanged)
        {
            frameChanged = false;

            Vector2 mouseDelta = MouseManager.DeltaPosition;
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
            ViewDir = viewDir;


            Vector3 acceleration = Vector3.Zero;
            if (KeyboardManager.IsKeyDown(Key.W))
                acceleration += ViewDir;
            
            if (KeyboardManager.IsKeyDown(Key.S))
                acceleration -= ViewDir;
            
            if (KeyboardManager.IsKeyDown(Key.D))
                acceleration += Vector3.Cross(ViewDir, Up).Normalized();

            if (KeyboardManager.IsKeyDown(Key.A))
                acceleration -= Vector3.Cross(ViewDir, Up).Normalized();

            
            Velocity += KeyboardManager.IsKeyDown(Key.LShift) ? acceleration * 5 : (KeyboardManager.IsKeyDown(Key.LControl) ? acceleration * 0.35f : acceleration);
            if (Vector3.Dot(Velocity, Velocity) < 0.01f)
                Velocity = Vector3.Zero;
            else
                frameChanged = true;

            Position += Velocity * dT;
            Velocity *= 0.95f;
            View = GenerateMatrix(Position, ViewDir, Up);
        }

        public static Matrix4 GenerateMatrix(Vector3 position, Vector3 viewDir, Vector3 up) => Matrix4.LookAt(position, position + viewDir, up);
    }
}
