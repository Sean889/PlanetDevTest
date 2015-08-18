using System;
using OpenTK;

namespace PlanetDevTest.Surface
{

	public class PlanetLayer : IDisposable
	{
		/// <summary>
		/// The mesh.
		/// </summary>
		public SubdivisionMesh Mesh;
		/// <summary>
		/// The integrator.
		/// </summary>
		public LayerIntegrator Integrator;

		/// <summary>
		/// The default displacement delegate that will be used if one isn't provided.
		/// </summary>
		/// <returns> 0. .</returns>
		/// <param name="Vec"> Input position, this is ignored.</param>
		public static double DefaultDisplacementDelegate(Vector3d Vec)
		{
			return 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnityPlanet.PlanetLayer"/> class.
		/// </summary>
		/// <param name="Integrator">Intgrator.</param>
		/// <param name="Radius">Planet radius.</param>
		/// <param name="SkirtDepth">Skirt depth.</param>
		public PlanetLayer (LayerIntegrator Integrator, double Radius, double SkirtDepth)
		{
			this.Integrator = Integrator;
			Mesh = new SubdivisionMesh (Radius, SkirtDepth, Integrator, DefaultDisplacementDelegate);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="UnityPlanet.PlanetLayer"/> class.
		/// </summary>
		/// <param name="Integrator">Intgrator.</param>
		/// <param name="Radius">Planet radius.</param>
		/// <param name="SkirtDepth">Skirt depth.</param>
		/// <param name="Del"> A delegate that returns a value to displace the mesh by.</param>
		public PlanetLayer(LayerIntegrator Integrator, double Radius, double SkirtDepth, PatchData.DisplacementDelegate Del)
		{
			this.Integrator = Integrator;
			Mesh = new SubdivisionMesh (Radius, SkirtDepth, Integrator, Del);
		}

		/// <summary>
		/// Subdivides the mesh then updates the Integrator.
		/// </summary>
		/// <param name="CameraPosition">Camera position.</param>
		public void Update(Vector3d CameraPosition)
		{
			Mesh.CheckAndSubdivide (CameraPosition);
			Integrator.Update ();
		}

		/// <summary>
		/// Releases all resource used by the <see cref="UnityPlanet.PlanetLayer"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="UnityPlanet.PlanetLayer"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="UnityPlanet.PlanetLayer"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="UnityPlanet.PlanetLayer"/> so the garbage
		/// collector can reclaim the memory that the <see cref="UnityPlanet.PlanetLayer"/> was occupying.</remarks>
		public void Dispose()
		{
			Mesh.Dispose ();
		}
	}
}

