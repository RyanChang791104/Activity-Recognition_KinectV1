using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using BioGait;
using FUKinectTool;
namespace WpfApplication1
{
    class Calculation
    {
        public Coordinate gravity = new Coordinate(0, -9.8, 0);
        public double getAngle(SkeletonPoint b1, SkeletonPoint b2, SkeletonPoint b3)
        {
            double delxVec1 = b2.X - b1.X;
            double delyVec1 = b2.Y - b1.Y;
            double delzVec1 = b2.Z - b1.Z;

            double delxVec2 = b3.X - b2.X;
            double delyVec2 = b3.Y - b2.Y;
            double delzVec2 = b3.Z - b2.Z;

            double angle = getTheta(delxVec1, delyVec1, delzVec1, delxVec2, delyVec2, delzVec2);
            return angle;
        }
        public double getTheta(double delxVec1, double delyVec1, double delzVec1, double delxVec2, double delyVec2, double delzVec2)
        {

            double dotPrdct = delxVec1 * delxVec2 + delyVec1 * delyVec2 + delzVec1 * delzVec2;
            double magnVec1 = Math.Sqrt(delxVec1 * delxVec1 + delyVec1 * delyVec1 + delzVec1 * delzVec1);
            double magnVec2 = Math.Sqrt(delxVec2 * delxVec2 + delyVec2 * delyVec2 + delzVec2 * delzVec2);

            double Theta = Math.Acos(dotPrdct / (magnVec1 * magnVec2)) * 180 / Math.PI;
            return Theta;
        }
        public Coordinate getVelocity(Coordinate current, DateTime timeStampCurrent, Coordinate prev, DateTime timeStampPrev)
        {
            double delTime = timeStampCurrent.Subtract(timeStampPrev).TotalSeconds;

            Coordinate vel = new Coordinate();
            vel.X = (current.X - prev.X) / (float)delTime;
            vel.Y = (current.Y - prev.Y) / (float)delTime;
            vel.Z = (current.Z - prev.Z) / (float)delTime;
            return vel;
        }
        public Coordinate getAcceleration(Coordinate current, Coordinate prev, Coordinate next, DateTime timeStampCurrent, DateTime timeStampPrev, DateTime timeStampNext)
        {
            double delTime1 = timeStampCurrent.Subtract(timeStampPrev).TotalSeconds;
            double delTime2 = timeStampNext.Subtract(timeStampCurrent).TotalSeconds;

            Coordinate acc = new Coordinate();
            acc.X = (2 * ((prev.X * (float)delTime2) - ((float)(delTime1 + delTime2) * current.X) + (next.X * (float)delTime1))) / (float)(delTime1 * delTime2 * (delTime1 + delTime2));
            acc.Y = (2 * ((prev.Y * (float)delTime2) - ((float)(delTime1 + delTime2) * current.Y) + (next.Y * (float)delTime1))) / (float)(delTime1 * delTime2 * (delTime1 + delTime2));
            acc.Z = (2 * ((prev.Z * (float)delTime2) - ((float)(delTime1 + delTime2) * current.Z) + (next.Z * (float)delTime1))) / (float)(delTime1 * delTime2 * (delTime1 + delTime2));
            return acc;
        }
        public Coordinate getGRF(Coordinate acceleration, double mass)
        {

            Coordinate GRF = new Coordinate();
            GRF.X = mass * (acceleration.X - gravity.X);
            GRF.Y = mass * (acceleration.Y - gravity.Y);
            GRF.Z = mass * (acceleration.Z - gravity.Z);
            return GRF;
        }

        public Coordinate center_mass(SkeletonPoint b1, SkeletonPoint b2, SkeletonPoint b3, SkeletonPoint b4, SkeletonPoint b5, SkeletonPoint b6, SkeletonPoint b7)
        {
           
            ///Alternative method to calculate center of mass
            /* float delxVec1 = (b1.X + b2.X + b3.X + b4.X + b5.X + b6.X + b7.X + b8.X + b9.X+ b10.X + b11.X+ b12.X + b13.X + b14.X + b15.X + b16.X + b17.X) / 17;
             float delyVec1 = (b1.Y + b2.Y + b3.Y + b4.Y + b5.Y + b6.Y + b7.Y + b8.Y + b9.Y + b10.Y + b11.Y + b12.Y + b13.Y + b14.Y + b15.Y + b16.Y + b17.Y)/17;
             float delzVec1 = (b1.Z + b2.Z + b3.Z + b4.Z + b5.Z + b6.Z + b7.Z + b8.Z + b9.Z + b10.Z + b11.Z + b12.Z + b13.Z + b14.Z + b15.Z + b16.Z + b17.Z)/17;
             */

            Coordinate c = new Coordinate();
            /*
                        c.X = delxVec1;
                        c.Y = delyVec1;
                        c.Z = delzVec1;
            */
            c.X = b7.X;
            c.Y = b7.Y;
            c.Z = b7.Z;
            return c;
        }
    }
}
