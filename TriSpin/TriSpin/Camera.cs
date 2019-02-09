using System;
using OpenTK;

namespace TriSpin
{
    class Camera
    {
        public Vector3 Position;
        public Vector3 Orientation;
        public float MoveSpeed = 0.2f;
        public float MouseSensitivity = 0.0025f;

        public Camera()
            : this(new Vector3(0, 0, 2))
        {}

        public Camera(Vector3 pos)
            : this(pos, new Vector3((float)Math.PI, 0, 0))
        {}

        public Camera(Vector3 pos, Vector3 orient)
        {
            Position = pos;
            Orientation = orient;
        }

        public Matrix4 GetViewMatrix()
        {
            var lookat = new Vector3
            {
                X = (float) (Math.Sin(Orientation.X)*Math.Cos(Orientation.Y)),
                Y = (float) Math.Sin(Orientation.Y),
                Z = (float) (Math.Cos(Orientation.X)*Math.Cos(Orientation.Y))
            };

            return Matrix4.LookAt(Position, Position + lookat, Vector3.UnitY);
        }

        public void Move(float x, float y, float z)
        {
            var offset = new Vector3();

            var forward = new Vector3((float)Math.Sin(Orientation.X), 0, (float)Math.Cos(Orientation.X));
            var right = new Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, MoveSpeed);

            Position += offset;
        }

        public void AddRotation(float x, float y)
        {
            x = x * MouseSensitivity;
            y = y * MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
        }
    }
}
