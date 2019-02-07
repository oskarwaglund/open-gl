using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        private int iboElements;

        private double time;

        private List<double> frameTimes = new List<double>(Enumerable.Range(0, 10).Select(s => 0.0));
        private int frameCount;

        Vector3[] vertData;
        Vector3[] colData;
        Matrix4[] mViewData;
        private int[] indiceData;

        private Camera cam = new Camera();
        Vector2 lastMousePos;

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
            GL.GenBuffers(1, out iboElements);

            CursorVisible = false;
            lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
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

            GL.ClearColor(Color.LightBlue);
            GL.PointSize(10);

            float V = 0.5f;
            float H = V/2;
            vertData = new[]
            {
                new Vector3(0, V-H, 0),
                new Vector3(V, -H, V),
                new Vector3(-V, -H, V),
                new Vector3(-V, -H, -V),
                new Vector3(V, -H, -V)
            };

            indiceData = new[]
            {
                0, 1, 2,
                0, 2, 3,
                0, 3, 4,
                0, 4, 1,
                1, 2, 3,
                1, 4, 3
            };

            GL.UseProgram(pgmId);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            time += e.Time;

            ProcessInput();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboPos);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertData.Length * Vector3.SizeInBytes), vertData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attrVpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboCol);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colData.Length * Vector3.SizeInBytes), colData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attrVcol, 3, VertexAttribPointerType.Float, true, 0, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, iboElements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indiceData.Length * sizeof(int)), indiceData, BufferUsageHint.StaticDraw);


            var projMatrix = cam.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f);
            mViewData = new[]
            {
                projMatrix
            };
            GL.UniformMatrix4(uniMview, false, ref mViewData[0]);
        }

        private void ProcessInput()
        {

            if (Keyboard.GetState().IsKeyDown(Key.W))
            {
                cam.Move(0f, 0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.S))
            {
                cam.Move(0f, -0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.A))
            {
                cam.Move(-0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.D))
            {
                cam.Move(0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.Q))
            {
                cam.Move(0f, 0f, -0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.E))
            {
                cam.Move(0f, 0f, 0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.Escape))
            {
                Exit();
            }

            //Mouse
            if (Focused)
            {
                var delta = lastMousePos - new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                cam.AddRotation(delta.X, delta.Y);
                lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);
            lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
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
            GL.DrawElements(BeginMode.Triangles, indiceData.Length, DrawElementsType.UnsignedInt, 0);

            GL.DisableVertexAttribArray(attrVpos);
            GL.DisableVertexAttribArray(attrVcol);

            GL.Flush();

            SwapBuffers();
        }

        public Win()
            : base(512, 512, new GraphicsMode(32, 24, 0, 4))
        {

        }
    }
}
