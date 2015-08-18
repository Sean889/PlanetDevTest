using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace PlanetDevTest
{
    static class VectorAddons
    {
        public static double Distance(Vector3d v1, Vector3d v2)
        {
            return (v1 - v2).Length;
        }
    }
}
