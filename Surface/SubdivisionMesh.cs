using System;
using PlanetDevTest;
using OpenTK;

namespace PlanetDevTest.Surface
{
    using dvec3 = Vector3d;

    /// <summary>
    /// A layer of the planet. One LOD mesh.
    /// </summary>
    public class SubdivisionMesh : IDisposable
    {
        #region fields
        private Patch[] sides = new Patch[6];

        private double PlanetRadiusInternal;
        private double SkirtDepthInternal;

        /// <summary>
        /// The current LayerIntegrator for the Layer.
        /// </summary>
        public readonly LayerIntegrator Integrator;

		public PatchData Data;
        #endregion

        #region properties
        /// <summary>
        /// Gets or sets the radius of the planet layer.
        /// Changing this value requires regenerating the entire planet.
        /// </summary>
        public double Radius
        {
            get 
            { 
                return PlanetRadiusInternal; 
            }
            set 
            { 
                PlanetRadiusInternal = value;
                RegeneratePlanet();
            }
        }
        /// <summary>
        /// Gets or sets the patch skirt depth of the planet layer.
        /// Changing this value requires regenerating the entire planet.
        /// </summary>
        public double SkirtDepth
        {
            get
            {
                return SkirtDepthInternal;
            }
            set
            {
                SkirtDepthInternal = value;
                RegeneratePlanet();
            }
        }
        #endregion

        #region methods
        /// <summary>
        /// Gets the specified side.
        /// </summary>
        /// <param name="side"> The index of the side. 0 = top, 1 = bottom, 2 = front, 3 = back, 4 = right, 5 = left. </param>
        /// <returns> The patch on the specified side. </returns>
        public Patch GetSide(int side)
        {
            return sides[side];
        }

        /// <summary>
        /// Resets the planet to the default mesh using the current parameters.
        /// </summary>
        public void RegeneratePlanet()
        {
            double radius = PlanetRadiusInternal;
            if(sides[0] != null)
            {
                //Dispose of existing sides
                sides[0].Dispose();
                sides[1].Dispose();
                sides[2].Dispose();
                sides[3].Dispose();
                sides[4].Dispose();
                sides[5].Dispose();
            }

            //Create the sides, the next subdivision operation will subdivide the mesh
            sides[0] = new Patch(new dvec3(radius, radius, -radius),    new dvec3(-radius, radius, -radius),    new dvec3(radius, radius, radius),      new dvec3(-radius, radius, radius),     radius, radius * 2, SkirtDepthInternal, Integrator, 0, Data);
			sides[1] = new Patch(new dvec3(-radius, -radius, radius),   new dvec3(-radius, -radius, -radius),   new dvec3(radius, -radius, radius),     new dvec3(radius, -radius, -radius),    radius, radius * 2, SkirtDepthInternal, Integrator, 0, Data);
			sides[2] = new Patch(new dvec3(-radius, -radius, -radius),  new dvec3(-radius, radius, -radius),    new dvec3(radius, -radius, -radius),    new dvec3(radius, radius, -radius),     radius, radius * 2, SkirtDepthInternal, Integrator, 0, Data);
			sides[3] = new Patch(new dvec3(radius, radius, radius),     new dvec3(-radius, radius, radius),     new dvec3(radius, -radius, radius),     new dvec3(-radius, -radius, radius),    radius, radius * 2, SkirtDepthInternal, Integrator, 0, Data);
			sides[4] = new Patch(new dvec3(-radius, radius, radius),    new dvec3(-radius, radius, -radius),    new dvec3(-radius, -radius, radius),    new dvec3(-radius, -radius, -radius),   radius, radius * 2, SkirtDepthInternal, Integrator, 0, Data);
			sides[5] = new Patch(new dvec3(radius, -radius, -radius),   new dvec3(radius, radius, -radius),     new dvec3(radius, -radius, radius),     new dvec3(radius, radius, radius),      radius, radius * 2, SkirtDepthInternal, Integrator, 0, Data);
        
            for(int i = 0; i < 6; i++)
            {
                sides[i].GenMeshData();
                Integrator.CreatePatch(sides[i]);
            }
        }

        /// <summary>
        /// Runs a check and subdivide on each of the sides.
        /// </summary>
        /// <param name="RelCamPos"> The current position of the camera relative to the plante. </param>
        /// <returns> Whether a change occured on any of the sides. </returns>
        public bool CheckAndSubdivide(Vector3d RelCamPos)
        {
            bool v = false;
            for(int i = 0; i < 6; i++)
            {
                v = v || sides[i].CheckAndSubdivide(RelCamPos);
            }
            return v;
        }

        /// <summary>
        /// Immediately forces the planet to the correct subdivision level.
        /// This function will execute all mesh generation on the current thread.
        /// If the mesh does not need to be immediately at the correct level use CheckAndSubdivide instead.
        /// </summary>
        /// <param name="RelCamPos"> The current camera position relative to the planet. </param>
        public void ForceSubdivide(Vector3d RelCamPos)
        {
            for(int i = 0; i < 6; i++)
            {
                sides[i].ForceSubdivide(RelCamPos);
            }
        }

        /// <summary>
        /// Constructs the planet layer.
        /// </summary>
        /// <param name="Radius"> The radius of the layer. </param>
        /// <param name="SkirtDepth"> The skirt depth of the patches in the layer. </param>
        /// <param name="Integrator"> The LayerIntegrator used for the patches. </param>
		public SubdivisionMesh(double Radius, double SkirtDepth, LayerIntegrator Integrator, PatchData.DisplacementDelegate Del)
        {
            this.PlanetRadiusInternal = Radius;
            this.SkirtDepthInternal = SkirtDepth;
            this.Integrator = Integrator;
			this.Data = new PatchData ();

			Data.Del = Del;

            MeshGeneratorThread.Init();

            RegeneratePlanet();
        }

        public void Dispose()
        {
            //Dispose of sides
            sides[0].Dispose();
            sides[1].Dispose();
            sides[2].Dispose();
            sides[3].Dispose();
            sides[4].Dispose();
            sides[5].Dispose();

            //Destroy executor thread
            MeshGeneratorThread.Deinit();
        }
        #endregion
    }
}
