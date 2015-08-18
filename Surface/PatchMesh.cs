using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace PlanetDevTest.Surface
{
    public sealed class PatchMesh : IDisposable
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct DataItem
        {
            [FieldOffset(0)]
            public Vector3 Vertex;
            [FieldOffset(sizeof(float) * 3)]
            public Vector3 Texcoord;
            [FieldOffset(sizeof(float) * 6)]
            public float Displacement;
            [FieldOffset(sizeof(float) * 7)]
            public float Padding;
        }

        public Patch Parent;

        public int Vao;
        public Vector3d Offset;

        private int Verts = 0;
        private int Normals = 0;
        private int Displacements = 0;

        public PatchMesh(Patch Parent, int Ibo)
        {
            this.Parent = Parent;
            this.Vao = 0;
            Offset = Parent.Position;
            CreateVao(Ibo);
        }

        private unsafe void CreateVao(int Ibo)
        {
            //DataItem[] Data = new DataItem[Patch.NumVertices];

            //for(int i = 0; i < Patch.NumVertices; i++)
            //{
            //    Data[i].Vertex = Parent.MeshData.Vertices[i];
            //    Data[i].Texcoord = Parent.MeshData.Texcoords[i];
            //    Data[i].Displacement = Parent.MeshData.Displacements[i];
            //    //Data[i].Padding = Parent.MeshData.Displacements[i];
            //}

            Verts = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Verts);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * 3 * Patch.NumVertices), Parent.MeshData.Vertices, BufferUsageHint.StaticDraw);

            Normals = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Normals);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * 3 * Patch.NumVertices), Parent.MeshData.Texcoords, BufferUsageHint.StaticDraw);

            Displacements = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Displacements);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * Patch.NumVertices), Parent.MeshData.Displacements, BufferUsageHint.StaticDraw);

            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ibo);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Verts);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Normals);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Displacements);
            GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindVertexArray(0);

            //GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(Verts);
            GL.DeleteBuffer(Normals);
            GL.DeleteBuffer(Displacements);
            GL.DeleteVertexArray(Vao);
        }
    }
}
