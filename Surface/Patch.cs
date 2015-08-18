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
using System;

namespace PlanetDevTest.Surface
{
    using ity = Int32;

    /// <summary>
    /// Per-planet data.
    /// </summary>
    public class PatchData
    {
        public delegate double DisplacementDelegate(Vector3d Coord);

        public DisplacementDelegate Del;
    }

    public class Patch : IDisposable
    {
        #region types
        public class Mesh
        {
            public Vector3[] Vertices = new Vector3[NUM_VERTICES];
            public Vector3[] Texcoords = new Vector3[NUM_VERTICES];
            public float[] Displacements = new float[NUM_VERTICES];

            public Vector3[] vertex
            {
                get
                {
                    return Vertices;
                }
            }
            public Vector3[] texcoord
            {
                get
                {
                    return Texcoords;
                }
            }

        }
        #endregion

        #region const
        private const ity SIDE_LEN = 33;
        private const ity NUM_VERTICES = (SIDE_LEN * SIDE_LEN + SIDE_LEN * 4);
        private const ity NUM_INDICES = (SIDE_LEN - 1) * (SIDE_LEN - 1) * 6 + 24 * (SIDE_LEN - 1);

        private const double PATCH_MULT = 1;
        private const double DIS_MULT = 2;
        private const double INTERP = 1.0 / (SIDE_LEN - 1);
        #endregion

        #region static
        public static readonly ity VerticesPerSide = SIDE_LEN;
        public static readonly ity NumVertices = NUM_VERTICES;
        public static readonly ity NumIndices = NUM_INDICES;

        public static readonly ity[] Indices = GetIndices();
        #endregion

        #region nonstatic
        public volatile object IntegratorObject;

        /// <summary>
        /// Depth of the skirt around the planet
        /// </summary>
        public readonly double SkirtDepth;
        /// <summary>
        /// The radius of the attached planet.
        /// </summary>
        public readonly double PlanetRadius;
        /// <summary>
        /// The side length of the patch on the base cube.
        /// </summary>
        public readonly double SideLength;
        /// <summary>
        /// The level of this patch within the quadtree
        /// </summary>
        public readonly uint Level;

        //Mesh corners
        public Vector3d Nwc;
        public Vector3d Nec;
        public Vector3d Swc;
        public Vector3d Sec;

        /// <summary>
        /// The position of the patch on the planet's surface
        /// </summary>
        public Vector3d Position;

        //Patch children
        public Patch Nw;
        public Patch Ne;
        public Patch Sw;
        public Patch Se;

        /// <summary>
        /// Mesh data
        /// </summary>
        public volatile Mesh MeshData;

        /// <summary>
        /// Layer manager
        /// </summary>
        public LayerIntegrator Integrator;

        /// <summary>
        /// Per-planet data.
        /// </summary>
        public PatchData Data;
        #endregion

        #region properties
        public bool IsSubdivided
        {
            get
            {
                return Nw != null;
            }
        }
        public bool HasMeshData
        {
            get
            {
                return MeshData != null;
            }
        }
        #endregion

        #region functions

