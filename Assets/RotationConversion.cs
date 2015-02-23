using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    class RotationConversion
    {
        // Maths comes from Python's Relax http://svn.gna.org/svn/relax/1.3/maths_fns/rotation_matrix.py

        static float EULER_EPSILON = 0.00005f;

        static public Rotation RotationMatrixToEulerZXY(CvMat R)
        {
            var i = 2;
            var j = 0; // EULER_NEXT[2]
            var k = 1; // EULER_NEXT[3]

            var cos_beta = Math.Sqrt(Math.Pow(R[i, i], 2) + Math.Pow(R[j, i], 2));

            double alpha, beta, gamma;
            if (cos_beta > EULER_EPSILON)
            {
                alpha = Math.Atan2(R[k, j], R[k, k]);
                beta = Math.Atan2(-R[k, i], cos_beta);
                gamma = Math.Atan2(R[j, i], R[i, i]);
            }
            else
            {
                alpha = Math.Atan2(-R[j, k], R[j, j]);
                beta = Math.Atan2(-R[k, i], cos_beta);
                gamma = 0.0;
            }

            alpha = wrap_angles(alpha, 0.0, 2.0 * Math.PI); // Z
            beta = wrap_angles(beta, 0.0, 2.0 * Math.PI); // X
            gamma = wrap_angles(gamma, 0.0, 2.0 * Math.PI); // Y

            // errr... 180 - Z result seems right. Why?
            return new Rotation(RadianToDegree(beta), RadianToDegree(gamma), 180.0 - RadianToDegree(alpha));
        }

        static double wrap_angles(double angle, double lower, double upper)
        {
            var window = 2 * Math.PI;
            if(window - (upper - lower) > 1e-7)
                throw new ArgumentException();

            while(true) {
                if(angle > upper)
                    angle = angle - window;
                else if (angle < lower)
                    angle = angle + window;
                else
                    break;
            }

            return angle;
        }

        private static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
  
    }
}
