using OpenTK;

namespace TriSpin
{
    public class Pyramid : Volume
    {
        public Pyramid()
        {
            float V = 0.5f;
            float H = V / 2;

            _Verts = new[]
            {
                new Vector3(0, V-H, 0),
                new Vector3(V, -H, V),
                new Vector3(-V, -H, V),
                new Vector3(-V, -H, -V),
                new Vector3(V, -H, -V)
            };

            _Indices = new[]
            {
                0, 1, 2,
                0, 2, 3,
                0, 3, 4,
                0, 4, 1,
                1, 2, 3,
                1, 4, 3
            };

            _ColorData = new[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f),
                new Vector3( 0f,  1f, 0f)
            };
        }
    }
}