        #region static
        private static ity[] GetIndices()
        {
            ity[] indices = new ity[NUM_INDICES];

            ity idx = 0;

            for (ity y = 0; y < SIDE_LEN - 1; y++)
            {
                for (ity x = 0; x < SIDE_LEN - 1; x++)
                {
                    //First triangle
                    indices[idx++] = (y + 1) * SIDE_LEN + x;
                    indices[idx++] = y * SIDE_LEN + x + 1;
                    indices[idx++] = (y * SIDE_LEN + x);

                    //Second triangle
                    indices[idx++] = (y + 1) * SIDE_LEN + x;
                    indices[idx++] = (y + 1) * SIDE_LEN + x + 1;
                    indices[idx++] = y * SIDE_LEN + x + 1;
                }
            }

            //Generate indices for skirt

            for (ity i = 0; i < SIDE_LEN - 1; i++)
            {
                //Top side
                indices[idx++] = i;
                indices[idx++] = SIDE_LEN * SIDE_LEN + i + 1;
                indices[idx++] = SIDE_LEN * SIDE_LEN + i;

                indices[idx++] = i;
                indices[idx++] = i + 1;
                indices[idx++] = SIDE_LEN * SIDE_LEN + i + 1;

                //Right side
                indices[idx++] = SIDE_LEN * (i + 2) - 1;
                indices[idx++] = SIDE_LEN * SIDE_LEN + SIDE_LEN + i;
                indices[idx++] = SIDE_LEN * (i + 1) - 1;

                indices[idx++] = SIDE_LEN * (i + 2) - 1;
                indices[idx++] = SIDE_LEN * SIDE_LEN + SIDE_LEN + i + 1;
                indices[idx++] = SIDE_LEN * SIDE_LEN + SIDE_LEN + i;

                //Bottom side
                indices[idx++] = SIDE_LEN * (SIDE_LEN + 2) + i;
                indices[idx++] = (SIDE_LEN - 1) * SIDE_LEN + i + 1;
                indices[idx++] = (SIDE_LEN - 1) * SIDE_LEN + i;

                indices[idx++] = SIDE_LEN * (SIDE_LEN + 2) + i;
                indices[idx++] = SIDE_LEN * (SIDE_LEN + 2) + i + 1;
                indices[idx++] = (SIDE_LEN - 1) * SIDE_LEN + i + 1;

                //Left side
                indices[idx++] = SIDE_LEN * (SIDE_LEN + 3) + i + 1;
                indices[idx++] = SIDE_LEN * i;
                indices[idx++] = SIDE_LEN * (SIDE_LEN + 3) + i;

                indices[idx++] = SIDE_LEN * (SIDE_LEN + 3) + i + 1;
                indices[idx++] = SIDE_LEN * (i + 1);
                indices[idx++] = SIDE_LEN * i;
            }
            return indices;

        }

        #endregion

        #region nonstatic
        public Patch(Vector3d Nwc, Vector3d Nec, Vector3d Swc, Vector3d Sec, double PlanetRadius, double SideLen, double SkirtDepth, LayerIntegrator Controller, uint Level, PatchData Data)
        {
            this.Nwc = Nwc;
            this.Nec = Nec;
            this.Swc = Swc;
            this.Sec = Sec;
            this.Nw = null;
            this.Ne = null;
            this.Sw = null;
            this.Se = null;
            this.MeshData = null;
            this.Position = ((Nwc + Nec + Swc + Sec) * 0.25).Normalized() * PlanetRadius;
            this.Level = Level;
            this.PlanetRadius = PlanetRadius;
            this.Integrator = Controller;
            this.SideLength = SideLen;
            this.SkirtDepth = SkirtDepth;
            this.Data = Data;
        }

