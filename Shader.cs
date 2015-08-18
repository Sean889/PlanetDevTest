//Generated File

using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using ShaderRuntime;

#pragma warning disable 168

namespace Shaders
{
    class PlanetShader : GLShader
    {
        public bool TransposeMatrix = false;
        public static bool ImplementationSupportsShaders
        {
            get
            {
                return (new Version(GL.GetString(StringName.Version).Substring(0, 3)) >= new Version(2, 0) ? true : false);
            }
        }
        public static int __MVP;
        public static int __Vertex;
        public static int __Normal;
        public static int __Displacement;
        public OpenTK.Matrix4 uniform_MVP;
        public static readonly string VertexShaderSource = "#version 430\n\n#pragma name PlanetShader\n\nlayout(location = 0) uniform mat4 MVP;\n\nlayout(location = 0) in vec3 Vertex;\nlayout(location = 1) in vec3 Normal;\nlayout(location = 2) in float Displacement;\n\nsmooth out vec3 vs_Normal;\nsmooth out float vs_Displacement;\n\nvoid main()\n{\n    gl_Position = MVP * vec4(Vertex, 1.0);\n    \n    vs_Normal = Normal;\n    vs_Displacement = Displacement / 128.0;\n}";
        public static readonly string FragmentShaderSource = "#version 430\n\nin vec3 vs_Normal;\nin float vs_Displacement;\n\nout vec3 colour;\n\nvoid main()\n{\n    colour = vec3(vs_Displacement, 0.0, 0.0);\n}";
        public static int ProgramID = 0;
        private static ShaderRuntime.Utility.Counter Ctr = new ShaderRuntime.Utility.Counter(new Action(delegate{ GL.DeleteProgram(ProgramID); }));
        public static void CompileShader()
        {
            int prg = GL.CreateProgram();

            int Vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(Vertex, VertexShaderSource);
            GL.CompileShader(Vertex);
            Debug.WriteLine(GL.GetShaderInfoLog(Vertex));
            GL.AttachShader(prg, Vertex);

            int Fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(Fragment, FragmentShaderSource);
            GL.CompileShader(Fragment);
            Debug.WriteLine(GL.GetShaderInfoLog(Fragment));
            GL.AttachShader(prg, Fragment);

            GL.LinkProgram(prg);
            Debug.WriteLine(GL.GetProgramInfoLog(prg));

            GL.DetachShader(prg, Vertex);
            GL.DeleteShader(Vertex);

            GL.DetachShader(prg, Fragment);
            GL.DeleteShader(Fragment);
            __MVP = GL.GetUniformLocation(prg, "MVP");
            __Vertex = GL.GetAttribLocation(prg, "Vertex");
            __Normal = GL.GetAttribLocation(prg, "Normal");
            __Displacement = GL.GetAttribLocation(prg, "Displacement");
            ProgramID = prg;
        }
        public void Compile()
        {
            if(ProgramID == 0)
                CompileShader();
            Ctr++;
        }
        public void SetParameter<T>(string name, T value)
        {
            try
            {
                switch(name)
                {
                    case "MVP":
                        uniform_MVP = (OpenTK.Matrix4)(object)value;
                        break;
                    default:
                        throw new InvalidIdentifierException("There is no uniform variable named " + name + " in this shader.");
                }
            }
            catch(InvalidCastException e)
            {
                throw new InvalidParameterTypeException("Invalid parameter type: " + name + " is not convertible to the type \"" + typeof(T).FullName + "\".");
            }
        }
        public T GetParameter<T>(string name)
        {
            try
            {
                switch(name)
                {
                    case "MVP":
                        return (T)(object)uniform_MVP;
                    default:
                        throw new InvalidIdentifierException("There is no uniform variable named " + name + " in this shader.");
                }
            }
            catch(InvalidCastException e)
            {
                throw new InvalidParameterTypeException("Invalid paramater type: " + name + " is not convertible to the type \"" + typeof(T).FullName + "\".");
            }
        }
        public int GetParameterLocation(string name)
        {
            switch(name)
            {
                case "MVP":
                    return __MVP;
                case "Vertex":
                    return __Vertex;
                case "Normal":
                    return __Normal;
                case "Displacement":
                    return __Displacement;
                default:
                    throw new InvalidIdentifierException("There is no parameter named " + name + ".");
            }
        }
        public void PassUniforms()
        {
            GL.UniformMatrix4(__MVP, TransposeMatrix, ref uniform_MVP);
        }
        public void UseShader()
        {
            GL.UseProgram(ProgramID);
            GL.EnableVertexAttribArray(__Vertex);
            GL.EnableVertexAttribArray(__Normal);
            GL.EnableVertexAttribArray(__Displacement);
        }
        public int GetShaderID()
        {
            if(ProgramID != 0)
                return ProgramID;
            throw new ShaderNotInitializedException("The shader \"PlanetShader\" has not been initialized. Call Compile() on one of the instances or CompileShader() to compile the shader");
        }
        public void Dispose()
        {
            Ctr--;
        }
        public bool IsSupported
        {
            get
            {
                return ImplementationSupportsShaders;
            }
        }
    }
}
