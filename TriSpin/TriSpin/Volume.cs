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

        //Define in child class
        protected Vector3[] _Verts;
        protected int[] _Indices;
        protected Vector3[] _ColorData;

        public Matrix4 ModelMatrix = Matrix4.Identity;
        public Matrix4 ViewProjectionMatrix = Matrix4.Identity;
        public Matrix4 ModelViewProjectionMatrix = Matrix4.Identity;

        public int VertLength
        {
            get { return _Verts.Length; }
        }

        public Vector3[] Verts
        {
            get
            {
                if (_Verts == null) throw new NotImplementedException();
                return _Verts;
            }
        }

        public int IndiceLength
        {
            get { return _Indices.Length; }
        }

        public int[] GetIndices(int offset = 0)
        {
            if (_Indices == null) throw new NotImplementedException();
            return _Indices.Select(i => i+offset).ToArray();
        }

        public int ColorDataLength
        {
            get { return _ColorData.Length; }
        }

        public Vector3[] ColorData
        {
            get
            {
                if (_ColorData == null) throw new NotImplementedException();
                return _ColorData;
            }
        }

        public void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) *
                Matrix4.CreateRotationX(Rotation.X) *
                Matrix4.CreateRotationY(Rotation.Y) *
                Matrix4.CreateRotationZ(Rotation.Z) *
                Matrix4.CreateTranslation(Position);
        }
    }
}
