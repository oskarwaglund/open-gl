using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using System.IO;
using OpenTK.Input;

namespace TriSpin
{
    class Win : GameWindow
    {
        private int pgmId;
        private int vsId;
        private int fsId;
        private int attrVcol;
        private int attrVpos;
        private int uniMview;
        private int vboPos;
        private int vboCol;
        private int vboMview;

        private double time = 0;

        private List<double> frameTimes = new List<double>(Enumerable.Range(0, 10).Select(s => 0.0));
        private int frameCount;

        Vector3[] vertData;
        Vector3[] colData;
        Matrix4[] mViewData;

        void initProgram()
        {
            pgmId = GL.CreateProgram();

            loadShader("vs.glsl", ShaderType.VertexShader, pgmId, out vsId);
            loadShader("fs.glsl", ShaderType.FragmentShader, pgmId, out fsId);

            GL.LinkProgram(pgmId);
            Console.WriteLine(GL.GetProgramInfoLog(pgmId));

            attrVpos = GL.GetAttribLocation(pgmId, "vPosition");
            attrVcol = GL.GetAttribLocation(pgmId, "vColor");
            uniMview = GL.GetUniformLocation(pgmId, "modelview");

            if (attrVpos == -1 || attrVcol == -1 || uniMview == -1)
            {
                Console.WriteLine("Error binding attributes");
            }

            GL.GenBuffers(1, out vboPos);
            GL.GenBuffers(1, out vboCol);
            GL.GenBuffers(1, out vboMview);
        }

        void loadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title = "Loading...";
            X = 1000;
            Y = 300;
            initProgram();

            colData = new []
            {
                new Vector3(1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f),
                new Vector3( 0f,  1f, 0f)
            };

            mViewData = new[]
            {
                Matrix4.Identity
            };
            GL.ClearColor(Color.LightBlue);
            GL.PointSize(3);

            GL.UseProgram(pgmId);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            time += e.Time;

            double a1 = time*2;
            double a2 = a1 + Math.PI*2/3;
            double a3 = a2 + Math.PI*2/3;

            vertData = new[]
            {
                new Vector3((float)Math.Cos(a1), (float)Math.Sin(a1), 0f),
                new Vector3((float)Math.Cos(a2), (float)Math.Sin(a2), 0f),
                new Vector3((float)Math.Cos(a3), (float)Math.Sin(a3), 0f)
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboPos);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertData.Length * Vector3.SizeInBytes), vertData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attrVpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboCol);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colData.Length * Vector3.SizeInBytes), colData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attrVcol, 3, VertexAttribPointerType.Float, true, 0, 0);

            GL.UniformMatrix4(uniMview, false, ref mViewData[0]);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                Exit();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            frameTimes[frameCount%frameTimes.Count] = e.Time;
            frameCount++;
            double fps = frameTimes.Count/frameTimes.Sum();

            Title = String.Format("Spinning dat triangle at {0} FPS", (int) fps);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            GL.EnableVertexAttribArray(attrVpos);
            GL.EnableVertexAttribArray(attrVcol);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            GL.DisableVertexAttribArray(attrVpos);
            GL.DisableVertexAttribArray(attrVcol);

            GL.Flush();

            SwapBuffers();
        }

        public Win()
            : base(400, 400,
                GraphicsMode.Default,
                "Busy Street",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                4, 5,
                GraphicsContextFlags.ForwardCompatible)
        {

        }
    }
}