        public void GenMeshData()
        {
            Mesh mesh_data_ptr = new Mesh();
            for (int x = 0; x < SIDE_LEN; x++)
            {
                //Calcualte horizontal position
                double interp = INTERP * (double)x;
                Vector3d v1 = Vector3d.Lerp(Nwc, Nec, interp);
                Vector3d v2 = Vector3d.Lerp(Swc, Sec, interp);
                for (int y = 0; y < SIDE_LEN; y++)
                {
                    //Calculate vertical position
                    Vector3d vtx = Vector3d.Lerp(v1, v2, INTERP * (double)y);
                    Vector3d nvtx = vtx.Normalized();
                    //Map to sphere then add displacement
                    vtx = nvtx * PlanetRadius;
                    double disp = Data.Del(vtx);
                    vtx += nvtx * disp;
                    //Assign vertex position
                    mesh_data_ptr.vertex[x * SIDE_LEN + y] = (Vector3)(vtx - Position);
                    //Texcoord is normal as well, data compactness
                    mesh_data_ptr.texcoord[x * SIDE_LEN + y] = (Vector3)nvtx;
                    //displacement
                    mesh_data_ptr.Displacements[x * SIDE_LEN + y] = (float)disp;
                }
            }

            //Skirt generation code

            /*
                Skirt is the position of the surface, but SkirtDepth units lower

                Calculate position on the sphere, then subtract SkirtDepth units
                Texture coordinate is still just normalized position
            */

            //Vertex normal releative to planet centre
            Vector3d vnrm;
            //Sizeof base surface data
            uint data_size = SIDE_LEN * SIDE_LEN;
            for (int i = 0; i < SIDE_LEN; i++)
            {
                vnrm = Vector3d.Lerp(Nwc, Swc, INTERP * (double)i).Normalized();
                mesh_data_ptr.vertex[data_size + i] = (Vector3)((vnrm * PlanetRadius - vnrm * SkirtDepth) - Position);
                mesh_data_ptr.texcoord[data_size + i] = (Vector3)vnrm;
            }
            data_size += SIDE_LEN;
            for (int i = 0; i < SIDE_LEN; i++)
            {
                vnrm = Vector3d.Lerp(Swc, Sec, INTERP * (double)i).Normalized();
                mesh_data_ptr.vertex[data_size + i] = (Vector3)((vnrm * PlanetRadius - vnrm * SkirtDepth) - Position);
                mesh_data_ptr.texcoord[data_size + i] = (Vector3)vnrm;
            }
            data_size += SIDE_LEN;
            for (int i = 0; i < SIDE_LEN; i++)
            {
                vnrm = Vector3d.Lerp(Nec, Sec, INTERP * (double)i).Normalized();
                mesh_data_ptr.vertex[data_size + i] = (Vector3)((vnrm * PlanetRadius - vnrm * SkirtDepth) - Position);
                mesh_data_ptr.texcoord[data_size + i] = (Vector3)vnrm;
            }
            data_size += SIDE_LEN;
            for (int i = 0; i < SIDE_LEN; i++)
            {
                vnrm = Vector3d.Lerp(Nwc, Nec, INTERP * (double)i).Normalized();
                mesh_data_ptr.vertex[data_size + i] = (Vector3)((vnrm * PlanetRadius - vnrm * SkirtDepth) - Position);
                mesh_data_ptr.texcoord[data_size + i] = (Vector3)vnrm;
            }

            MeshData = mesh_data_ptr;
        }
#pragma warning disable 168
        public void MergeChildren()
        {
            try
            {
                //Dispose of all the children
                Nw.Dispose();
                Ne.Dispose();
                Sw.Dispose();
                Se.Dispose();

                Nw = null;
                Ne = null;
                Sw = null;
                Se = null;
            }
            catch (NullReferenceException e)
            {

            }
        }
#pragma warning restore 168
        public void Split()
        {
            //Calculate the centre of this patch
            Vector3d centre = (Nwc + Nec + Swc + Sec) * 0.25;

            Nw = new Patch(Nwc, (Nwc + Nec) * 0.5, (Nwc + Swc) * 0.5, centre, PlanetRadius, SideLength * 0.5, SkirtDepth, Integrator, Level + 1, Data);
            Ne = new Patch((Nwc + Nec) * 0.5, Nec, centre, (Nec + Sec) * 0.5, PlanetRadius, SideLength * 0.5, SkirtDepth, Integrator, Level + 1, Data);
            Sw = new Patch((Nwc + Swc) * 0.5, centre, Swc, (Swc + Sec) * 0.5, PlanetRadius, SideLength * 0.5, SkirtDepth, Integrator, Level + 1, Data);
            Se = new Patch(centre, (Nec + Sec) * 0.5, (Swc + Sec) * 0.5, Sec, PlanetRadius, SideLength * 0.5, SkirtDepth, Integrator, Level + 1, Data);
        }
        public void Subdivide()
        {
            Split();
            Nw.GenMeshData();
            Ne.GenMeshData();
            Sw.GenMeshData();
            Se.GenMeshData();
        }

