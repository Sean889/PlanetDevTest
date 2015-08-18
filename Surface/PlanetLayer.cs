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

