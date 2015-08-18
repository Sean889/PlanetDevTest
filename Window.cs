using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using LibNoise;

namespace PlanetDevTest
{
    class Window : GameWindow
    {
        Surface.Planet Planet = new Surface.Planet();
        Surface.PlanetSurfaceIntegrator Integrator;
        ShaderRuntime.GLShader Shader = new Shaders.PlanetShader();
        static IModule Noise = new RidgedMultifractal();

        private static double DispDel(Vector3d p)
        {
            Vector3d v = p.Normalized();
            double val = (Noise.GetValue(v.X, v.Y, v.Z) + 1.0) * 64;
            return val;
        }

        Vector3d CamPos = new Vector3d(0, 0, 10);
        Quaterniond CamRot = Quaterniond.Identity;

        public Window()
            : base(1080, 720,
            new GraphicsMode(), "OpenGL 3 Example", 0,
            DisplayDevice.Default, 3, 2,
            GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Shader.Compile();
            Integrator = new Surface.PlanetSurfaceIntegrator(Shader, Planet);
            Planet.MainLayer = new Surface.PlanetLayer(Integrator, 1000, 100, DispDel);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.ClearColor(System.Drawing.Color.MidnightBlue);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Integrator.Update();
            Integrator.Draw((Matrix4d.CreateFromQuaternion(CamRot) * Matrix4d.CreateTranslation(CamPos)).Inverted() * Matrix4d.CreatePerspectiveFieldOfView(1, (double)Width / (double)Height, 1, 10000.0));

            SwapBuffers();
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double displacement = 5;

            base.OnUpdateFrame(e);

            Planet.MainLayer.Update(CamPos);

            var Keyboard = OpenTK.Input.Keyboard.GetState();

            Matrix4d rot = Matrix4d.CreateFromQuaternion(CamRot);

            Vector3d left = rot.Row0.Xyz;
            Vector3d up = rot.Row1.Xyz;
            Vector3d front = rot.Row2.Xyz; ;

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

            if(Keyboard[OpenTK.Input.Key.Space])
            {
                CamPos = Vector3d.Zero;
                CamRot = Quaterniond.Identity;
            }

            CamRot *= Quaterniond.FromMatrix(Matrix3d.CreateRotationX(NewCamRot.X) * Matrix3d.CreateRotationY(NewCamRot.Y) * Matrix3d.CreateRotationZ(NewCamRot.Z));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.Exit();
            Planet.MainLayer.Dispose();
        }
    }
}
