using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK;

namespace TriSpin
{
    class ObjVolume : Volume
    {
        Vector3[] vertices;
        Vector3[] colors;
        Vector2[] textureCoords;

        List<Tuple<int, int, int>> faces = new List<Tuple<int, int, int>>();

        public override int VertLength { get { return vertices.Length; } }
        public override int IndiceLength { get { return faces.Count * 3; } }
        public override int ColorDataLength { get { return colors.Length; } }

        public override Vector3[] Verts()
        {
            return vertices;
        }

        public override int[] Indices(int offset = 0)
        {
            return faces.SelectMany(t => new[] { t.Item1 + offset, t.Item2 + offset, t.Item3 + offset }).ToArray();
        }

        public override Vector3[] ColorData()
        {
            return colors;
        }

        public override Vector2[] GetTextureCoords()
        {
            return textureCoords;
        }

        public static ObjVolume LoadFromFile(string filename)
        {
            ObjVolume obj = new ObjVolume();
            try
            {
                using (StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
                {
                    obj = LoadFromString(reader.ReadToEnd());
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("File not found: {0}", filename);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading file: {0}", filename);
            }

            return obj;
        }

        public static ObjVolume LoadFromString(string obj)
        {
            // Seperate lines from the file
            List<String> lines = new List<string>(obj.Split('\n'));

            // Lists to hold model data
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texs = new List<Vector2>();
            List<Tuple<int, int, int>> faces = new List<Tuple<int, int, int>>();

            // Read file line by line
            foreach (String line in lines)
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Replace('.', ',')).ToArray();
                if (split.Length == 0) continue;
                if (split[0].Equals("v")) // Vertex definition
                {
                    if (split.Length != 4)
                    {
                        Console.WriteLine(line + " does not contain 4 elements!");
                        continue;
                    }

                    Vector3 vec = new Vector3();

                    // Attempt to parse each part of the vertice
                    bool success = float.TryParse(split[1], out vec.X);
                    success &= float.TryParse(split[2], out vec.Y);
                    success &= float.TryParse(split[3], out vec.Z);

                    // Dummy color/texture coordinates for now
                    colors.Add(new Vector3((float)Math.Sin(vec.Z), (float)Math.Sin(vec.Z), (float)Math.Sin(vec.Z)));
                    texs.Add(new Vector2((float)Math.Sin(vec.Z), (float)Math.Sin(vec.Z)));

                    // If any of the parses failed, report the error
                    if (!success)
                    {
                        Console.WriteLine("Error parsing vertex: {0}", line);
                    }

                    verts.Add(vec);
                }
                else if (split[0].Equals("f")) // Face definition
                {
                    if (split.Length != 4)
                    {
                        Console.WriteLine(line + " does not contain 4 elements!");
                        continue;
                    }

                    Tuple<int, int, int> face = new Tuple<int, int, int>(0, 0, 0);


                    int i1, i2, i3;

                    // Attempt to parse each part of the face
                    bool success = int.TryParse(split[1], out i1);
                    success &= int.TryParse(split[2], out i2);
                    success &= int.TryParse(split[3], out i3);

                    // If any of the parses failed, report the error
                    if (!success)
                    {
                        Console.WriteLine("Error parsing face: {0}", line);
                    }
                    else
                    {
                        // Decrement to get zero-based vertex numbers
                        face = new Tuple<int, int, int>(i1 - 1, i2 - 1, i3 - 1);
                        faces.Add(face);
                    }

                }
            }

            // Create the ObjVolume
            ObjVolume vol = new ObjVolume();
            vol.vertices = verts.ToArray();
            vol.faces = new List<Tuple<int, int, int>>(faces);
            vol.colors = colors.ToArray();
            vol.textureCoords = texs.ToArray();

            return vol;
        }
    }
}
