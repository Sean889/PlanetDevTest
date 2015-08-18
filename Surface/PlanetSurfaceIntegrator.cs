using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ShaderRuntime;

namespace PlanetDevTest.Surface
{
    public class PlanetSurfaceIntegrator : LayerIntegrator
    {
        private ConcurrentQueue<Patch> Create = new ConcurrentQueue<Patch>();
        private ConcurrentQueue<Patch> Add = new ConcurrentQueue<Patch>();
        private ConcurrentQueue<Patch> Remove = new ConcurrentQueue<Patch>();
        private ConcurrentQueue<Patch> Delete = new ConcurrentQueue<Patch>();

        private List<Patch> Global = new List<Patch>();
        private List<PatchMesh> Active = new List<PatchMesh>();
        private int Ibo;

        public GLShader PlanetShader;
        public Planet Planet;

        public void CreatePatch(Patch p)
        {
            Create.Enqueue(p);
        }

        public void AddPatch(Patch p)
        {
            Add.Enqueue(p);
        }

        public void RemovePatch(Patch p)
        {
            Remove.Enqueue(p);
        }

        public void DeletePatch(Patch p)
        {
            Delete.Enqueue(p);
        }

        public void Update()
        {
            Patch p;
            while(Create.TryDequeue(out p))
            {
                PatchMesh Mesh = new PatchMesh(p, Ibo);
                p.IntegratorObject = Mesh;

                Global.Add(p);
                Active.Add(Mesh);
            }

            while(Add.TryDequeue(out p))
            {
                Active.Add((PatchMesh)p.IntegratorObject);
            }

            while(Remove.TryDequeue(out p))
            {
                Active.Remove((PatchMesh)p.IntegratorObject);
            }

            while(Delete.TryDequeue(out p))
            {
                PatchMesh Mesh = (PatchMesh)p.IntegratorObject;

                Global.Remove(p);
                Active.Remove(Mesh);

                Mesh.Dispose();
            }
        }

        private static Matrix4 ToMatrix4(Matrix4d m)
        {
            return new Matrix4(
                (float)m[0, 0], (float)m[0, 1], (float)m[0, 2], (float)m[0, 3],
                (float)m[1, 0], (float)m[1, 1], (float)m[1, 2], (float)m[1, 3],
                (float)m[2, 0], (float)m[2, 1], (float)m[2, 2], (float)m[2, 3],
                (float)m[3, 0], (float)m[3, 1], (float)m[3, 2], (float)m[3, 3]);
        }

        public void Draw(Matrix4d CamMat)
        {
            PlanetShader.UseShader();

            Matrix4d PlanetMat = Matrix4d.CreateTranslation(Planet.Position) * Matrix4d.CreateFromQuaternion(Planet.Rotation);

            foreach(PatchMesh Mesh in Active)
            {
                PlanetShader.SetParameter("MVP", ToMatrix4(Matrix4d.CreateTranslation(Mesh.Offset) * PlanetMat * CamMat));

                GL.BindVertexArray(Mesh.Vao);

                PlanetShader.PassUniforms();

                GL.DrawElements(PrimitiveType.Triangles, Patch.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
        }

        public PlanetSurfaceIntegrator(GLShader Shader, Planet Planet)
        {
            this.Planet = Planet;
            PlanetShader = Shader;

            Ibo = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Patch.NumIndices * sizeof(int)), Patch.Indices, BufferUsageHint.StaticDraw);
        }
    }
}
