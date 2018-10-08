using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using System.Text;

namespace BioGait
{
    public class Coordinate
    {
        public double X;
        public double Y;
        public double Z;

        public Coordinate(SkeletonPoint point) {
            X = (double)point.X;
            Y = (double)point.Y;
            Z = (double)point.Z;
        }

        public Coordinate() {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Coordinate(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double magnitude(){
            double mag = Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2)+Math.Pow(Z,2));
            return mag;
        }
    }
}
