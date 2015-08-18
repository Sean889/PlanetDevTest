using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace PlanetDevTest.Surface
{
    public class Planet
    {
        public PlanetLayer MainLayer;

        public Vector3d Position;
        public Quaterniond Rotation = new Quaterniond(0, 0, 0, 1);
    }
}
