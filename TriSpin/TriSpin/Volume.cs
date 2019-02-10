using System;
using System.Linq;
using OpenTK;

namespace TriSpin
{
    public abstract class Volume
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public Matrix4 ModelMatrix = Matrix4.Identity;
        public Matrix4 ViewProjectionMatrix = Matrix4.Identity;
        public Matrix4 ModelViewProjectionMatrix = Matrix4.Identity;

        public bool IsTextured = false;
        public int TextureID;
        public int TextureCoordsCount;
        public abstract Vector2[] GetTextureCoords();

        public abstract int VertLength { get; }
        public abstract Vector3[] Verts();

        public abstract int IndiceLength { get; }
        public abstract int[] Indices(int offset = 0);

        public abstract int ColorDataLength { get; }
        public abstract Vector3[] ColorData();

        public void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateRotationX(Rotation.X) *
                Matrix4.CreateRotationY(Rotation.Y) *
                Matrix4.CreateRotationZ(Rotation.Z) *
                Matrix4.CreateScale(Scale) *
                Matrix4.CreateTranslation(Position);
        }
    }
}
