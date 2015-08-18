using System;

namespace PlanetDevTest.Surface
{
    public interface LayerIntegrator
    {
        void CreatePatch(Patch p);
        void AddPatch(Patch p);
        void RemovePatch(Patch p);
        void DeletePatch(Patch p);

		void Update();
    }
}
