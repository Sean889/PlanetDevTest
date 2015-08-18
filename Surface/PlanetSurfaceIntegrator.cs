/*
    Copyright (c) 2015 Sean Lynch

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
 */

using OpenTK;
using OpenTK.Graphics.OpenGL;
using ShaderRuntime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
