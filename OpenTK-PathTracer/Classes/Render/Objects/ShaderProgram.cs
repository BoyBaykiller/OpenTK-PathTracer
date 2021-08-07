using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    class Shader : IDisposable
    {
        public readonly int ID;
        public readonly string Path;
        public readonly ShaderType ShaderType;

        public Shader(ShaderType shaderType, string path)
        {
            if (!System.IO.File.Exists(path))
                throw new Exception($"{path} does not exist");

            Path = path;
            ShaderType = shaderType;
            
            ID = GL.CreateShader(shaderType);
            GL.ShaderSource(ID, System.IO.File.ReadAllText(path));
            GL.CompileShader(ID);

            string compileInfo = GL.GetShaderInfoLog(ID);
            if (compileInfo != string.Empty)
            {
                Console.WriteLine($"Error under {path}. {compileInfo}");
            }
        }

        public void Dispose()
        {
            GL.DeleteShader(ID);
        }
    }

    class ShaderProgram : IDisposable
    {
        public readonly int ID;

        private static int lastBindedID = -1;
        public ShaderProgram(Shader[] shaders)
        {
            if (shaders == null || shaders.Length == 0)
                throw new Exception("ShaderProgram: Shader arrat is empty or null");

            if(!shaders.All(s => shaders.All(s1 => s.ID == s1.ID || s1.ShaderType != s.ShaderType)))
                throw new Exception("ShaderProgram: A ShaderProgram can only hold one instance of every ShaderType. Validate the shader array. ");

            ID = GL.CreateProgram();

            for (int i = 0; i < shaders.Length; i++)
                GL.AttachShader(ID, shaders[i].ID);

            GL.LinkProgram(ID);
            for (int i = 0; i < shaders.Length; i++)
            {
                GL.DetachShader(ID, shaders[i].ID);
                shaders[i].Dispose();
            }
        }

        public void Use()
        {
            if (lastBindedID != ID)
            {
                GL.UseProgram(ID);
                lastBindedID = ID;
            }
        }

        public static void Use(int ID)
        {
            if (lastBindedID != ID)
            {
                GL.UseProgram(ID);
                lastBindedID = ID;
            }
        }

        public static void UploadToBinded(int location, Matrix4 matrix4, bool transpose = false)
        {
            GL.UniformMatrix4(location, transpose, ref matrix4);
        }
        public void Upload(int location, Matrix4 matrix4, bool transpose = false)
        {
            Use();
            GL.UniformMatrix4(location, transpose, ref matrix4);
        }
        public void Upload(string name, Matrix4 matrix4, bool transpose = false)
        {
            GL.UniformMatrix4(GetUniformLocation(name), transpose, ref matrix4);
        }

        public static void UploadToBinded(int location, Vector4 vector4)
        {
            GL.Uniform4(location, vector4);
        }
        public void Upload(int location, Vector4 vector4)
        {
            Use();
            GL.Uniform4(location, vector4);
        }
        public void Upload(string name, Vector4 vector4)
        {
            GL.Uniform4(GetUniformLocation(name), vector4);
        }

        public static void UploadToBinded(int location, Vector3 vector3)
        {
            GL.Uniform3(location, vector3);
        }
        public void Upload(int location, Vector3 vector3)
        {
            Use();
            GL.Uniform3(location, vector3);
        }
        public void Upload(string name, Vector3 vector3)
        {
            GL.Uniform3(GetUniformLocation(name), vector3);
        }

        public static void UploadToBinded(int location, Vector2 vector2)
        {
            GL.Uniform2(location, vector2);
        }
        public void Upload(int location, Vector2 vector2)
        {
            Use();
            GL.Uniform2(location, vector2);
        }
        public void Upload(string name, Vector2 vector2)
        {
            GL.Uniform2(GetUniformLocation(name), vector2);
        }

        public static void UploadToBinded(int location, float x)
        {
            GL.Uniform1(location, x);
        }
        public void Upload(int location, float x)
        {
            Use();
            GL.Uniform1(location, x);
        }
        public void Upload(string name, float x)
        {
            GL.Uniform1(GetUniformLocation(name), x);
        }

        public static void UploadToBinded(int location, int x)
        {
            GL.Uniform1(location, x);
        }
        public void Upload(int location, int x)
        {
            Use();
            GL.Uniform1(location, x);
        }
        public void Upload(string name, int x)
        {
            GL.Uniform1(GetUniformLocation(name), x);
        }

        public static void UploadToBinded(int location, uint x)
        {
            GL.Uniform1(location, x);
        }
        public void Upload(int location, uint x)
        {
            Use();
            GL.Uniform1(location, x);
        }
        public void Upload(string name, uint x)
        {
            GL.Uniform1(GetUniformLocation(name), x);
        }

        public static void UploadToBinded(int location, bool x)
        {
            GL.Uniform1(location, x ? 1 : 0);
        }
        public void Upload(int location, bool x)
        {
            Use();
            GL.Uniform1(location, x ? 1 : 0);
        }
        public void Upload(string name, bool x)
        {
            GL.Uniform1(GetUniformLocation(name), x ? 1 : 0);
        }

        public int GetUniformLocation(string name)
        {
            Use();
            return GL.GetUniformLocation(ID, name);
        }


        public void Dispose()
        {
            GL.DeleteProgram(ID);
        }
    }
}