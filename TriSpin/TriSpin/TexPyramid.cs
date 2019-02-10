using System.Linq;
using OpenTK;

namespace TriSpin
{
    class TexPyramid : Volume
    {
        private Vector3[] _Verts;
        private int[] _Indices;
        private Vector3[] _ColorData;

        public TexPyramid()
        {
            float V = 0.5f;
            float H = V / 2;
            var top = new Vector3(0, V - H, 0);
            var pp = new Vector3(V, -H, V);
            var np = new Vector3(-V, -H, V);
            var nn = new Vector3(-V, -H, -V);
            var pn = new Vector3(V, -H, -V);

            _Verts = new []{
                //Sides
                top, np, pp,
                top, pn, pp,
                top, nn, pn,
                top, nn, np,
                //Bottom
                nn, pn, pp,
                nn, np, pp
            };

            _Indices = Enumerable.Range(0, _Verts.Length).ToArray();
            _ColorData = _Verts.Select(v => new Vector3(0.761f, 0.698f, 0.502f)).ToArray();
        }

        public override Vector2[] GetTextureCoords()
        {
            return new[]
            {
                //Sides
                v2(0.5f, 1), v2(0, 0), v2(1, 0),
                v2(0.5f, 1), v2(0, 0), v2(1, 0),
                v2(0.5f, 1), v2(0, 0), v2(1, 0),
                v2(0.5f, 1), v2(0, 0), v2(1, 0),
                //Bottom
                v2(0, 0), v2(1, 0), v2(1, 1),
                v2(0, 0), v2(0, 1), v2(1, 1)
            };
        }

        public override int VertLength
        {
            get { return _Verts.Length; }
        }
        public override Vector3[] Verts()
        {
            return _Verts;
        }

        public override int IndiceLength
        {
            get { return _Indices.Length; }
        }
        public override int[] Indices(int offset = 0)
        {
            return _Indices.Select(i => i + offset).ToArray();
        }

        public override int ColorDataLength
        {
            get { return _ColorData.Length; }
        }
        public override Vector3[] ColorData()
        {
            return _ColorData;
        }

        private Vector2 v2(float x, float y)
        {
            return new Vector2(x, y);
        }
    }
}
