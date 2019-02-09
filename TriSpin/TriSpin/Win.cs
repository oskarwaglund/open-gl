using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using OpenTK.Input;

namespace TriSpin
{
    class Win : GameWindow
    {
        private double time;

        Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
        Dictionary<string, int> textures = new Dictionary<string, int>();
        string activeShader;
        private int iboElements;

        private bool renderReady;

        private Camera cam = new Camera(new Vector3(0, 0, 4f));
        Vector2 lastMousePos;

        List<Volume> Volumes = new List<Volume>();
        private Vector3[] vertData;
        private int[] indiceData;
        private Vector3[] colorData;
        private Vector2[] texCoordData;

        private bool shaderKeyPressed;

        private void InitProgram()
        {
            GL.GenBuffers(1, out iboElements);

            shaders.Add("tex", new Shader("vs_tex.glsl", "fs_tex.glsl"));
            shaders.Add("def", new Shader("vs.glsl", "fs.glsl"));

            activeShader = shaders.Keys.First();

            textures.Add("bricks", LoadImage("bricks.jpg"));
            textures.Add("bricks2", LoadImage("bricks2.jpg"));

            TexPyramid tp1 = new TexPyramid();
            tp1.Position = new Vector3(-1, 0, 0);
            tp1.TextureID = textures["bricks2"];
            Volumes.Add(tp1);

            TexPyramid tp2 = new TexPyramid();
            tp2.TextureID = textures["bricks"];
            tp2.Position = new Vector3(1, 0, 0);
            Volumes.Add(tp2);

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
            List<Vector2> texCoords = new List<Vector2>();

            int vertCount = 0;

            foreach (var vol in Volumes)
            {
                verts.AddRange(vol.Verts);
                inds.AddRange(vol.GetIndices(vertCount));
                colors.AddRange(vol.ColorData);
                texCoords.AddRange(vol.GetTextureCoords());
                vertCount += vol.VertLength;
            }

            vertData = verts.ToArray();
            indiceData = inds.ToArray();
            colorData = colors.ToArray();
            texCoordData = texCoords.ToArray();

            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertData.Length * Vector3.SizeInBytes), vertData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colorData.Length * Vector3.SizeInBytes), colorData, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }
            else if (shaders[activeShader].GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("texcoord"));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texCoordData.Length * Vector2.SizeInBytes), texCoordData, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, iboElements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indiceData.Length * sizeof(int)), indiceData, BufferUsageHint.StaticDraw);

            Volumes[0].Rotation = new Vector3(0, 0.5f * (float)time, 0);
            Volumes[1].Rotation = new Vector3(0, 0.5f * (float)time, 0);

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

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            shaders[activeShader].EnableVertexAttribArrays(true);

            int currIndice = 0;
            foreach (var vol in Volumes)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, vol.TextureID);
                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref vol.ModelViewProjectionMatrix);

                if (shaders[activeShader].GetUniform("maintexture") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetUniform("maintexture"), 0);
                }

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

            if (Keyboard.GetState().IsKeyDown(Key.X))
            {
                shaderKeyPressed = true;
            }
            else if (shaderKeyPressed)
            {
                shaderKeyPressed = false;
                activeShader = shaders.Keys.First(s => s != activeShader);
            }

            //Mouse
            if (Focused)
            {
                var delta = lastMousePos - new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                cam.AddRotation(delta.X, delta.Y);
                lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }
        }

        int LoadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }

        int LoadImage(string fileName)
        {
            Bitmap file = new Bitmap(fileName);
            return LoadImage(file);
        }

        public Win()
            : base(1024, 1024, new GraphicsMode(32, 24, 0, 4))
        {

        }
    }
}
