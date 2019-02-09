using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace TriSpin
{
    class Shader
    {
        public int ProgramID = -1;
        public int VShaderID = -1;
        public int FShaderID = -1;
        public int AttributeCount = 0;
        public int UniformCount = 0;

        public Dictionary<String, AttributeInfo> Attributes = new Dictionary<string, AttributeInfo>();
        public Dictionary<String, UniformInfo> Uniforms = new Dictionary<string, UniformInfo>();
        public Dictionary<String, uint> Buffers = new Dictionary<string, uint>();

        public Shader()
        {
            ProgramID = GL.CreateProgram();
        }

        public Shader(String vShader, String fShader) : this()
        {
            LoadShader(vShader, ShaderType.VertexShader);
            LoadShader(fShader, ShaderType.FragmentShader);

            Link();
            GenBuffers();
        }

        private void LoadShader(String shaderCode, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            GL.ShaderSource(address, shaderCode);
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        public void LoadShader(string shaderFile, ShaderType shaderType)
        {
            int id;
            using (StreamReader sr = new StreamReader(shaderFile))
            {
                LoadShader(sr.ReadToEnd(), shaderType, ProgramID, out id);
            }
            switch (shaderType)
            {
                case ShaderType.VertexShader:
                    VShaderID = id;
                    break;
                case ShaderType.FragmentShader:
                    FShaderID = id;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void Link()
        {
            GL.LinkProgram(ProgramID);
            Console.WriteLine(GL.GetProgramInfoLog(ProgramID));

            GL.GetProgram(ProgramID, GetProgramParameterName.ActiveAttributes, out AttributeCount);
            GL.GetProgram(ProgramID, GetProgramParameterName.ActiveUniforms, out UniformCount);

            for (int i = 0; i < AttributeCount; i++)
            {
                AttributeInfo info = new AttributeInfo();

                int length;
                GL.GetActiveAttrib(ProgramID, i, 256, out length, out info.size, out info.type, out info.name);

                info.address = GL.GetAttribLocation(ProgramID, info.name);
                Attributes.Add(info.name, info);
            }

            for (int i = 0; i < UniformCount; i++)
            {
                UniformInfo info = new UniformInfo();

                int length;
                GL.GetActiveUniform(ProgramID, i, 256, out length, out info.size, out info.type, out info.name);

                Uniforms.Add(info.name, info);
                info.address = GL.GetUniformLocation(ProgramID, info.name);
            }
        }

        public void GenBuffers()
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                uint buffer;
                GL.GenBuffers(1, out buffer);
                Buffers.Add(Attributes.Values.ElementAt(i).name, buffer);
            }

            for (int i = 0; i < Uniforms.Count; i++)
            {
                uint buffer;
                GL.GenBuffers(1, out buffer);
                Buffers.Add(Uniforms.Values.ElementAt(i).name, buffer);
            }
        }

        public void EnableVertexAttribArrays(bool enable)
        {
            foreach (var v in Attributes.Values)
            {
                if (enable)
                {
                    GL.EnableVertexAttribArray(v.address);
                }
                else
                {
                    GL.DisableVertexAttribArray(v.address);
                }
            }
        }

        public int GetAttribute(string name)
        {
            return Attributes.ContainsKey(name) ? Attributes[name].address : -1;
        }

        public int GetUniform(string name)
        {
            return Uniforms.ContainsKey(name) ? Uniforms[name].address : -1;
        }

        public uint GetBuffer(string name)
        {
            return Buffers.ContainsKey(name) ? Buffers[name] : 0;
        }
    }

    public class AttributeInfo
    {
        public String name = "";
        public int address = -1;
        public int size;
        public ActiveAttribType type;
    }

    public class UniformInfo
    {
        public String name = "";
        public int address = -1;
        public int size;
        public ActiveUniformType type;
    }
}
