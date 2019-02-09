﻿using System.Linq;
using OpenTK;

namespace TriSpin
{
    class TexPyramid : Volume
    {
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

        private Vector2 v2(float x, float y)
        {
            return new Vector2(x, y);
        }
    }
}
