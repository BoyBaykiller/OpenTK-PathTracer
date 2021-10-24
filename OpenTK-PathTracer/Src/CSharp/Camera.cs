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
        public Camera(Vector3 position, Vector3 up, float lookX = 0.0f, float lookY = -90.0f, float mouseSensitivity = 0.1f, float speed = 10)
        {
            LookX = lookX;
            LookY = lookY;

            ViewDir.X = MathF.Cos(MathHelper.DegreesToRadians(LookX)) * MathF.Cos(MathHelper.DegreesToRadians(LookY));
            ViewDir.Y = MathF.Sin(MathHelper.DegreesToRadians(LookY));
            ViewDir.Z = MathF.Sin(MathHelper.DegreesToRadians(LookX)) * MathF.Cos(MathHelper.DegreesToRadians(LookY));

            View = GenerateMatrix(position, ViewDir, up);
            Position = position;
            Up = up;
            MovmentSpeed = speed;
            MouseSensitivity = mouseSensitivity;
        }


        public float LookX { get; private set; }
        public float LookY { get; private set; }
        public void ProcessInputs(float dT, out bool frameChanged)
        {
            frameChanged = false;

            Vector2 mouseDelta = MouseManager.DeltaPosition;
            if (mouseDelta.X != 0 || mouseDelta.Y != 0)
                frameChanged = true;

            LookX += mouseDelta.X * MouseSensitivity;
            LookY -= mouseDelta.Y * MouseSensitivity;

            if (LookY >= 90) LookY = 89.999f;
            if (LookY <= -90) LookY = -89.999f;

            ViewDir.X = MathF.Cos(MathHelper.DegreesToRadians(LookX)) * MathF.Cos(MathHelper.DegreesToRadians(LookY));
            ViewDir.Y = MathF.Sin(MathHelper.DegreesToRadians(LookY));
            ViewDir.Z = MathF.Sin(MathHelper.DegreesToRadians(LookX)) * MathF.Cos(MathHelper.DegreesToRadians(LookY));

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

        public static Matrix4 GenerateMatrix(Vector3 position, Vector3 viewDir, Vector3 up)
        {
            return Matrix4.LookAt(position, position + viewDir, up);
        }
    }
}