        private bool PatchShouldSubdivide(Vector3d CamPos)
        {
            double dis = (VectorAddons.Distance(Position, CamPos) - SideLength);
            return SideLength >= (SIDE_LEN - 1) * PATCH_MULT && dis < SideLength * DIS_MULT;
        }
        private bool PatchShouldMerge(Vector3d CamPos)
        {
            double dis = (VectorAddons.Distance(Position, CamPos) - SideLength);
            return dis > SideLength * DIS_MULT;
        }

        public bool CheckAndSubdivide(Vector3d CamPos)
        {
            //Check whether this patch is subdivided
            if (IsSubdivided)
            {
                //Check whether this patch should merge it's children
                if (PatchShouldMerge(CamPos))
                {
                    //Put this patch in the list to be rendered
                    Integrator.AddPatch(this);
                    //Merge this patch's children
                    MergeChildren();

                    //Indicate that the mesh changed
                    return true;
                }
                else
                {
                    //Since this patch should not be merged, check if its children should be merged or subdivided
                    bool r1 = Nw.CheckAndSubdivide(CamPos);
                    bool r2 = Ne.CheckAndSubdivide(CamPos);
                    bool r3 = Sw.CheckAndSubdivide(CamPos);
                    bool r4 = Se.CheckAndSubdivide(CamPos);

                    //return whether a change occured in any of the sub patches
                    return r1 || r2 || r3 || r4;
                }
            }
            //The patch is not subdivided. Check whether the patch should be subdivided.
            else if (PatchShouldSubdivide(CamPos))
            {
                //Create the sub-patches, but don't generate any mesh data
                Split();

                MeshGeneratorThread.EnqueueTask(new Action(delegate
                {
                    //Generate mesh data
                    if (IsSubdivided)
                    {
                        Nw.GenMeshData();
                        Ne.GenMeshData();
                        Sw.GenMeshData();
                        Se.GenMeshData();
                    }

                    //Add the new patches to the Integrator
                    Integrator.CreatePatch(Nw);
                    Integrator.CreatePatch(Ne);
                    Integrator.CreatePatch(Sw);
                    Integrator.CreatePatch(Se);

                    //Remove the current patch from the Integrator
                    Integrator.RemovePatch(this);
                }));
            }

            //Nothing changed.
            return false;
        }
        public void ForceSubdivide(Vector3d CamPos)
        {
            //Check whether this patch is subdivided
            if (IsSubdivided)
            {
                //Check whether this patch should merge it's children
                if (PatchShouldMerge(CamPos))
                {
                    //Put this patch in the list to be rendered
                    Integrator.AddPatch(this);
                    //Merge the children
                    MergeChildren();

                    //Indicate that the mesh changed
                    return;
                }
                else
                {
                    //Since this patch should not be merged, check if its children should be merged or subdivided
                    Nw.ForceSubdivide(CamPos);
                    Ne.ForceSubdivide(CamPos);
                    Sw.ForceSubdivide(CamPos);
                    Se.ForceSubdivide(CamPos);

                    //return whether a change occured in any of the sub patches
                    return;
                }
            }
            //The patch is not subdivided. Check whether the patch should be subdivided.
            else if (PatchShouldSubdivide(CamPos))
            {
                //Create the sub-patches, but don't generate any mesh data
                Subdivide();

                Integrator.CreatePatch(Nw);
                Integrator.CreatePatch(Ne);
                Integrator.CreatePatch(Sw);
                Integrator.CreatePatch(Se);

                Integrator.RemovePatch(this);

                //Generate mesh data here
                Nw.ForceSubdivide(CamPos);
                Ne.ForceSubdivide(CamPos);
                Sw.ForceSubdivide(CamPos);
                Se.ForceSubdivide(CamPos);
            }

            //Nothing changed.
            return;
        }

        public void Dispose()
        {
            if (IsSubdivided)
            {
                Nw.Dispose();
                Ne.Dispose();
                Sw.Dispose();
                Se.Dispose();
            }

            Integrator.DeletePatch(this);
        }


        #endregion

        #endregion

    }
}
