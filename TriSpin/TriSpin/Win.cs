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
        private double time;

        Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
        string activeShader = "def";
        private int iboElements;

        private bool renderReady = false;

        private List<double> frameTimes = new List<double>(Enumerable.Range(0, 10).Select(s => 0.0));
        private int frameCount;

        private Camera cam = new Camera();
        Vector2 lastMousePos;

        List<Volume> Volumes = new List<Volume>();
        private Vector3[] vertData = {};
        private int[] indiceData = {};
        private Vector3[] colorData = {};

        private void InitProgram()
        {
            GL.GenBuffers(1, out iboElements);
            shaders.Add("def", new Shader("vs.glsl", "fs.glsl"));
            Volumes.Add(new Pyramid());
            Volumes.Add(new Pyramid());

            CursorVisible = false;
            lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title = "Loading...";

            InitProgram();
            GL.ClearColor(Color.LightBlue);
            GL.UseProgram(shaders[activeShader].ProgramID);
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);
            lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            time += e.Time;

            ProcessInput();

            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();

            int vertCount = 0;

            foreach (var vol in Volumes)
            {
                verts.AddRange(vol.Verts);
                inds.AddRange(vol.GetIndices(vertCount));
                colors.AddRange(vol.ColorData);
                vertCount += vol.VertLength;
            }

            vertData = verts.ToArray();
            indiceData = inds.ToArray();
            colorData = colors.ToArray();

            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertData.Length * Vector3.SizeInBytes), vertData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colorData.Length * Vector3.SizeInBytes), colorData, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, iboElements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indiceData.Length * sizeof(int)), indiceData, BufferUsageHint.StaticDraw);

            Volumes[0].Rotation = new Vector3(0, 0.5f * (float)time, 0);
            Volumes[0].Position = new Vector3(0, 0, -2f);

            foreach (Volume vol in Volumes)
            {
                vol.CalculateModelMatrix();
                vol.ViewProjectionMatrix = cam.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f);
                vol.ModelViewProjectionMatrix = vol.ModelMatrix * vol.ViewProjectionMatrix;
            }

            renderReady = true;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (!renderReady) return;

            frameTimes[frameCount%frameTimes.Count] = e.Time;
            frameCount++;
            double fps = frameTimes.Count/frameTimes.Sum();

            Title = String.Format("Spinning dat triangle at {0} FPS", (int) fps);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            shaders[activeShader].EnableVertexAttribArrays(true);

            int currIndice = 0;
            foreach (var vol in Volumes)
            {
                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref vol.ModelViewProjectionMatrix);
                GL.DrawElements(BeginMode.Triangles, vol.IndiceLength, DrawElementsType.UnsignedInt, currIndice * sizeof(uint));
                currIndice += vol.IndiceLength;
            }

            shaders[activeShader].EnableVertexAttribArrays(false);

            GL.Flush();
            SwapBuffers();
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

        public Win()
            : base(512, 512, new GraphicsMode(32, 24, 0, 4))
        {

        }
    }
}
