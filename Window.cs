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

using LibNoise;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace PlanetDevTest
{
	using Surface = PlanetLib.Surface;
	using N = LibNoise;

	class Window : GameWindow
	{
		Surface.PlanetMesh Planet = new Surface.PlanetMesh();
		Surface.NewIntegrator Integrator;
		ShaderRuntime.GLShader Shader = new Shaders.PlanetShader();	
		private bool IsLine = false;
		private PlanetLib.Frustum Frustum = new PlanetLib.Frustum();
		static INoiseModule Noise;

		private const double NearZ = 10;
		private const double FarZ = 100000000;
		private const double MaxD = 160000;

		private static double DispDel(Vector3d p)
		{
			Vector3d v = p * 0.000005;
			return (Noise.GetValue(v.X, v.Y, v.Z)) * MaxD;
			//return 0;
		}

		static readonly Vector3d DefaultPos = new Vector3d(0, 0, 6001000);

		Vector3d CamPos = DefaultPos;
		Quaterniond CamRot = Quaterniond.Identity;

		public Window()
			: base(1080, 720,
			new GraphicsMode(), "OpenGL 3 Example", 0,
			DisplayDevice.Default, 3, 2,
			GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
		{
			Noise = NoiseModules.MountainModule;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Shader.Compile();
			Integrator = new Surface.NewIntegrator(Shader, Planet);
			Planet.AddLayer(new Surface.SubdivisionMesh(6000000, 100, Integrator, DispDel));
			Planet.Layers[0].SkirtDepth = 10000;
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			GL.FrontFace(FrontFaceDirection.Ccw);
			GL.ClearColor(System.Drawing.Color.MidnightBlue);

			Shader.SetParameter("MaxDisplacement", (float)MaxD);
			Shader.SetParameter("LightDir", -Vector3.UnitX);

			Frustum.SetCamInternals(60, (double)Width / (double)Height, NearZ, System.Math.Min(FarZ, Planet.Layers[0].Radius * 0.5));
		}

		protected override void OnResize(EventArgs e)
		{
			Frustum.SetCamInternals(60, (double)Width / (double)Height, NearZ, FarZ);
		}
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			Integrator.Draw((Matrix4d.CreateFromQuaternion(CamRot) * Matrix4d.CreateTranslation(CamPos)).Inverted() * Matrix4d.CreatePerspectiveFieldOfView(1, (double)Width / (double)Height, NearZ, FarZ), Frustum);

			SwapBuffers();
		}
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			double displacement = 500;

			base.OnUpdateFrame(e);
			Planet.Update(CamPos);

			if (Focused)
			{
				var Keyboard = OpenTK.Input.Keyboard.GetState();

				Matrix4d rot = Matrix4d.CreateFromQuaternion(CamRot);

				Vector3d left = rot.Row0.Xyz;
				Vector3d up = rot.Row1.Xyz;
				Vector3d front = rot.Row2.Xyz; ;

				if (Keyboard[OpenTK.Input.Key.ShiftLeft])
					displacement *= 100;

				if (Keyboard[OpenTK.Input.Key.W])
					CamPos -= front * displacement;
				if (Keyboard[OpenTK.Input.Key.S])
					CamPos += front * displacement;
				if (Keyboard[OpenTK.Input.Key.D])
					CamPos += left * displacement;
				if (Keyboard[OpenTK.Input.Key.A])
					CamPos -= left * displacement;
				if (Keyboard[OpenTK.Input.Key.Q])
					CamPos -= up * displacement;
				if (Keyboard[OpenTK.Input.Key.E])
					CamPos += up * displacement;


				Vector3d NewCamRot = new Vector3d();
				if (Keyboard[OpenTK.Input.Key.I])
					NewCamRot.X -= 0.1;
				if (Keyboard[OpenTK.Input.Key.K])
					NewCamRot.X += 0.1;
				if (Keyboard[OpenTK.Input.Key.J])
					NewCamRot.Y -= 0.1;
				if (Keyboard[OpenTK.Input.Key.L])
					NewCamRot.Y += 0.1;
				if (Keyboard[OpenTK.Input.Key.U])
					NewCamRot.Z -= 0.1;
				if (Keyboard[OpenTK.Input.Key.O])
					NewCamRot.Z += 0.1;

				if (Keyboard[OpenTK.Input.Key.Space])
				{
					CamPos = DefaultPos;
					CamRot = Quaterniond.Identity;
				}

				if (Keyboard[OpenTK.Input.Key.R])
				{
					Shader.Recompile();
				}

				if (Keyboard[OpenTK.Input.Key.X])
				{
					if (IsLine)
					{
						GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
						IsLine = false;
					}
					else
					{
						GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
						IsLine = true;
					}
				}

				if (Keyboard[OpenTK.Input.Key.Z])
				{
					Integrator.Reset();
					Planet.Layers[0].RegeneratePlanet();
				}

				if (Keyboard[OpenTK.Input.Key.Escape])
				{
					Exit();
				}

				Frustum.SetCamDef(CamPos, CamPos + front, up);

				CamRot *= Quaterniond.FromMatrix(Matrix3d.CreateRotationX(NewCamRot.X) * Matrix3d.CreateRotationY(NewCamRot.Y) * Matrix3d.CreateRotationZ(NewCamRot.Z));
			}

		}

		protected override void OnClosed(EventArgs e)
		{
			base.Exit();
			Planet.Dispose();
		}
	}
}
